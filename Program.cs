using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;

namespace Stoplicht
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string apiKey = args[1];
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(
                            Encoding.ASCII.GetBytes(string.Format("{0}:{1}", "", apiKey))));
            var timer = new Timer(
                async e => await UpdateBuildStatus(client, args[0]),
                null,
                TimeSpan.Zero,
                TimeSpan.FromMinutes(1));
            Console.ReadLine();
        }

        public static async Task UpdateBuildStatus(HttpClient client, string url)
        {
            HttpResponseMessage response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();

            var responseDynamic = JsonSerializer.Deserialize<dynamic>(responseBody);
            UpdateLight(responseDynamic?.GetProperty("result").GetString());
        }

        public static void UpdateLight(string status)
        {
            Process p = new Process();
            ProcessStartInfo psi = new ProcessStartInfo()
            {
                FileName = Path.Combine(Directory.GetCurrentDirectory(), "USBswitchCmd.exe"),
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true
            };

            switch (status)
            {
                case "succeeded":
                    psi.Arguments = "G";
                    break;
                case "failed":
                    psi.Arguments = "R";
                    break;
                default:
                    psi.Arguments = "Y";
                    break;
            }
            p.StartInfo = psi;
            p.Start();
        }
    }
}