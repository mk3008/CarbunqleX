using Carbunqlex.ValueExpressions;

namespace Carbunqlex.Parsing.ValueExpression;

public class GroupingSetExpression : ISqlComponent
{
    public List<IValueExpression> Values { get; }
    public GroupingSetExpression(List<IValueExpression> values)
    {
        Values = values;
    }
    public string ToSqlWithoutCte()
    {
        return $"({string.Join(", ", Values.Select(e => e.ToSqlWithoutCte()))})";
    }
    public IEnumerable<Token> GenerateTokensWithoutCte()
    {
        var tokens = new List<Token>
        {
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
}
