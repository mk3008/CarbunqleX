using System.Text;

namespace Carbunqlex.ValueExpressions;

/// <summary>
/// Represents an array value expression.
/// e.g. ARRAY[1, 2, 3]
/// </summary>
public class ArrayExpression : IValueExpression, IArgumentExpression
{
    public IEnumerable<IValueExpression> Elements { get; }

    public ArrayExpression(IEnumerable<IValueExpression> elements)
    {
        Elements = elements;
    }

    public string DefaultName => string.Empty;

    public bool MightHaveQueries => Elements.Any(element => element.MightHaveQueries);

    public string ToSqlWithoutCte()
    {
        var sb = new StringBuilder();
        sb.Append("array[");
        sb.Append(string.Join(", ", Elements.Select(element => element.ToSqlWithoutCte())));
        sb.Append("]");
        return sb.ToString();
    }

    public IEnumerable<Token> GenerateTokensWithoutCte()
    {
        yield return new Token(TokenType.Keyword, "array");
        yield return new Token(TokenType.OpenBracket, "[");
        for (int i = 0; i < Elements.Count(); i++)
        {
            foreach (var lexeme in Elements.ElementAt(i).GenerateTokensWithoutCte())
            {
                yield return lexeme;
            }
            if (i < Elements.Count() - 1)
            {
                yield return Token.Comma;
            }
        }
        yield return new Token(TokenType.CloseBracket, "]");
    }

    public IEnumerable<ISelectQuery> GetQueries()
    {
        var queries = new List<ISelectQuery>();
        foreach (var element in Elements)
        {
            if (element.MightHaveQueries)
            {
                queries.AddRange(element.GetQueries());
            }
        }
        return queries;
    }

    public IEnumerable<ColumnExpression> ExtractColumnExpressions()
    {
        return Elements.SelectMany(element => element.ExtractColumnExpressions());
    }
}
