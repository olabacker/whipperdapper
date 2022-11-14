using Microsoft.Extensions.DependencyInjection;

namespace WhipperDapper;
public static class WhipperDapperExtensions
{
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
