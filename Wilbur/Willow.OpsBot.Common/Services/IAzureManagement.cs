using Willow.OpsBot.Common.Models.Enums;

namespace Willow.OpsBot.Common.Services;

public interface IAzureManagement
{
    Task<bool> AccessibleServer(ServerTypes type, string name);
    Task AddUserIpToServer(ServerTypes type, string name, string ip, string userName);
    Task RemoveTimeBasedFirewallRule(List<string>? serverNameExclusions);
}
