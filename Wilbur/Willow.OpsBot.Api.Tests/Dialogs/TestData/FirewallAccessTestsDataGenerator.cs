// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder.Testing.XUnit;
using Willow.OpsBot.Api.Dialogs;
using Willow.OpsBot.Api.Tests.Builders;
using Willow.OpsBot.Common.Models.Enums;

namespace Willow.OpsBot.Api.Tests.Dialogs.TestData;

public class FirewallAccessTestsDataGenerator
{
    public const string OauthCardSnapshotName = "oauth-prompt";
    public const string IpCardSnapshotName = "ip-prompt";
    public const string ServerCardSnapshotName = "server-prompt";
    public const string HelpTextSnapshotName = "help-prompt";

    public static IEnumerable<object[]> InvalidFlows()
    {
        yield return BuildTestCaseObject(
            "Exits on Multiple invalid server Input",
            new FirewallAccessDetails(),
            new List<UtteranceAndReply>
            {
                new("hi", ReplyType.Hero, FirewallAccessDialog.ServerTypeMsgText),
                new("PostgreSQL", ReplyType.Card, ServerCardSnapshotName),
                new("invalid-server-1", ReplyType.Text, FirewallAccessDialog.ServerNotFoundMessage),
                new(ReplyType.Card, ServerCardSnapshotName),
                new("invalid-server-2", ReplyType.Text, FirewallAccessDialog.ServerNotFoundMessage),
                new(ReplyType.Card, ServerCardSnapshotName),
                new("invalid-server-3", ReplyType.Exception, "Wasn't able to get a valid server name for PostgreSql")
            },
            new FirewallAccessDetails
            {
                Server = "test-server-pg-a",
                Ip = "127.0.0.1",
                Type = ServerTypes.PostgreSql,
                Prefilled = false
            });

        yield return BuildTestCaseObject(
            "Exits on Multiple invalid ip Input",
            new FirewallAccessDetails(),
            new List<UtteranceAndReply>
            {
                new("hi", ReplyType.Hero, FirewallAccessDialog.ServerTypeMsgText),
                new("PostgreSQL", ReplyType.Card, ServerCardSnapshotName),
                new("test-server-pg-a", ReplyType.Card, IpCardSnapshotName),
                new("127.0.0.1.1", ReplyType.Card, IpCardSnapshotName),
                new("127.0.0.1.2", ReplyType.Card, IpCardSnapshotName),
                new("127.0.0.1.3", ReplyType.Exception, FirewallAccessDialog.InvalidIpAddress)
            },
            new FirewallAccessDetails
            {
                Server = "test-server-pg-a",
                Ip = "127.0.0.1",
                Type = ServerTypes.PostgreSql,
                Prefilled = false
            });
    }

    public static IEnumerable<object[]> StartLoginFlows()
    {
        yield return BuildTestCaseObject(
            "Handles Typed Input",
            new FirewallAccessDetails(),
            new List<UtteranceAndReply>
            {
                new("hi", ReplyType.Hero, FirewallAccessDialog.ServerTypeMsgText),
                new("PostgreSQL", ReplyType.Card, ServerCardSnapshotName),
                new("test-server-pg-a", ReplyType.Card, IpCardSnapshotName),
                new("127.0.0.1", ReplyType.Card, OauthCardSnapshotName)
            },
            new FirewallAccessDetails
            {
                Server = "test-server-pg-a",
                Ip = "127.0.0.1",
                Type = ServerTypes.PostgreSql,
                Prefilled = false
            });


        yield return BuildTestCaseObject(
            "Handles Other Text Field Input",
            new FirewallAccessDetails(),
            new List<UtteranceAndReply>
            {
                new("hi", ReplyType.Hero, FirewallAccessDialog.ServerTypeMsgText),
                new("PostgreSQL", ReplyType.Card, ServerCardSnapshotName),
                new(new ActivityBuilder().WithServerSelectOtherValue("test-server-pg-a").Build(), ReplyType.Card,
                    IpCardSnapshotName),
                new("127.0.0.1", ReplyType.Card, OauthCardSnapshotName)
            },
            new FirewallAccessDetails
            {
                Server = "test-server-pg-a",
                Ip = "127.0.0.1",
                Type = ServerTypes.PostgreSql,
                Prefilled = false
            });

        yield return BuildTestCaseObject(
            "Re Prompt For server",
            new FirewallAccessDetails(),
            new List<UtteranceAndReply>
            {
                new("hi", ReplyType.Hero, FirewallAccessDialog.ServerTypeMsgText),
                new("PostgreSQL", ReplyType.Card, ServerCardSnapshotName),
                new("invalid-server", ReplyType.Text, FirewallAccessDialog.ServerNotFoundMessage),
                new(ReplyType.Card, ServerCardSnapshotName),
                new(new ActivityBuilder().WithServerSelectValue("test-server-pg-a").Build(), ReplyType.Card,
                    IpCardSnapshotName),
                new("127.0.0.1", ReplyType.Card, OauthCardSnapshotName)
            },
            new FirewallAccessDetails
            {
                Server = "test-server-pg-a",
                Ip = "127.0.0.1",
                Type = ServerTypes.PostgreSql,
                Prefilled = false
            });

        yield return BuildTestCaseObject(
            "Re Prompt For Type",
            new FirewallAccessDetails(),
            new List<UtteranceAndReply>
            {
                new("hi", ReplyType.Hero, FirewallAccessDialog.ServerTypeMsgText),
                new("INVALID", ReplyType.Hero, FirewallAccessDialog.ServerTypeMsgText),
                new("PostgreSQL", ReplyType.Card, ServerCardSnapshotName),
                new(new ActivityBuilder().WithServerSelectValue("test-server-pg-a").Build(), ReplyType.Card,
                    IpCardSnapshotName),
                new("127.0.0.1", ReplyType.Card, OauthCardSnapshotName)
            },
            new FirewallAccessDetails
            {
                Server = "test-server-pg-a",
                Ip = "127.0.0.1",
                Type = ServerTypes.PostgreSql,
                Prefilled = false
            });

        yield return BuildTestCaseObject(
            "Re Prompt For Ip",
            new FirewallAccessDetails(),
            new List<UtteranceAndReply>
            {
                new("hi", ReplyType.Hero, FirewallAccessDialog.ServerTypeMsgText),
                new("PostgreSQL", ReplyType.Card, ServerCardSnapshotName),
                new(new ActivityBuilder().WithServerSelectValue("test-server-pg-a").Build(), ReplyType.Card, "ip-card"),
                new("not an ip", ReplyType.Card, "ip-card"),
                new("127.0.0.1.0", ReplyType.Card, IpCardSnapshotName),
                new("127.0.0.1", ReplyType.Card, OauthCardSnapshotName)
            },
            new FirewallAccessDetails
            {
                Server = "test-server-pg-a",
                Ip = "127.0.0.1",
                Type = ServerTypes.PostgreSql,
                Prefilled = false
            });
    }

