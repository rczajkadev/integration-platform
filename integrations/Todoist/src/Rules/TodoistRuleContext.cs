namespace Integrations.Todoist.Rules;

internal sealed class TodoistRuleContext
{
    private readonly List<string> _messages = [];

    public IReadOnlyCollection<string> Messages => _messages;

    public bool HasMessages => _messages.Count > 0;

    public void AddMessage(string message)
    {
        if (string.IsNullOrWhiteSpace(message)) return;
        _messages.Add(message.Trim());
    }
}
