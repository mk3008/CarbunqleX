using Carbunqlex.Expressions;
using Carbunqlex.Lexing;

namespace Carbunqlex.Parsing.Expressions;

public class CubeExpression : IValueExpression
{
    public List<IValueExpression> Values { get; }

    public string DefaultName => string.Empty;

    public bool MightHaveQueries => false;

    public CubeExpression(List<IValueExpression> values)
    {
        Values = values;
    }

    public string ToSqlWithoutCte()
    {
        return $"cube({string.Join(", ", Values.Select(r => r.ToSqlWithoutCte()))})";
    }

    public IEnumerable<Token> GenerateTokensWithoutCte()
    {
        var tokens = new List<Token>
        {
            new Token(TokenType.Identifier, "cube"),
            new Token(TokenType.OpenParen, "(")
        };

        for (int i = 0; i < Values.Count; i++)
        {
            tokens.AddRange(Values[i].GenerateTokensWithoutCte());
            if (i < Values.Count - 1)
            {
                tokens.Add(new Token(TokenType.Comma, ","));
            }
        }
        tokens.Add(new Token(TokenType.CloseParen, ")"));
        return tokens;
    }

    public IEnumerable<ISelectQuery> GetQueries()
    {
        yield break;
    }

    public IEnumerable<ColumnExpression> ExtractColumnExpressions()
    {
        return Enumerable.Empty<ColumnExpression>();
    }
}
