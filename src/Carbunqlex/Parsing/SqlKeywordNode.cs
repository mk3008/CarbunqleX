using System.Text;

namespace Carbunqlex.Parsing;

public readonly struct SqlKeywordNode
{
    private static readonly IReadOnlyDictionary<string, SqlKeywordNode> EmptyDictionary = new Dictionary<string, SqlKeywordNode>();

    public string Keyword { get; init; }
    public IReadOnlyDictionary<string, SqlKeywordNode> Children { get; init; }
    public bool IsTerminal { get; init; }
    public SqlKeywordNode(string keyword, bool isTerminal, IEnumerable<SqlKeywordNode> children)
    {
        if (keyword == null) throw new ArgumentNullException(nameof(keyword));
        if (children == null) throw new ArgumentNullException(nameof(children));
        Keyword = keyword;
        Children = children.ToDictionary(x => x.Keyword);
        IsTerminal = isTerminal;
    }
    public SqlKeywordNode(string keyword)
    {
        if (keyword == null) throw new ArgumentNullException(nameof(keyword));
        Keyword = keyword;
        IsTerminal = true;
        Children = EmptyDictionary;
    }

    public string ToTreeString(int indent = 0)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"{new string(' ', indent)}{Keyword} {(IsTerminal ? "[terminal]" : "")}");
        foreach (var child in Children.Values)
        {
            sb.Append(child.ToTreeString(indent + 2));
        }
        return sb.ToString();
    }
}
