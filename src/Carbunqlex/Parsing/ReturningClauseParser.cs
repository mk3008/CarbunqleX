﻿using Carbunqlex.Clauses;

namespace Carbunqlex.Parsing;

internal static class ReturningClauseParser
{
    public static ReturningClause Parse(SqlTokenizer tokenizer)
    {
        tokenizer.Read("returning");

        // Parse expressions
        var expressions = new List<SelectExpression>();
        while (true)
        {
            expressions.Add(SelectExpressionParser.Parse(tokenizer));
            if (tokenizer.IsEnd)
            {
                break;
            }
            if (tokenizer.Peek().Type == TokenType.Comma)
            {
                tokenizer.CommitPeek();
                continue;
            }
            break;
        }
        return new ReturningClause(expressions);
    }
}
