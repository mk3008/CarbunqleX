using Carbunqlex.ValueExpressions;
using System.Text;

namespace Carbunqlex.Clauses;

public class SelectClause : ISqlComponent
{
    public List<SelectExpression> Expressions { get; }
    public IDistinctClause DistinctClause { get; set; }

    public SelectClause(params SelectExpression[] selectExpressions)
    {
        DistinctClause = EmptyDistinctClause.Instance;
        Expressions = selectExpressions.ToList();
    }

    public SelectClause(IEnumerable<SelectExpression> selectExpressions)
    {
        DistinctClause = EmptyDistinctClause.Instance;
        Expressions = selectExpressions.ToList();
    }

    public SelectClause(IDistinctClause distinctClause, params SelectExpression[] selectExpressions)
    {
        DistinctClause = distinctClause;
        Expressions = selectExpressions.ToList();
    }

    public SelectClause(IDistinctClause distinctClause, IEnumerable<SelectExpression> selectExpressions)
    {
        DistinctClause = distinctClause;
        Expressions = selectExpressions.ToList();
    }

    public string ToSqlWithoutCte()
    {
        var sb = new StringBuilder();
        sb.Append("select ");

        var distinctSql = DistinctClause.ToSqlWithoutCte();
        if (!string.IsNullOrEmpty(distinctSql))
        {
            sb.Append(distinctSql).Append(" ");
        }

        if (Expressions.Count == 0)
        {
            sb.Append("*");
        }
        else
        {
            foreach (var column in Expressions)
            {
                sb.Append(column.ToSqlWithoutCte()).Append(", ");
            }

            // Remove the trailing comma and space if there are any expressions
            if (Expressions.Count > 0)
            {
                sb.Length -= 2;
            }
        }

        return sb.ToString();
    }

    public IEnumerable<Token> GenerateTokensWithoutCte()
    {
        // Estimate the initial capacity for the tokens list.
        // Each SelectExpression can return up to 3 tokens (column name, AS, alias).
        // Adding 2 for the "select" keyword and potential "distinct" clause.
        int initialCapacity = Expressions.Count * 3 + 2;
        var tokens = new List<Token>(initialCapacity)
        {
            new Token(TokenType.Command, "select")
        };

        tokens.AddRange(DistinctClause.GenerateTokensWithoutCte());

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
