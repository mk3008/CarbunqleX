namespace Carbunqlex.Parsing;

public static class SqlKeywordBuilder
{
    public static List<SqlKeywordNode> Build(HashSet<string> strings)
    {
        // Split strings by space and get the first element
        var firstWords = strings
            .Select(x => x.Split(' ', StringSplitOptions.RemoveEmptyEntries).First())
            .Distinct()
            .ToList();

        var result = new List<SqlKeywordNode>();
        foreach (var firstWord in firstWords)
        {
            result.AddRange(Build(firstWord, strings));
        }
        return result;
    }

    private static List<SqlKeywordNode> Build(string prefix, HashSet<string> strings)
    {
        // If there is a text that exactly matches the prefix, terminal = true
        var isTerminal = strings.Contains(prefix);

        // Get texts that start with the prefix
        var children = strings
            .Where(x => x.StartsWith(prefix + " "))
            .Select(x => x.Substring(prefix.Length).Trim())
            .Where(x => !string.IsNullOrEmpty(x))
            .ToHashSet();

        // Get the next words after the prefix
        var nextWords = children
            .Select(x => x.Split(' ', StringSplitOptions.RemoveEmptyEntries).First())
            .Distinct()
            .ToHashSet();

        // If there are no next words, treat as a terminal node
        if (nextWords.Count == 0)
        {
            return new List<SqlKeywordNode> { new SqlKeywordNode(prefix, isTerminal, Enumerable.Empty<SqlKeywordNode>()) };
        }
        else
        {
            var result = new List<SqlKeywordNode>();
            foreach (var nextWord in nextWords)
            {
                result.AddRange(Build(nextWord, children));
            }
            return new List<SqlKeywordNode> { new SqlKeywordNode(prefix, isTerminal, result) };
        }
    }
}
