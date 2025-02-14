using Carbunqlex.ValueExpressions;
using System.Text;

namespace Carbunqlex.Clauses;

public class ReturningClause : ISqlComponent
{
    public List<SelectExpression> Expressions { get; }

    public ReturningClause(params SelectExpression[] selectExpressions)
    {
        Expressions = selectExpressions.ToList();
    }

    public ReturningClause(List<SelectExpression> selectExpressions)
    {
        Expressions = selectExpressions;
    }

    public string ToSqlWithoutCte()
    {
        var sb = new StringBuilder();
        sb.Append("returning");

        if (Expressions.Count == 0)
        {
            sb.Append(" *");
        }
        else
        {
            sb.Append(" ");
            for (var i = 0; i < Expressions.Count; i++)
            {
                sb.Append(Expressions[i].ToSqlWithoutCte());
                if (i < Expressions.Count - 1)
                {
                    sb.Append(", ");
                }
            }
        }

        return sb.ToString();
    }

    public IEnumerable<Token> GenerateTokensWithoutCte()
    {
        var tokens = new List<Token>();
        tokens.Add(new Token(TokenType.Command, "returning"));

        if (Expressions.Count == 0)
        {
            tokens.Add(new Token(TokenType.Identifier, "*"));
        }
        else
        {
            foreach (var column in Expressions)
            {
                tokens.AddRange(column.GenerateTokensWithoutCte());
            }
        }

        return tokens;
    }

    public IEnumerable<ISelectQuery> GetQueries()
    {
        yield break;
    }

    public IEnumerable<ColumnExpression> ExtractColumnExpressions()
    {
        yield break;
    }
}
