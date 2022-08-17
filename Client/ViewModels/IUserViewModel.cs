using System.Threading.Tasks;
using Models;

namespace Client.ViewModels
{
    public interface IUserViewModel
    {
        public User User { get; set; }
        public Task NewUser();
        public Task<User> GetUserByJWTAsync(string jwtToken);
        public Task<string> AuthenticateJWT();
        public Task EditUser();
    }
}
