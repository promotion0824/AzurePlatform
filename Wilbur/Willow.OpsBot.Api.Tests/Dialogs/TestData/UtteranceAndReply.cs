using Microsoft.Bot.Schema;

namespace Willow.OpsBot.Api.Tests.Dialogs.TestData;

public record UtteranceAndReply
{
    public UtteranceAndReply(Activity activity, ReplyType replyType, string replyOrSnapshotName)
    {
        Activity = activity;
        ReplyType = replyType;
        ReplyOrSnapshotName = replyOrSnapshotName;
    }

    public UtteranceAndReply(string utterance, ReplyType replyType, string replyOrSnapshotName)
    {
        Utterance = utterance;
        ReplyType = replyType;
        ReplyOrSnapshotName = replyOrSnapshotName;
    }

    public UtteranceAndReply(ReplyType replyType, string replyOrSnapshotName)
    {
        ReplyType = replyType;
        ReplyOrSnapshotName = replyOrSnapshotName;
    }

    public UtteranceAndReply()
    {
    }

    public string? Utterance { get; init; }
    public Activity? Activity { get; init; }
    public ReplyType? ReplyType { get; init; }
    public string? ReplyOrSnapshotName { get; init; }
}
