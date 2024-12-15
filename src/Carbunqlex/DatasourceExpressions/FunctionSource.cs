using Carbunqlex.ValueExpressions;
using System.Text;

namespace Carbunqlex.DatasourceExpressions;

public class FunctionSource : IDatasource
{
    public string FunctionName { get; set; }
    public List<IValueExpression> Arguments { get; set; }
    public string Alias { get; set; }
    public ColumnAliasClause ColumnAliasClause { get; set; }

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
        ColumnAliasClause = new ColumnAliasClause(Enumerable.Empty<string>());
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

    public IEnumerable<Lexeme> GenerateLexemesWithoutCte()
    {
        var lexemes = new List<Lexeme>
        {
            new Lexeme(LexType.Keyword, FunctionName),
            new Lexeme(LexType.Identifier, Alias)
        };
        lexemes.AddRange(Arguments.SelectMany(arg => arg.GenerateLexemesWithoutCte()));
        lexemes.AddRange(ColumnAliasClause.GenerateLexemesWithoutCte());
        return lexemes;
    }

    public IEnumerable<IQuery> GetQueries()
    {
        var queries = new List<IQuery>();

        foreach (var argument in Arguments)
        {
            if (argument.MightHaveQueries)
            {
                queries.AddRange(argument.GetQueries());
            }
        }

        return queries;
    }

    public IEnumerable<ColumnExpression> GetSelectableColumns()
    {
        if (string.IsNullOrEmpty(Alias))
        {
            return Enumerable.Empty<ColumnExpression>();
        }
        return ColumnAliasClause.ColumnAliases.Select(column => new ColumnExpression(Alias, column));
    }
}
