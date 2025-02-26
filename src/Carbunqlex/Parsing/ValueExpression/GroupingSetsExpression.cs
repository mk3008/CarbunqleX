using Carbunqlex.Lexing;
using Carbunqlex.ValueExpressions;

namespace Carbunqlex.Parsing.ValueExpression;

public class GroupingSetsExpression : IValueExpression
{
    public List<GroupingSetExpression> GroupingSets { get; }

    public string DefaultName => string.Empty;

    public bool MightHaveQueries => false;

    public GroupingSetsExpression(List<GroupingSetExpression> groupingSets)
    {
        GroupingSets = groupingSets;
    }
    public string ToSqlWithoutCte()
    {
        return $"grouping sets({string.Join(", ", GroupingSets.Select(g => g.ToSqlWithoutCte()))})";
    }
    public IEnumerable<Token> GenerateTokensWithoutCte()
    {
        var tokens = new List<Token>
        {
            new Token(TokenType.Identifier, "grouping sets"),
            new Token(TokenType.OpenParen, "(")
        };

        for (int i = 0; i < GroupingSets.Count; i++)
        {
            tokens.AddRange(GroupingSets[i].GenerateTokensWithoutCte());
            if (i < GroupingSets.Count - 1)
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
