namespace Chik.Exams;

public static class SesEmailHelper
{
    /// <summary>
    /// Checks if an email address is likely to be verified in AWS SES
    /// </summary>
    public static bool IsLikelyVerifiedEmail(string email)
    {
        return true;
        // Common test domains that are often verified
        // var verifiedDomains = new[]
        // {
        //     "amazon.com",
        //     "aws.amazon.com",
        //     "example.com",
        //     "test.com"
        // };

        // var domain = email.Split('@').LastOrDefault()?.ToLower();
        // return verifiedDomains.Contains(domain);
    }

    /// <summary>
    /// Provides guidance for AWS SES email verification issues
    /// </summary>
    public static void PrintVerificationGuidance(string email)
    {
        Console.WriteLine("=== AWS SES Email Verification Issue ===");
        Console.WriteLine($"Email: {email}");
        Console.WriteLine();
        Console.WriteLine("SOLUTIONS:");
        Console.WriteLine();
        Console.WriteLine("1. VERIFY THE EMAIL ADDRESS:");
        Console.WriteLine("   - Go to AWS SES Console");
        Console.WriteLine("   - Navigate to 'Verified identities'");
        Console.WriteLine("   - Click 'Create identity'");
        Console.WriteLine("   - Choose 'Email address' and enter: " + email);
        Console.WriteLine("   - Check the email and click the verification link");
        Console.WriteLine();
        Console.WriteLine("2. REQUEST PRODUCTION ACCESS:");
        Console.WriteLine("   - Go to AWS SES Console");
        Console.WriteLine("   - Navigate to 'Account dashboard'");
        Console.WriteLine("   - Click 'Request production access'");
        Console.WriteLine("   - Fill out the form and wait for approval");
        Console.WriteLine();
        Console.WriteLine("3. USE A VERIFIED EMAIL FOR TESTING:");
        Console.WriteLine("   - Use an email address you've already verified");
        Console.WriteLine("   - Or use a test domain like 'example.com'");
        Console.WriteLine();
        Console.WriteLine("4. CHECK SENDER VERIFICATION:");
        Console.WriteLine("   - Ensure 'support@mykeels.com' is verified");
        Console.WriteLine("   - Or verify your domain 'mykeels.com'");
        Console.WriteLine("=========================================");
    }

    /// <summary>
    /// Gets the AWS SES console URL for the specified region
    /// </summary>
    public static string GetSesConsoleUrl(string region = "us-east-1")
    {
        return $"https://{region}.console.aws.amazon.com/ses/home?region={region}#/verified-identities";
    }

    /// <summary>
    /// Validates email format
    /// </summary>
    public static bool IsValidEmailFormat(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}
