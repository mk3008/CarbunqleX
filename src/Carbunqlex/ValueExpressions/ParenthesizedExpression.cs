﻿using System.Text;

namespace Carbunqlex.ValueExpressions;

public class ParenthesizedExpression : IValueExpression
{
    public IValueExpression InnerExpression { get; set; }

    public ParenthesizedExpression(IValueExpression innerExpression)
    {
        InnerExpression = innerExpression;
    }

    public string DefaultName => InnerExpression.DefaultName;

    public IEnumerable<Lexeme> GetLexemes()
    {
        yield return new Lexeme(LexType.OpenParen, "(");
        foreach (var lexeme in InnerExpression.GetLexemes())
        {
            yield return lexeme;
        }
        yield return new Lexeme(LexType.CloseParen, ")");
    }

    public string ToSql()
    {
        var sb = new StringBuilder();
        sb.Append("(");
        sb.Append(InnerExpression.ToSql());
        sb.Append(")");
        return sb.ToString();
    }
}
