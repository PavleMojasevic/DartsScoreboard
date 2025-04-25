namespace DartsScoreboard
{
    public interface IStandardGamePersistence
    {
        Task Add(StandardGame entity);
        Task<StandardGame?> Get(string id);
        Task<List<StandardGame>> GetAll();
        Task Remove(string id);
    }
}
