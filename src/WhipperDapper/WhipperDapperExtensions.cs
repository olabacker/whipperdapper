using Microsoft.Extensions.DependencyInjection;

namespace WhipperDapper;
public static class WhipperDapperExtensions
{
    /// <summary>
    /// This will make the <see cref="DapperService" /> available from the service provider. 
    /// </summary>
    /// <param name="this"></param>
    /// <param name="dapperSettings"></param>
    public static void AddWhipperDapper(this IServiceCollection @this, DapperSettings dapperSettings)
    {
        @this.AddSingleton(dapperSettings);
        @this.AddScoped<DapperService>();
    }

    public static void AddWhipperDapper(this IServiceCollection @this, string connectionString)
    {
        AddWhipperDapper(@this, new DapperSettings(connectionString));
    }
}
