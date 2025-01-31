using Carbunqlex.ValueExpressions;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Carbunqlex.DatasourceExpressions;

public class FunctionSource : IDatasource
{
    public string FunctionName { get; set; }

    public List<IValueExpression> Arguments { get; set; }

    public string TableFullName => string.Empty;

    public string DefaultName => string.Empty;

    public bool HasWithOrdinalityKeyword { get; set; }

    public FunctionSource(string functionName, IEnumerable<IValueExpression> arguments, bool hasWithOrdinalityKeyword)
    {
        FunctionName = functionName;
        Arguments = arguments.ToList();
        HasWithOrdinalityKeyword = hasWithOrdinalityKeyword;
    }

    public FunctionSource(string functionName, IEnumerable<IValueExpression> arguments)
    {
        FunctionName = functionName;
        Arguments = arguments.ToList();
    }

    public FunctionSource(string functionName)
    {
        FunctionName = functionName;
        Arguments = new List<IValueExpression>();
        HasWithOrdinalityKeyword = false;
    }

    public string ToSqlWithoutCte()
    {
        var sb = new StringBuilder();
        sb.Append(FunctionName);
        sb.Append("(");
        sb.Append(string.Join(", ", Arguments.Select(arg => arg.ToSqlWithoutCte())));
        sb.Append(")");

        if (HasWithOrdinalityKeyword)
        {
            sb.Append(" with ordinality");
        }

        return sb.ToString();
    }

    public IEnumerable<Token> GenerateTokensWithoutCte()
    {
        var tokens = new List<Token>
        {
            new Token(TokenType.Identifier, FunctionName),
            new Token(TokenType.OpenParen, "(")
        };
        for (int i = 0; i < Arguments.Count; i++)
        {
            tokens.AddRange(Arguments[i].GenerateTokensWithoutCte());
            if (i < Arguments.Count - 1)
            {
                tokens.Add(new Token(TokenType.Comma, ","));
            }
        }
        tokens.Add(new Token(TokenType.CloseParen, ")"));

        if (HasWithOrdinalityKeyword)
        {
            tokens.Add(new Token(TokenType.Command, "with ordinality"));
        }
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
        return Enumerable.Empty<string>();
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
