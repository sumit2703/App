using System.Threading.Tasks;
using DatingApp.API.Properties.Models;

namespace DatingApp.API.Controllers.Data
{
    public interface IAuthRepository
    {
         Task<user> Register(user user,string password);
         Task<user> Login(string username,string password);

         Task<bool> UserExists(string username); 


    }
}