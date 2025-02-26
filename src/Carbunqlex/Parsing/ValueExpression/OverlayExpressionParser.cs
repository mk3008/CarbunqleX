﻿using Carbunqlex.Lexing;
using Carbunqlex.ValueExpressions;

namespace Carbunqlex.Parsing.ValueExpression;

public static class OverlayExpressionParser
{
    public static OverlayExpression Parse(SqlTokenizer tokenizer)
    {
        tokenizer.Read("overlay");
        tokenizer.Read(TokenType.OpenParen);

        var originalText = ValueExpressionParser.Parse(tokenizer);

        tokenizer.Read("placing");
        var newSubstring = ValueExpressionParser.Parse(tokenizer);

        tokenizer.Read("from");
        var start = ValueExpressionParser.Parse(tokenizer);

        if (tokenizer.Peek().CommandOrOperatorText == "for")
        {
            tokenizer.CommitPeek();
            var count = ValueExpressionParser.Parse(tokenizer);
            tokenizer.Read(TokenType.CloseParen);
            return new OverlayExpression(originalText, newSubstring, start, count);
        }

        tokenizer.Read(TokenType.CloseParen);
        return new OverlayExpression(originalText, newSubstring, start);
    }
}
