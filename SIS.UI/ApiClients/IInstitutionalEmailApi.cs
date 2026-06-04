using Refit;

namespace SIS.UI.ApiClients
{
    public interface IInstitutionalEmailApi
    {
        [Get("/api/institutional-email/generate")]
        Task<GenerateEmailResponse> GenerateAsync([AliasAs("firstName")] string firstName,
                                                  [AliasAs("lastName")]  string lastName);
    }

    public class GenerateEmailResponse
    {
        public string Email { get; set; } = string.Empty;
    }
}
