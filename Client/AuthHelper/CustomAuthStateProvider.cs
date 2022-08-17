using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Authorization;
using Blazored.LocalStorage;
using Models;
using Client.ViewModels;

namespace Client
{
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        private readonly ILocalStorageService _localStorage;
        private readonly IUserViewModel _userViewModel;
        public CustomAuthStateProvider(IUserViewModel userViewModel, ILocalStorageService localStorage)
        {
            _userViewModel = userViewModel;
            _localStorage = localStorage;

        }
        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            User currentUser = await GetUserByJWTAsync();
            
            if (currentUser != null && currentUser.Email != null)
            {
                //create a claims
                var claimEmailAddress = new Claim(ClaimTypes.Name, currentUser.Email);
                var claimNameIdentifier = new Claim(ClaimTypes.NameIdentifier, Convert.ToString(currentUser.Id));
                var claimRole = new Claim(ClaimTypes.Role, currentUser.Role.ToString() == null ? "" : currentUser.Role.ToString());
            
                //create claimsIdentity
                var claimsIdentity = new ClaimsIdentity(new[] { claimEmailAddress, claimNameIdentifier, claimRole }, "serverAuth");
                //create claimsPrincipal
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
                return new AuthenticationState(claimsPrincipal);
            }
            else
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));

           
        }

        public async Task<User> GetUserByJWTAsync()
        {
            //pulling the token from localStorage
            var jwtToken = await _localStorage.GetItemAsStringAsync("jwt_token");
            Console.WriteLine(jwtToken);
            if (jwtToken == null) return null;

            return await _userViewModel.GetUserByJWTAsync(jwtToken);
        }
    }
}

