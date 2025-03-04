﻿using Carbunqlex.Expressions;
using Carbunqlex.Lexing;

namespace Carbunqlex.Parsing.Expressions;

public static class CastExpressionParser
{
    public static CastExpression Parse(SqlTokenizer tokenizer)
    {
        tokenizer.Read("cast");

        tokenizer.Read(TokenType.OpenParen);

        var expression = ValueExpressionParser.Parse(tokenizer);

        tokenizer.Read("as");

        var targetType = ValueExpressionParser.Parse(tokenizer);

        tokenizer.Read(TokenType.CloseParen);

        return new CastExpression(expression, targetType.ToSqlWithoutCte());
    }
}
