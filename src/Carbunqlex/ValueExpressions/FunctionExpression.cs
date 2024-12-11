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

    public string ToSqlWithoutCte()
    {
        var sb = new StringBuilder();
        sb.Append(FunctionName);
        sb.Append("(");
        sb.Append(string.Join(", ", Arguments.Select(arg => arg.ToSqlWithoutCte())));
        sb.Append(")");
        if (OverClause != null)
        {
            sb.Append(" ");
            sb.Append(OverClause.ToSqlWithoutCte());
        }
        return sb.ToString();
    }

    public string DefaultName => string.Empty;

    public bool MightHaveCommonTableClauses => Arguments.Any(arg => arg.MightHaveCommonTableClauses) || (OverClause?.MightHaveCommonTableClauses ?? false);

    public IEnumerable<Lexeme> GenerateLexemesWithoutCte()
    {
        yield return new Lexeme(LexType.Keyword, FunctionName);
        yield return new Lexeme(LexType.OpenParen, "(");
        for (int i = 0; i < Arguments.Count; i++)
        {
            foreach (var lexeme in Arguments[i].GenerateLexemesWithoutCte())
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
            foreach (var lexeme in OverClause.GenerateLexemesWithoutCte())
            {
                yield return lexeme;
            }
        }
    }

    public IEnumerable<CommonTableClause> GetCommonTableClauses()
    {
        if (!MightHaveCommonTableClauses)
        {
            return Enumerable.Empty<CommonTableClause>();
        }

        var commonTableClauses = new List<CommonTableClause>();

        foreach (var argument in Arguments)
        {
            if (argument.MightHaveCommonTableClauses)
            {
                commonTableClauses.AddRange(argument.GetCommonTableClauses());
            }
        }

        if (OverClause?.MightHaveCommonTableClauses ?? false)
        {
            commonTableClauses.AddRange(OverClause.GetCommonTableClauses());
        }

        return commonTableClauses;
    }
}
