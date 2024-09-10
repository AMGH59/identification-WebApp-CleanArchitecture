using Serilog;
using Serilog.Sinks.Elasticsearch;

namespace IdentificationService.API.Extensions
{
    public static class ElasticsearchExtensions
    {
        public static void ConfigureElasticsearch(this IHostBuilder hostBuilder, IConfiguration configuration)
        {
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(configuration["Elasticsearch:Uri"]))
                {
                    AutoRegisterTemplate = true,
                })
                .CreateLogger();

            hostBuilder.UseSerilog();
        }
    }
}
