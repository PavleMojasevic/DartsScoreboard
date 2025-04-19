using IndexedDB.Blazor;
using System;

namespace DartsScoreboard;

public class UserPersistence : IUserPersistence
{
    private IIndexedDbFactory _DbFactory;

    public UserPersistence(IIndexedDbFactory dbFactory)
    {
        _DbFactory = dbFactory;
    }
    public async Task AddUser(User user)
    {
        using var db = await _DbFactory.Create<DartsScoreBoardDb>();
        db.Users.Add(user);
        await db.SaveChanges();
    }
    public async Task RemoveUser(int id)
    {
        using var db = await _DbFactory.Create<DartsScoreBoardDb>();
        var user = db.Users.FirstOrDefault(x => x.Id == id);
        if (user == null)
            return;
        db.Users.Remove(user);
        await db.SaveChanges();
    }
    public async Task<User?> GetUser(int id)
    {
        using var db = await _DbFactory.Create<DartsScoreBoardDb>();
        return db.Users.FirstOrDefault(x => x.Id == id);
    }
    public async Task<List<User>> GetAllUsers()
    {
        using var db = await _DbFactory.Create<DartsScoreBoardDb>();
        return db.Users.ToList();
    }
    public async Task UpdateUser(User user)
    {
        using var db = await _DbFactory.Create<DartsScoreBoardDb>();
        var dbUser = db.Users.FirstOrDefault(x => x.Id == user.Id);
        if (dbUser == null)
            return;
        dbUser.Name = user.Name;
        await db.SaveChanges();
    }
}
