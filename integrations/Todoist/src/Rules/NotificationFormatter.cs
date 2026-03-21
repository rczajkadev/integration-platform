namespace Integrations.Todoist.Rules;

internal static class NotificationFormatter
{
    public static string BuildNumberedListMessage(string header, IEnumerable<string> items)
    {
        var numberedItems = items.Select((item, index) => $"{index + 1}) {item}");
        return $"{header}{Environment.NewLine}{string.Join(Environment.NewLine, numberedItems)}";
    }
}
