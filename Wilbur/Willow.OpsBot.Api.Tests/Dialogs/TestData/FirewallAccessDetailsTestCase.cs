using Willow.OpsBot.Api.Dialogs;

namespace Willow.OpsBot.Api.Tests.Dialogs.TestData;

public record FirewallAccessDetailsTestCase(string Name, FirewallAccessDetails InitialData,
    List<UtteranceAndReply> UtterancesAndReplies, FirewallAccessDetails ExpectedResult);
