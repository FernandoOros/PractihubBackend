using Dapper;
using PractihubAPI.Models;
using PractihubAPI.Models.Authenticate;
using System.Threading.Tasks;

namespace PractihubAPI.Persistance.Authenticate
{
    public interface IUserHandler
    {
        public Task<User> FindUser(DynamicParameters parameters);
        public Task<CustomResponse> RegisterUser(DynamicParameters parameters);
    }
}
