using System.Configuration;

namespace Services.Infrastructure.RestConfiguration
{
    public class RestConfiguration : IRestConfiguration
    {
        public string RestApi { get; private set; }

        public static RestConfiguration FromConfig()
        {
            return new RestConfiguration
            {
                RestApi = ConfigurationManager.AppSettings["RestConfiguration.RestApi"]
            };
        }
    }
}