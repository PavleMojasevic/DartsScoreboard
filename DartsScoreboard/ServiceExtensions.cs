using IndexedDB.Blazor;

namespace DartsScoreboard;

public static class ServiceExtensions
{
    public static void AddDartsScoreboardServices(this IServiceCollection services)
    {
        services.AddTransient<IUserPersistence, UserPersistence>();
        services.AddTransient<ICricketPracticeGamePersistence, CricketPracticeGamePersistence>();
        services.AddSingleton<IIndexedDbFactory, IndexedDbFactory>();

        services.AddSingleton<PlayerSelectionService>();
    }
}
