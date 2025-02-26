using Carbunqlex.Expressions;
using Carbunqlex.Lexing;
using System.Text;

namespace Carbunqlex.Clauses;

public class HavingClause : ISqlComponent
{
    public List<IValueExpression> Conditions { get; set; }

    public HavingClause(params IValueExpression[] conditions)
    {
        Conditions = conditions.ToList();
    }

    public HavingClause(List<IValueExpression> conditions)
    {
        Conditions = conditions;
    }

    public string ToSqlWithoutCte()
    {
        if (Conditions.Count == 0)
        {
            return string.Empty;
        }

        var sb = new StringBuilder("having ");
        sb.Append(string.Join(", ", Conditions.Select(c => c.ToSqlWithoutCte())));
        return sb.ToString();
    }

    public IEnumerable<Token> GenerateTokensWithoutCte()
    {
        if (Conditions.Count == 0)
        {
            return Enumerable.Empty<Token>();
        }

        int initialCapacity = Conditions.Count * 5 + 1;
        var tokens = new List<Token>(initialCapacity)
        {
            new Token(TokenType.StartClause, "having", "having")
        };

        foreach (var condition in Conditions)
        {
            tokens.AddRange(condition.GenerateTokensWithoutCte());
            tokens.Add(new Token(TokenType.Comma, ",", "having"));
        }

        if (tokens.Count > 1)
        {
            tokens.RemoveAt(tokens.Count - 1);
        }

        tokens.Add(new Token(TokenType.EndClause, string.Empty, "having"));
        return tokens;
    }

    public IEnumerable<ISelectQuery> GetQueries()
    {
        return Conditions
            .Where(condition => condition.MightHaveQueries)
            .SelectMany(condition => condition.GetQueries());
    }

    public void Add(IValueExpression condition)
    {
        Conditions.Add(condition);
    }

    public void AddRange(IEnumerable<IValueExpression> conditions)
    {
        Conditions.AddRange(conditions);
    }
}
