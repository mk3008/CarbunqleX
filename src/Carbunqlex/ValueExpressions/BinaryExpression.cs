﻿using System.Text;

namespace Carbunqlex.ValueExpressions;

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

    public IEnumerable<Lexeme> GetLexemes()
    {
        foreach (var lexeme in Left.GetLexemes())
        {
            yield return lexeme;
        }
        yield return new Lexeme(LexType.Operator, Operator);
        foreach (var lexeme in Right.GetLexemes())
        {
            yield return lexeme;
        }
    }

    public string ToSql()
    {
        var sb = new StringBuilder();
        sb.Append(Left.ToSql());
        sb.Append(" ");
        sb.Append(Operator);
        sb.Append(" ");
        sb.Append(Right.ToSql());
        return sb.ToString();
    }
}
