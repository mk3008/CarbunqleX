﻿using Carbunqlex.Expressions;
using Carbunqlex.Lexing;

namespace Carbunqlex.Parsing.Expressions;

public class NormalizeExpressionParser
{
    public static NormalizeExpression Parse(SqlTokenizer tokenizer)
    {
        tokenizer.Read("normalize");
        tokenizer.Read(TokenType.OpenParen);

        var originalText = ValueExpressionParser.Parse(tokenizer);

        if (tokenizer.TryPeek(out var token) && token.Type == TokenType.Comma)
        {
            tokenizer.CommitPeek();
            var form = tokenizer.Read(TokenType.Literal).Value;
            tokenizer.Read(TokenType.CloseParen);
            return new NormalizeExpression(originalText, form);
        }

        tokenizer.Read(TokenType.CloseParen);
        return new NormalizeExpression(originalText);
    }
}
