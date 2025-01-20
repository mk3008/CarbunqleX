using System.Text;

namespace Carbunqlex.ValueExpressions;

/// <summary>
/// Represents an array value expression.
/// e.g. ARRAY[1, 2, 3]
/// </summary>
public class ArrayExpression : IValueExpression, IArgumentExpression
{
    public IArgumentExpression Arguments { get; }

    public ArrayExpression(IArgumentExpression arguments)
    {
        Arguments = arguments;
    }

    public string DefaultName => string.Empty;

    public bool MightHaveQueries => Arguments.MightHaveQueries;

    public string ToSqlWithoutCte()
    {
        var sb = new StringBuilder();
        sb.Append("array[");
        sb.Append(Arguments.ToSqlWithoutCte());
        sb.Append("]");
        return sb.ToString();
    }

    public IEnumerable<Token> GenerateTokensWithoutCte()
    {
        yield return new Token(TokenType.Command, "array");
        yield return new Token(TokenType.OpenBracket, "[");
        foreach (var lexeme in Arguments.GenerateTokensWithoutCte())
        {
            yield return lexeme;
        }
        yield return new Token(TokenType.CloseBracket, "]");
    }

    public IEnumerable<ISelectQuery> GetQueries()
    {
        return Arguments.GetQueries();
    }

    public IEnumerable<ColumnExpression> ExtractColumnExpressions()
    {
        return Arguments.ExtractColumnExpressions();
    }
}
