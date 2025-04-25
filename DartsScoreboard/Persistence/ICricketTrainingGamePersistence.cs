namespace DartsScoreboard
{
    public interface ICricketPracticeGamePersistence
    {
        Task Add(CricketPracticeGame entity);
        Task<CricketPracticeGame?> Get(string id);
        Task<List<CricketPracticeGame>> GetAll();
        Task Remove(string id);
    }
}