#!/bin/sh

# Export DB environment variables directly
export DB_HOST="$DB_HOST"
export DB_USER="$DB_USER"
export DB_PASSWORD="$DB_PASSWORD"
export DB_NAME="$DB_NAME"
export DB_DATABASE="$DB_DATABASE"
export DB_ROOT_PASSWORD="$DB_ROOT_PASSWORD"

# Add to envvars for apache2ctl compatibility
echo "export DB_HOST='$DB_HOST'" >> /etc/apache2/envvars
echo "export DB_USER='$DB_USER'" >> /etc/apache2/envvars
echo "export DB_PASSWORD='$DB_PASSWORD'" >> /etc/apache2/envvars
echo "export DB_NAME='$DB_NAME'" >> /etc/apache2/envvars
echo "export DB_DATABASE='$DB_DATABASE'" >> /etc/apache2/envvars
echo "export DB_ROOT_PASSWORD='$DB_ROOT_PASSWORD'" >> /etc/apache2/envvars

# Create PerlSetEnv config for mod_perl (must be set before modules load)
cat > /etc/apache2/conf-enabled/qst-env.conf << EOF
# Database environment variables for mod_perl
PerlSetEnv DB_HOST $DB_HOST
PerlSetEnv DB_USER $DB_USER
PerlSetEnv DB_PASSWORD $DB_PASSWORD
PerlSetEnv DB_NAME $DB_NAME
PerlSetEnv DB_DATABASE $DB_DATABASE
EOF

# Copy custom CSS if it exists
if [ -f /Styles/custom.css ]; then
    cp /Styles/custom.css /var/www/qst/custom.css
    echo "Custom CSS installed"
fi

# Enable mod_substitute for CSS injection
a2enmod substitute > /dev/null 2>&1 || true

# Configure Apache to inject custom CSS into all HTML responses
cat > /etc/apache2/conf-enabled/qst-css-inject.conf << 'EOF'
# Inject custom CSS into HTML responses
<Location />
    AddOutputFilterByType SUBSTITUTE text/html
    Substitute "s|</head>|<link rel=\"stylesheet\" href=\"/custom.css\" type=\"text/css\"></head>|i"
</Location>
EOF

# Update admin password using Perl's Crypt::PBKDF2 (same as QST uses)
perl -MCrypt::PBKDF2 -MDBI -e '
    my $pbkdf2 = Crypt::PBKDF2->new(
        hash_class => "HMACSHA2",
        hash_args => { sha_size => 256 },
        iterations => 1000,
        salt_len => 4,
    );
    my $hash = $pbkdf2->generate($ENV{DB_PASSWORD});
    my $dbh = DBI->connect(
        "DBI:mysql:database=$ENV{DB_NAME};host=$ENV{DB_HOST}",
        $ENV{DB_USER},
        $ENV{DB_PASSWORD},
        { RaiseError => 1 }
    );
    $dbh->do("UPDATE users SET pass = ? WHERE u_name = ?", undef, $hash, "admin");
    print "Admin password updated successfully\n";
' 2>/dev/null || echo "Note: Could not update admin password (DB may not be ready yet)"

# Source the full envvars (includes Apache defaults)
. /etc/apache2/envvars

# Start Apache
exec /usr/sbin/apache2 -D FOREGROUND
