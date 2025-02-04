using Carbunqlex.Clauses;

namespace Carbunqlex.Parsing;

public static class SelectExpressionParser
{
    public static SelectExpression Parse(SqlTokenizer tokenizer)
    {
        var expression = ValueExpressionParser.Parse(tokenizer);

        if (tokenizer.IsEnd)
        {
            return new SelectExpression(expression);
        }

        var next = tokenizer.Peek();
        if (next.Value == "as")
        {
            // have "as" keyword
            tokenizer.CommitPeek();
            var alias = tokenizer.Read(TokenType.Identifier).Value;
            return new SelectExpression(expression, alias);
        }
        else if (next.Type == TokenType.Identifier)
        {
            // no "as" keyword
            tokenizer.CommitPeek();
            return new SelectExpression(expression, next.Value);
        }

        // no alias
        return new SelectExpression(expression);
    }
}
