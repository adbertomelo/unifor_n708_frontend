using System.Net;
using System.Web.Configuration;

namespace Bolao10.Services
{
    public class ApiService
    {

        string _baseUrl = "";

        public ApiService()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            _baseUrl = WebConfigurationManager.AppSettings["ApiBaseUrl"];
        }

        public string GetUrlBase()
        {
            return _baseUrl;
        }
    }
}