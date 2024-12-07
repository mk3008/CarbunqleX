using Carbunqlex.ValueExpressions;
using System.Text;

namespace Carbunqlex.DatasourceExpressions;

public class FunctionSource : IDatasource, ISqlComponent
{
    public string FunctionName { get; set; }
    public List<IValueExpression> Arguments { get; set; }
    public string Alias { get; set; }
    public ColumnAliases ColumnAliases { get; set; }

    public FunctionSource(string functionName, IEnumerable<IValueExpression> arguments, string alias, ColumnAliases columnAliases)
    {
        FunctionName = functionName;
        Arguments = arguments.ToList();
        Alias = alias;
        ColumnAliases = columnAliases;
    }

    public FunctionSource(string functionName, IEnumerable<IValueExpression> arguments, string alias)
    {
        FunctionName = functionName;
        Arguments = arguments.ToList();
        Alias = alias;
        ColumnAliases = new ColumnAliases(Enumerable.Empty<string>());
    }

    public FunctionSource(string functionName, string alias)
    {
        FunctionName = functionName;
        Arguments = new List<IValueExpression>();
        Alias = alias;
        ColumnAliases = new ColumnAliases(Enumerable.Empty<string>());
    }

    public string ToSql()
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
        sb.Append(string.Join(", ", Arguments.Select(arg => arg.ToSql())));
        sb.Append(") as ");
        sb.Append(Alias);
        sb.Append(ColumnAliases.ToSql());
        return sb.ToString();
    }

    public IEnumerable<Lexeme> GetLexemes()
    {
        var lexemes = new List<Lexeme>
        {
            new Lexeme(LexType.Keyword, FunctionName),
            new Lexeme(LexType.Identifier, Alias)
        };
        lexemes.AddRange(Arguments.SelectMany(arg => arg.GetLexemes()));
        lexemes.AddRange(ColumnAliases.GetLexemes());
        return lexemes;
    }
}
