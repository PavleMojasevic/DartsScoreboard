using IndexedDB.Blazor;

namespace DartsScoreboard;

public static class ServiceExtensions
{
    public static void AddDartsScoreboardServices(this IServiceCollection services)
    {
        services.AddTransient<IUserPersistence, UserPersistence>();
        services.AddSingleton<IIndexedDbFactory, IndexedDbFactory>();
    }
}
