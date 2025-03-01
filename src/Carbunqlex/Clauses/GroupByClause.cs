using Carbunqlex.Expressions;
using Carbunqlex.Lexing;
using System.Text;

namespace Carbunqlex.Clauses;

public class GroupByClause : ISqlComponent
{
    public List<IValueExpression> GroupByColumns { get; }

    public GroupByClause(params IValueExpression[] groupByColumns)
    {
        GroupByColumns = groupByColumns.ToList();
    }

    public GroupByClause(List<IValueExpression> groupByColumns)
    {
        GroupByColumns = groupByColumns;
    }

    public string ToSqlWithoutCte()
    {
        if (GroupByColumns.Count == 0)
        {
            return string.Empty;
        }

        var sb = new StringBuilder();
        sb.Append("group by ");
        sb.Append(string.Join(", ", GroupByColumns.Select(c => c.ToSqlWithoutCte())));
        return sb.ToString();
    }

    public IEnumerable<Token> GenerateTokensWithoutCte()
    {
        if (GroupByColumns.Count == 0)
        {
            return Enumerable.Empty<Token>();
        }

        var tokens = new List<Token>()
        {
            new Token(TokenType.StartClause, "group by")
        };

        for (int i = 0; i < GroupByColumns.Count; i++)
        {
            var column = GroupByColumns[i];
            tokens.AddRange(column.GenerateTokensWithoutCte());
            if (i < GroupByColumns.Count - 1)
            {
                tokens.Add(new Token(TokenType.Comma, ","));
            }
        }

        return tokens;
    }

    public IEnumerable<ISelectQuery> GetQueries()
    {
        return GroupByColumns
            .Where(column => column.MightHaveQueries)
            .SelectMany(column => column.GetQueries());
    }

    public void Add(IValueExpression column)
    {
        GroupByColumns.Add(column);
    }

    public void AddRange(IEnumerable<IValueExpression> columns)
    {
        GroupByColumns.AddRange(columns);
    }
}
