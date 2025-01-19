using Carbunqlex.Clauses;
using Carbunqlex.ValueExpressions;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Carbunqlex.DatasourceExpressions;

public class FunctionSource : IDatasource
{
    public string FunctionName { get; set; }
    public List<IValueExpression> Arguments { get; set; }
    public string Alias { get; set; }
    public IColumnAliasClause ColumnAliasClause { get; set; }
    public string TableFullName => string.Empty;

    public FunctionSource(string functionName, IEnumerable<IValueExpression> arguments, string alias, ColumnAliasClause columnAliases)
    {
        FunctionName = functionName;
        Arguments = arguments.ToList();
        Alias = alias;
        ColumnAliasClause = columnAliases;
    }

    public FunctionSource(string functionName, IEnumerable<IValueExpression> arguments, string alias)
    {
        FunctionName = functionName;
        Arguments = arguments.ToList();
        Alias = alias;
        ColumnAliasClause = EmptyColumnAliasClause.Instance;
    }

    public FunctionSource(string functionName, string alias)
    {
        FunctionName = functionName;
        Arguments = new List<IValueExpression>();
        Alias = alias;
        ColumnAliasClause = new ColumnAliasClause(Enumerable.Empty<string>());
    }

    public string ToSqlWithoutCte()
    {
        if (string.IsNullOrWhiteSpace(FunctionName))
        {
            throw new ArgumentException("FunctionName is required for a function source.", nameof(FunctionName));
        }
        if (string.IsNullOrWhiteSpace(Alias))
        {
            throw new ArgumentException("Alias is required for a function source.", nameof(Alias));
        }
        var sb = new StringBuilder();
        sb.Append(FunctionName);
        sb.Append("(");
        sb.Append(string.Join(", ", Arguments.Select(arg => arg.ToSqlWithoutCte())));
        sb.Append(") as ");
        sb.Append(Alias);
        sb.Append(ColumnAliasClause.ToSqlWithoutCte());
        return sb.ToString();
    }

    public IEnumerable<Token> GenerateTokensWithoutCte()
    {
        var tokens = new List<Token>
        {
            new Token(TokenType.Command, FunctionName),
            new Token(TokenType.Identifier, Alias)
        };
        tokens.AddRange(Arguments.SelectMany(arg => arg.GenerateTokensWithoutCte()));
        tokens.AddRange(ColumnAliasClause.GenerateTokensWithoutCte());
        return tokens;
    }

    public IEnumerable<ISelectQuery> GetQueries()
    {
        var queries = new List<ISelectQuery>();

        foreach (var argument in Arguments)
        {
            if (argument.MightHaveQueries)
            {
                queries.AddRange(argument.GetQueries());
            }
        }

        return queries;
    }

    public IEnumerable<string> GetSelectableColumns()
    {
        return ColumnAliasClause.GetColumnNames();
    }

    public bool TryGetSubQuery([NotNullWhen(true)] out ISelectQuery? subQuery)
    {
        subQuery = null;
        return false;
    }

    public bool TryGetTableName([NotNullWhen(true)] out string? tableFullName)
    {
        tableFullName = null;
        return false;
    }

    public bool TryGetUnionQuerySource([NotNullWhen(true)] out UnionQuerySource? unionQuerySource)
    {
        unionQuerySource = null;
        return false;
    }
}
