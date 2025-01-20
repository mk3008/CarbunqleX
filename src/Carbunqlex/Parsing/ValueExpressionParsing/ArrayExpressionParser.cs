﻿using Carbunqlex.ValueExpressions;

namespace Carbunqlex.Parsing.ValueExpressionParsing;

public static class ArrayExpressionParser
{
    private static string ParserName => nameof(ArrayExpressionParser);

    public static ArrayExpression Parse(SqlTokenizer tokenizer)
    {
        var command = tokenizer.Read(ParserName, "array");

        var args = ValueExpressionParser.ParseArguments(tokenizer, TokenType.OpenBracket, TokenType.CloseBracket);

        return new ArrayExpression(new ValueArguments(args));
    }
}
