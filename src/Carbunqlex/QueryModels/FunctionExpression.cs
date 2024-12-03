namespace Carbunqlex.QueryModels;

public class FunctionExpression : IValueExpression
{
    // Note: Aggregate functions (e.g., SUM, COUNT, AVG) can be represented using FunctionExpression.

    public List<IValueExpression> Arguments { get; set; }
    public string FunctionName { get; set; }

    public FunctionExpression(string functionName, params IValueExpression[] arguments)
    {
        FunctionName = functionName;
        Arguments = arguments.ToList();
    }

    public string ToSql()
    {
        var args = string.Join(", ", Arguments.Select(arg => arg.ToSql()));
        return $"{FunctionName}({args})";
    }

    public string DefaultName => string.Empty;

    public IEnumerable<Lexeme> GetLexemes()
    {
        yield return new Lexeme(LexType.Keyword, FunctionName);
        yield return new Lexeme(LexType.OpenParen, "(");
        for (int i = 0; i < Arguments.Count; i++)
        {
            foreach (var lexeme in Arguments[i].GetLexemes())
            {
                yield return lexeme;
            }
            if (i < Arguments.Count - 1)
            {
                yield return new Lexeme(LexType.Comma, ",");
            }
        }
        yield return new Lexeme(LexType.CloseParen, ")");
    }
}
