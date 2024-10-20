using Microsoft.Bot.Schema;
using Willow.OpsBot.Api.Models;

namespace Willow.OpsBot.Api.Tests.Builders;

public class ActivityBuilder
{
    private object? _value;

    public ActivityBuilder WithServerSelectValue(string selection)
    {
        _value = new ServerSelectionResult
        {
            ServerSelect = selection
        };
        return this;
    }

    public ActivityBuilder WithServerSelectOtherValue(string selection)
    {
        _value = new ServerSelectionResult
        {
            OtherSelect = selection
        };
        return this;
    }

    public Activity Build()
    {
        return new Activity
        {
            Value = _value
        };
    }
}
