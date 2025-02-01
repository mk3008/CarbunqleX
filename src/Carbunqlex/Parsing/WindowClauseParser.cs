using Carbunqlex.Clauses;

namespace Carbunqlex.Parsing;

/// <summary>
/// e.g. window w as (partition by a order by b)
/// </summary>
public static class WindowClauseParser
{
    private static string ParserName => nameof(WindowClauseParser);

    public static WindowClause Parse(SqlTokenizer tokenizer)
    {
        tokenizer.Read(ParserName, "window");

        var windowExpressions = new List<WindowExpression>();

        while (true)
        {
            var windowExpression = WindowExpressionParser.Parse(tokenizer);
            windowExpressions.Add(windowExpression);
            if (tokenizer.IsEnd)
            {
                break;
            }
            if (tokenizer.Peek().Type == TokenType.Comma)
            {
                tokenizer.CommitPeek();
                continue;
            }
            break;
        }

        return new WindowClause(windowExpressions.ToArray());
    }
}

public static class WindowExpressionParser
{
    private static string ParserName => nameof(WindowExpressionParser);
    public static WindowExpression Parse(SqlTokenizer tokenizer)
    {
        var alias = tokenizer.Read(ParserName, TokenType.Identifier).Value;
        tokenizer.Read(ParserName, "as");
        var windowFunction = WindowFunctionParser.Parse(tokenizer);
        if (windowFunction is NamelessWindowDefinition w)
        {
            return new WindowExpression(alias, w);
        }
        throw new Exception("Named window definition expected");
    }
}
