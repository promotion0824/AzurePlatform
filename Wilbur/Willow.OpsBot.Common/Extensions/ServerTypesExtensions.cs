using Willow.OpsBot.Common.Models.Enums;

namespace Willow.OpsBot.Common.Extensions;

public static class ServerTypesExtensions
{
    public static string GetAzureType(this ServerTypes type)
    {
        return type switch
        {
            ServerTypes.PostgreSql => "microsoft.dbforpostgresql/servers",
            ServerTypes.SqlServer => "microsoft.sql/servers",
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }

    public static ServerTypes GetServerType(this string type)
    {
        return type switch
        {
            "microsoft.dbforpostgresql/servers" => ServerTypes.PostgreSql,
            "microsoft.sql/servers" => ServerTypes.SqlServer,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
}
