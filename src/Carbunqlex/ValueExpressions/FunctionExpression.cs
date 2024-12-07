using Carbunqlex.Clauses;
using System.Text;

namespace Carbunqlex.ValueExpressions;

public class FunctionExpression : IValueExpression
{
    public List<IValueExpression> Arguments { get; set; }
    public string FunctionName { get; set; }
    public OverClause? OverClause { get; set; }

    public FunctionExpression(string functionName, params IValueExpression[] arguments)
    {
        FunctionName = functionName;
        Arguments = arguments.ToList();
    }

    public string ToSql()
    {
        var sb = new StringBuilder();
        sb.Append(FunctionName);
        sb.Append("(");
        sb.Append(string.Join(", ", Arguments.Select(arg => arg.ToSql())));
        sb.Append(")");
        if (OverClause != null)
        {
            sb.Append(" ");
            sb.Append(OverClause.ToSql());
        }
        return sb.ToString();
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
        if (OverClause != null)
        {
            foreach (var lexeme in OverClause.GetLexemes())
            {
                yield return lexeme;
            }
        }
    }
}
