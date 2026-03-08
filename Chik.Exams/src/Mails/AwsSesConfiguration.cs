namespace Chik.Exams;

public static class AwsSesConfiguration
{
    public static class SmtpEndpoints
    {
        // AWS SES SMTP endpoints by region
        public const string UsEast1 = "email-smtp.us-east-1.amazonaws.com";
        public const string UsWest2 = "email-smtp.us-west-2.amazonaws.com";
        public const string EuWest1 = "email-smtp.eu-west-1.amazonaws.com";
        public const string ApSoutheast1 = "email-smtp.ap-southeast-1.amazonaws.com";
        public const string ApNortheast1 = "email-smtp.ap-northeast-1.amazonaws.com";
    }

    public static class SmtpPorts
    {
        public const int StartTls = 587;  // STARTTLS
        public const int Ssl = 2465;       // SSL/TLS
    }

    public static (string Host, int Port) GetSmtpSettings(string region = "us-east-1", bool useSsl = false)
    {
        var host = region.ToLower() switch
        {
            "us-east-1" => SmtpEndpoints.UsEast1,
            "us-west-2" => SmtpEndpoints.UsWest2,
            "eu-west-1" => SmtpEndpoints.EuWest1,
            "ap-southeast-1" => SmtpEndpoints.ApSoutheast1,
            "ap-northeast-1" => SmtpEndpoints.ApNortheast1,
            _ => SmtpEndpoints.UsEast1 // Default to us-east-1
        };

        var port = useSsl ? SmtpPorts.Ssl : SmtpPorts.StartTls;

        return (host, port);
    }
}
