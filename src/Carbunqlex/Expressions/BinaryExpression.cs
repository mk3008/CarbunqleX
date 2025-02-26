using Carbunqlex.Lexing;

namespace Carbunqlex.Expressions;

/// <summary>
/// Represents a binary expression, which consists of a left operand, an operator, and a right operand.
/// This class can be used to represent both arithmetic and logical expressions.
/// </summary>
public class BinaryExpression : IValueExpression
{
    public string Operator { get; set; }
    public IValueExpression Left { get; set; }
    public IValueExpression Right { get; set; }

    public BinaryExpression(string @operator, IValueExpression left, IValueExpression right)
    {
        Operator = @operator;
        Left = left;
        Right = right;
    }

    public string DefaultName => Left.DefaultName;

    public bool MightHaveQueries => Left.MightHaveQueries || Right.MightHaveQueries;

    public IEnumerable<Token> GenerateTokensWithoutCte()
    {
        foreach (var lexeme in Left.GenerateTokensWithoutCte())
        {
            yield return lexeme;
        }
        yield return new Token(TokenType.Operator, Operator);
        foreach (var lexeme in Right.GenerateTokensWithoutCte())
        {
            yield return lexeme;
        }
    }

    public string ToSqlWithoutCte()
    {
        // If the operator is "::", then this is a Postgres type cast expression.
        // Inserting spaces is not syntactically problematic,
        // but it is generally written without spaces, so we follow that convention.
        if (Operator == "::")
        {
            return $"{Left.ToSqlWithoutCte()}{Operator}{Right.ToSqlWithoutCte()}";
        }
        else
        {
            return $"{Left.ToSqlWithoutCte()} {Operator} {Right.ToSqlWithoutCte()}";
        }
    }

    public IEnumerable<ISelectQuery> GetQueries()
    {
        var queries = new List<ISelectQuery>();

        if (Left.MightHaveQueries)
        {
            queries.AddRange(Left.GetQueries());
        }
        if (Right.MightHaveQueries)
        {
            queries.AddRange(Right.GetQueries());
        }

        return queries;
    }

    public IEnumerable<ColumnExpression> ExtractColumnExpressions()
    {
        var columns = new List<ColumnExpression>();
        columns.AddRange(Left.ExtractColumnExpressions());
        columns.AddRange(Right.ExtractColumnExpressions());
        return columns;
    }
}
