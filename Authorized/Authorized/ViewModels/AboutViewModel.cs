using IdentityModel.OidcClient;
using IdentityModel.OidcClient.Browser;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Authorized.ViewModels
{
    public partial class AboutViewModel : ObservableObject
    {
        public AboutViewModel()
        {
            Title = "About Time!";

            var browser = DependencyService.Get<IBrowser>();
            var options = new OidcClientOptions
            {
                Authority = "https://engineering.snow.edu/aspen/auth/realms/aspen",
                ClientId = "aspen-web",
                Scope = "profile email api-use",
                RedirectUri = "xamarinformsclients://callback",
                Browser = browser
            };

            _client = new OidcClient(options);
            _apiClient.Value.BaseAddress = new Uri("https://engineering.snow.edu/aspen/");

            OutputText = "Ready to go!";
        }


        OidcClient _client;
        LoginResult _result;
        Lazy<HttpClient> _apiClient = new Lazy<HttpClient>(() => new HttpClient());
        string accessToken;

        [ObservableProperty]
        private string title;

        [ObservableProperty]
        private string outputText;

        [ICommand]
        private async Task CopyJwt()
        {
            await Xamarin.Essentials.Clipboard.SetTextAsync(accessToken);
        }

        [ICommand]
        private async Task Login()
        {
            try
            {
                _result = await _client.LoginAsync(new LoginRequest());

                if (_result.IsError)
                {
                    OutputText = _result.Error;
                    return;
                }

                var sb = new StringBuilder(128);
                foreach (var claim in _result.User.Claims)
                {
                    sb.AppendFormat("{0}: {1}\n", claim.Type, claim.Value);
                }

                sb.AppendFormat("\n{0}: {1}\n", "refresh token", _result?.RefreshToken ?? "none");
                sb.AppendFormat("\n{0}: {1}\n", "access token", _result.AccessToken);
                accessToken = _result.AccessToken;

                OutputText = sb.ToString();

                _apiClient.Value.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _result?.AccessToken ?? "");
            }
            catch (Exception ex)
            {
                OutputText = ex.ToString();
            }
        }

        [ICommand]
        private async Task CallApi()
        {
            try
            {
                var result = await _apiClient.Value.GetAsync("api/admin");

                if (result.IsSuccessStatusCode)
                {
                    OutputText = JsonDocument.Parse(await result.Content.ReadAsStringAsync()).RootElement.GetRawText();
                }
                else
                {
                    OutputText = result.ToString();
                }
            }
            catch (Exception ex)
            {
                OutputText = ex.ToString();
            }
        }
    }
}