    public static IEnumerable<object[]> MockLoginFlows()
    {
        yield return BuildTestCaseObject(
            "PostgreSQL Fully Qualified Domain",
            new FirewallAccessDetails(),
            new List<UtteranceAndReply>
            {
                new("hi", ReplyType.Hero, FirewallAccessDialog.ServerTypeMsgText),
                new("PostgreSQL", ReplyType.Card, ServerCardSnapshotName),
                new("test-server-pg-a.postgres.database.azure.com", ReplyType.Card, IpCardSnapshotName),
                new("127.0.0.1", ReplyType.Text, "TextPrompt mock invoked with options: "),
                new(ReplyType.Text, "Adding access to PostgreSql - test-server-pg-a.postgres.database.azure.com for 127.0.0.1"),
                new(ReplyType.Text,
                    "Your access was added as Megan Bowen  (MeganB@M365x214355.onmicrosoft.com). To logout, type 'logout'."),
                new(ReplyType.Text, "Access will be removed after one day"),
                new(ReplyType.Card, HelpTextSnapshotName)
            },
            new FirewallAccessDetails
            {
                Server = "test-server-pg-a.postgres.database.azure.com",
                Ip = "127.0.0.1",
                Type = ServerTypes.PostgreSql,
                Prefilled = false
            });

        yield return BuildTestCaseObject(
            "PostgreSQL",
            new FirewallAccessDetails(),
            new List<UtteranceAndReply>
            {
                new("hi", ReplyType.Hero, FirewallAccessDialog.ServerTypeMsgText),
                new("PostgreSQL", ReplyType.Card, ServerCardSnapshotName),
                new("test-server-pg-a", ReplyType.Card, IpCardSnapshotName),
                new("127.0.0.1", ReplyType.Text, "TextPrompt mock invoked with options: "),
                new(ReplyType.Text, "Adding access to PostgreSql - test-server-pg-a for 127.0.0.1"),
                new(ReplyType.Text,
                    "Your access was added as Megan Bowen  (MeganB@M365x214355.onmicrosoft.com). To logout, type 'logout'."),
                new(ReplyType.Text, "Access will be removed after one day"),
                new(ReplyType.Card, HelpTextSnapshotName)
            },
            new FirewallAccessDetails
            {
                Server = "test-server-pg-a",
                Ip = "127.0.0.1",
                Type = ServerTypes.PostgreSql,
                Prefilled = false
            });

        yield return BuildTestCaseObject(
            "SqlServer",
            new FirewallAccessDetails(),
            new List<UtteranceAndReply>
            {
                new("hi", ReplyType.Hero, FirewallAccessDialog.ServerTypeMsgText),
                new("SqlServer", ReplyType.Card, ServerCardSnapshotName),
                new("test-server-sql-a", ReplyType.Card, IpCardSnapshotName),
                new("127.0.0.1", ReplyType.Text, "TextPrompt mock invoked with options: "),
                new(ReplyType.Text, "Adding access to SqlServer - test-server-sql-a for 127.0.0.1"),
                new(ReplyType.Text,
                    "Your access was added as Megan Bowen  (MeganB@M365x214355.onmicrosoft.com). To logout, type 'logout'."),
                new(ReplyType.Text, "Access will be removed after one day"),
                new(ReplyType.Card, HelpTextSnapshotName)
            },
            new FirewallAccessDetails
            {
                Server = "test-server-sql-a",
                Ip = "127.0.0.1",
                Type = ServerTypes.SqlServer,
                Prefilled = false
            });

        yield return BuildTestCaseObject(
            "SqlServer Fully Qualified Domain",
            new FirewallAccessDetails(),
            new List<UtteranceAndReply>
            {
                new("hi", ReplyType.Hero, FirewallAccessDialog.ServerTypeMsgText),
                new("SqlServer", ReplyType.Card, ServerCardSnapshotName),
                new("test-server-sql-a.database.windows.net", ReplyType.Card, IpCardSnapshotName),
                new("127.0.0.1", ReplyType.Text, "TextPrompt mock invoked with options: "),
                new(ReplyType.Text, "Adding access to SqlServer - test-server-sql-a.database.windows.net for 127.0.0.1"),
                new(ReplyType.Text,
                    "Your access was added as Megan Bowen  (MeganB@M365x214355.onmicrosoft.com). To logout, type 'logout'."),
                new(ReplyType.Text, "Access will be removed after one day"),
                new(ReplyType.Card, HelpTextSnapshotName)
            },
            new FirewallAccessDetails
            {
                Server = "test-server-sql-a.database.windows.net",
                Ip = "127.0.0.1",
                Type = ServerTypes.SqlServer,
                Prefilled = false
            });

        yield return BuildTestCaseObject(
            "SqlServer Pre Filled",
            new FirewallAccessDetails(),
            new List<UtteranceAndReply>
            {
                new("fw/SqlServer/test-server-sql-a/127.0.0.1", ReplyType.Text,
                    "TextPrompt mock invoked with options: "),
                new(ReplyType.Text, "Adding access to SqlServer - test-server-sql-a for 127.0.0.1"),
                new(ReplyType.Text,
                    "Your access was added as Megan Bowen  (MeganB@M365x214355.onmicrosoft.com). To logout, type 'logout'."),
                new(ReplyType.Text, "Access will be removed after one day")
            },
            new FirewallAccessDetails
            {
                Server = "test-server-sql-a",
                Ip = "127.0.0.1",
                Type = ServerTypes.SqlServer,
                Prefilled = true
            });

        yield return BuildTestCaseObject(
            "Invalid prefill ServerType prompts for Type Only",
            new FirewallAccessDetails(),
            new List<UtteranceAndReply>
            {
                new("fw/MySQLServerNotSupported/test-server-sql-a/127.0.0.1", ReplyType.Hero,
                    FirewallAccessDialog.ServerTypeMsgText),
                new("SqlServer", ReplyType.Text, "TextPrompt mock invoked with options: "),
                new(ReplyType.Text, "Adding access to SqlServer - test-server-sql-a for 127.0.0.1"),
                new(ReplyType.Text,
                    "Your access was added as Megan Bowen  (MeganB@M365x214355.onmicrosoft.com). To logout, type 'logout'."),
                new(ReplyType.Text, "Access will be removed after one day")
            },
            new FirewallAccessDetails
            {
                Server = "test-server-sql-a",
                Ip = "127.0.0.1",
                Type = ServerTypes.SqlServer,
                Prefilled = true
            });

        yield return BuildTestCaseObject(
            "Invalid prefill Ip Address prompts for Type Ip",
            new FirewallAccessDetails(),
            new List<UtteranceAndReply>
            {
                new("fw/SqlServer/test-server-sql-a/266.0.0.1", ReplyType.Card, IpCardSnapshotName),
                new("127.0.0.1", ReplyType.Text, "TextPrompt mock invoked with options: "),
                new(ReplyType.Text, "Adding access to SqlServer - test-server-sql-a for 127.0.0.1"),
                new(ReplyType.Text,
                    "Your access was added as Megan Bowen  (MeganB@M365x214355.onmicrosoft.com). To logout, type 'logout'."),
                new(ReplyType.Text, "Access will be removed after one day")
            },
            new FirewallAccessDetails
            {
                Server = "test-server-sql-a",
                Ip = "127.0.0.1",
                Type = ServerTypes.SqlServer,
                Prefilled = true
            });
    }

    private static object[] BuildTestCaseObject(string testCaseName, FirewallAccessDetails inputBookingInfo,
        List<UtteranceAndReply> utterancesAndReplies, FirewallAccessDetails expectedBookingInfo)
    {
        var testData = new FirewallAccessDetailsTestCase(
            testCaseName,
            inputBookingInfo,
            utterancesAndReplies,
            expectedBookingInfo
        );
        return new object[] { new TestDataObject(testData) };
    }
}
