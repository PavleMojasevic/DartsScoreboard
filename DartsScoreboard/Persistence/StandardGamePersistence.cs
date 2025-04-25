using IndexedDB.Blazor;

namespace DartsScoreboard;

public class StandardGamePersistence : IStandardGamePersistence
{
    private IIndexedDbFactory _DbFactory;

    public StandardGamePersistence(IIndexedDbFactory dbFactory)
    {
        _DbFactory = dbFactory;
    }
    public async Task Add(StandardGame entity)
    {
        using var db = await _DbFactory.Create<DartsScoreBoardDb>();
        db.StandardGames.Add(entity);
        await db.SaveChanges();
    }
    public async Task Remove(string code)
    {
        using var db = await _DbFactory.Create<DartsScoreBoardDb>();
        var entity = db.StandardGames.FirstOrDefault(x => x.Code == code);
        if (entity == null)
            return;
        db.StandardGames.Remove(entity);
        await db.SaveChanges();
    }
    public async Task<StandardGame?> Get(string code)
    {
        using var db = await _DbFactory.Create<DartsScoreBoardDb>();
        return db.StandardGames.FirstOrDefault(x => x.Code == code);
    }
    public async Task<List<StandardGame>> GetAll()
    {
        using var db = await _DbFactory.Create<DartsScoreBoardDb>();
        return db.StandardGames.ToList();
    }
}
