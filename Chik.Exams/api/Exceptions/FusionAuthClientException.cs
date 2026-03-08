using io.fusionauth;

namespace Chik.Exams.Api;

public class FusionAuthClientException : Exception
{
    public FusionAuthClientException(string message, com.inversoft.error.Errors? errors) : base($"{message}: {(errors is not null ? string.Join(
                "\n",
                new List<string>()
                .Concat((errors.fieldErrors ?? new Dictionary<string, List<com.inversoft.error.Error>>()).Select(e => $"{e.Key}: {e.Value}"))
                .Concat((errors.generalErrors ?? []).Select(e => $"{e.code}: {e.message}, {e.data}"))
            ) : "Empty error response")}")
    {
    }
}