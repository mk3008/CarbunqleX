using Carbunqlex.Clauses;
using Carbunqlex.Lexing;
using System.Text;

namespace Carbunqlex.Expressions;

public class FunctionExpression : IValueExpression
{
    public IArgumentExpression Arguments { get; set; }

    public string FunctionName { get; set; }

    public IFunctionModifier? FunctionModifier { get; set; }

    /// <summary>
    /// Prefix to be added to the function name.e.g. "distinct"
    /// </summary>
    public string PrefixModifier { get; set; }

    public FunctionExpression(string functionName, string prefixModifier, IArgumentExpression arguments)
    {
        FunctionName = functionName;
        PrefixModifier = prefixModifier;
        Arguments = arguments;
        FunctionModifier = null;
    }

    public FunctionExpression(string functionName, string prefixModifier, IArgumentExpression arguments, IFunctionModifier? functionModifier)
    {
        FunctionName = functionName;
        PrefixModifier = prefixModifier;
        Arguments = arguments;
        FunctionModifier = functionModifier;
    }

    public string ToSqlWithoutCte()
    {
        var sb = new StringBuilder();
        sb.Append(FunctionName);
        sb.Append("(");
        if (!string.IsNullOrEmpty(PrefixModifier))
        {
            sb.Append(PrefixModifier).Append(" ");
        }
        sb.Append(Arguments.ToSqlWithoutCte());
        sb.Append(")");

        if (FunctionModifier != null)
        {
            sb.Append(" ").Append(FunctionModifier.ToSqlWithoutCte());
        }

        return sb.ToString();
    }

    public string DefaultName => string.Empty;

    public bool MightHaveQueries => Arguments.MightHaveQueries || (FunctionModifier?.MightHaveQueries ?? false);

    public IEnumerable<Token> GenerateTokensWithoutCte()
    {
        yield return new Token(TokenType.Command, FunctionName);
        yield return new Token(TokenType.OpenParen, "(");
        if (!string.IsNullOrEmpty(PrefixModifier))
        {
            yield return new Token(TokenType.Command, PrefixModifier);
        }
        foreach (var lexeme in Arguments.GenerateTokensWithoutCte())
        {
            yield return lexeme;
        }
        yield return new Token(TokenType.CloseParen, ")");

        if (FunctionModifier != null)
        {
            foreach (var lexeme in FunctionModifier.GenerateTokensWithoutCte())
            {
                yield return lexeme;
            }
        }
    }

    public IEnumerable<ISelectQuery> GetQueries()
    {
        var queries = new List<ISelectQuery>();

        if (Arguments.MightHaveQueries)
        {
            queries.AddRange(Arguments.GetQueries());
        }

        if (FunctionModifier != null && FunctionModifier.MightHaveQueries)
        {
            queries.AddRange(FunctionModifier.GetQueries());
        }

        return queries;
    }

    public IEnumerable<ColumnExpression> ExtractColumnExpressions()
    {
        return Arguments.ExtractColumnExpressions();
    }
}
