namespace Chik.Exams;

public class InvalidJwtException : Exception
{
    public InvalidJwtException(string message) : base(message)
    {
    }

    public InvalidJwtException(string message, Exception innerException) : base(message, innerException)
    {
    }
}