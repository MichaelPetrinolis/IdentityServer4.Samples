using Clients;
using IdentityModel;
using IdentityModel.Client;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace PhiloxenonDemoClientCredentialsFlow
{
    public class Program
    {
        public static void Main(string[] args)
        {
            do
            {
                try
                {
                    MainAsync().GetAwaiter().GetResult();
                }
                catch (Exception ex)
                {
                    ex.Message.ConsoleRed();
                }
                "Press space + enter to terminate".ConsoleYellow();
            } while (Console.ReadLine() != " ");
        }

        public static async Task MainAsync()
        {
            Console.Title = "Console Client Credentials Flow";

            var response = await RequestTokenAsync();
            response.Show();

            string tenant = "UNKNOWN";


            if (response.AccessToken.Contains("."))
            {
                var parts = response.AccessToken.Split('.');
                var header = parts[0];
                var claims = parts[1];
                var claimValues = JObject.Parse(Encoding.UTF8.GetString(Base64Url.Decode(claims)));
                tenant = claimValues.Value<string>("client_philoxenon_tenant");
            }
            "Press enter to call service".ConsoleYellow();
            Console.ReadLine();
            await CallServiceAsync(response.AccessToken, tenant);
        }

        static async Task<TokenResponse> RequestTokenAsync()
        {
            var client = new TokenClient(
                Constants.TokenEndpoint,
                "philoxenon.demo",
                "secret");

            return await client.RequestClientCredentialsAsync("philoxenon philoxenon.fullaccess");
        }

        static async Task CallServiceAsync(string token, string tenant)
        {
            var baseAddress = $"http://localhost:5412/api/{tenant}/";

            var client = new HttpClient
            {
                BaseAddress = new Uri(baseAddress)
            };

            client.SetBearerToken(token);
            var response = await client.GetStringAsync("rooms");

            "\n\\Rooms:".ConsoleGreen();
            Console.WriteLine(JArray.Parse(response));
        }
    }
}