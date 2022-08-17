using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Models;

namespace Client.ViewModels
{
    public class UserViewModel : IUserViewModel
    {
        private HttpClient _httpClient;

        public UserViewModel()
        {

        }
        public UserViewModel(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public User User { get; set; }

        public async Task NewUser()
        {
            await _httpClient.PostAsJsonAsync<User>("users/new", User);
        }

        public async Task<string> AuthenticateJWT()
        {
            //creating authentication request
            AuthenticationUser authenticationUser = new AuthenticationUser();
            authenticationUser.Email = this.User.Email;
            authenticationUser.Password = this.User.Password;

            //authenticating the request
            var response = await _httpClient.PostAsJsonAsync<AuthenticationUser>($"users/authenticatejwt", authenticationUser);

            //sending the token to the client to store
            authenticationUser = await response.Content.ReadFromJsonAsync<AuthenticationUser>();
            return authenticationUser.Token;
        }

        public async Task<User> GetUserByJWTAsync(string jwtToken)
        {
            //preparing the http request
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, "users/getuserbyjwt");
            requestMessage.Content = new StringContent(jwtToken);

            requestMessage.Content.Headers.ContentType
                = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

            //making the http request
            var response = await _httpClient.SendAsync(requestMessage);

            var responseStatusCode = response.StatusCode;
            var returnedUser = await response.Content.ReadFromJsonAsync<User>();

            //returning the user if found
            if (returnedUser != null) return await Task.FromResult(returnedUser);
            else return null;
        }

        public async Task EditUser()
        {
            await _httpClient.PutAsJsonAsync("users", User);
        }

    }
}
