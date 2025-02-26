using Carbunqlex.Lexing;
using Carbunqlex.ValueExpressions;
using System.Text;

namespace Carbunqlex.Clauses;

public class SelectClause : ISqlComponent
{
    public List<SelectExpression> Expressions { get; }

    public IDistinctClause? DistinctClause { get; set; }

    public SelectClause(params SelectExpression[] selectExpressions)
    {
        DistinctClause = null;
        Expressions = selectExpressions.ToList();
    }

    public SelectClause(List<SelectExpression> selectExpressions)
    {
        DistinctClause = null;
        Expressions = selectExpressions;
    }

    public SelectClause(IDistinctClause? distinctClause, params SelectExpression[] selectExpressions)
    {
        DistinctClause = distinctClause;
        Expressions = selectExpressions.ToList();
    }

    public SelectClause(IDistinctClause? distinctClause, List<SelectExpression> selectExpressions)
    {
        DistinctClause = distinctClause;
        Expressions = selectExpressions;
    }

    public string ToSqlWithoutCte()
    {
        var sb = new StringBuilder();
        sb.Append("select");

        if (DistinctClause != null)
        {
            sb.Append(" ").Append(DistinctClause.ToSqlWithoutCte());
        }

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
        tokens.Add(new Token(TokenType.Command, "select"));

        if (DistinctClause != null)
        {
            tokens.AddRange(DistinctClause.GenerateTokensWithoutCte());
        }

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
        return Expressions
            .Where(item => item.Value.MightHaveQueries)
            .SelectMany(item => item.Value.GetQueries());
    }

    public IEnumerable<ColumnExpression> ExtractColumnExpressions()
    {
        return Expressions.SelectMany(expr => expr.Value.ExtractColumnExpressions());
    }
}
