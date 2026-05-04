using Scalar.AspNetCore;

namespace SIS.API.Extensions
{
    public static class ScalarExtensions
    {
        public static WebApplication UseScalar(this WebApplication app)
        {
            app.MapOpenApi();

            app.MapScalarApiReference(options =>
            {
                options.Title = "SIS API";
                options.Theme = ScalarTheme.DeepSpace;
                options.DefaultHttpClient = new(ScalarTarget.CSharp, ScalarClient.HttpClient);
            });

            return app;
        }
    }
}
