
namespace DartsScoreboard
{
    public interface IUserPersistence
    {
        Task AddUser(User user);
        Task<List<User>> GetAllUsers();
        Task<User?> GetUser(int id);
        Task RemoveUser(int id);
        Task UpdateUser(User user);
    }
}