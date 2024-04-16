using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using System.Data;
using Dapper;
using PractihubAPI.Models.Opinion;
using System.Threading.Tasks;
using PractihubAPI.Models.Authenticate;
using PractihubAPI.Models;

namespace PractihubAPI.Persistance.Authenticate
{
    public class UserHandler:IUserHandler
    {
        private readonly IConfiguration configuration;

        public UserHandler(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public async Task<User> FindUser(DynamicParameters parameters)
        {
            using IDbConnection connection = Connection;

            string query = @"SELECT * FROM [User] WHERE Email = @Email";

            var response = await connection.QueryFirstOrDefaultAsync<User>(query, parameters, commandTimeout: 0).ConfigureAwait(false);

            return response;
        }


        public async Task<CustomResponse> RegisterUser(DynamicParameters parameters)
        {
            using IDbConnection connection = Connection;

            await connection.QueryMultipleAsync("InsertUser", parameters, commandTimeout: 0, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
           

            return new CustomResponse()
            {
                Status = parameters.Get<string>("MessageResponse") == "successfully" ? true : false,
                Message = parameters.Get<string>("MessageResponse") != "successfully" ? "error" : "successfully"
            };
        }

        public IDbConnection Connection
        {
            get
            {
                return new SqlConnection(configuration.GetConnectionString("DBConnection"));
            }
        }
    }
}
