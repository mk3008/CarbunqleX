namespace Carbunqlex.Parsing;

public static class ReadOnlyMemoryExtensions
{
    public static Token ReadLexeme(this ReadOnlyMemory<char> memory, string previousIdentifier, int start, out int end)
    {
        end = start;
        var p = start;
        var lexeme = string.Empty;

        // Skip white spaces and comments
        memory.SkipWhiteSpacesAndComments(ref p);

        // comma
        if (memory.StartWith(",", p, out p))
        {
            //memory.SkipWhiteSpacesAndComments(ref p);
            end = p;
            return new Token(TokenType.Comma, ",", string.Empty);
        }

        // dot
        if (memory.StartWith(".", p, out p))
        {
            //memory.SkipWhiteSpacesAndComments(ref p);
            end = p;
            return new Token(TokenType.Dot, ".", string.Empty);
        }

        // single quote 
        if (memory.TryReadSingleQuoteLexeme(p, out p, out lexeme))
        {
            memory.SkipWhiteSpacesAndComments(ref p);
            var raw = memory.Slice(start, p - start).ToString();
            end = p;
            return new Token(TokenType.Constant, lexeme, raw, string.Empty);
        }

        // double quote
        if (memory.TryReadEnclosedLexeme(p, '"', '"', out p, out lexeme))
        {
            memory.SkipWhiteSpacesAndComments(ref p);
            var raw = memory.Slice(start, p - start).ToString();
            end = p;
            return new Token(TokenType.Constant, lexeme, raw, string.Empty);
        }

        // back quote
        if (memory.TryReadEnclosedLexeme(p, '`', '`', out p, out lexeme))
        {
            memory.SkipWhiteSpacesAndComments(ref p);
            var raw = memory.Slice(start, p - start).ToString();
            end = p;
            return new Token(TokenType.Identifier, lexeme, raw, string.Empty);
        }

        // square brackets (for SQL Server)
        if (previousIdentifier != "array" && memory.TryReadEnclosedLexeme(p, '[', ']', out p, out lexeme))
        {
            memory.SkipWhiteSpacesAndComments(ref p);
            var raw = memory.Slice(start, p - start).ToString();
            end = p;
            return new Token(TokenType.Identifier, lexeme, raw, string.Empty);
        }

        // open parenthesis
        if (memory.StartWith("(", p, out p))
        {
            memory.SkipWhiteSpacesAndComments(ref p);
            end = p;
            return new Token(TokenType.OpenParen, "(", string.Empty);
        }

        // close parenthesis
        if (memory.StartWith(")", p, out p))
        {
            memory.SkipWhiteSpacesAndComments(ref p);
            end = p;
            return new Token(TokenType.CloseParen, ")", string.Empty);
        }

        // open bracket
        if (memory.StartWith("[", p, out p))
        {
            memory.SkipWhiteSpacesAndComments(ref p);
            end = p;
            return new Token(TokenType.OpenBracket, "[", string.Empty);
        }

        // close bracket
        if (memory.StartWith("]", p, out p))
        {
            memory.SkipWhiteSpacesAndComments(ref p);
            end = p;
            return new Token(TokenType.CloseBracket, "]", string.Empty);
        }

        // parameter @
        if (memory.StartWith("@", p, out p))
        {
            memory.TryReadWord(p, out p, out var name);
            memory.SkipWhiteSpacesAndComments(ref p);
            var raw = memory.Slice(start, p - start).ToString();
            end = p;
            return new Token(TokenType.Parameter, "@" + name, raw, string.Empty);
        }

        // parameter :
        if (memory.StartWith(":", p, out p))
        {
            memory.TryReadWord(p, out p, out var name);
            memory.SkipWhiteSpacesAndComments(ref p);
            var raw = memory.Slice(start, p - start).ToString();
            end = p;
            return new Token(TokenType.Parameter, ":" + name, raw, string.Empty);
        }

        // parameter $
        if (memory.StartWith("$", p, out p))
        {
            memory.TryReadWord(p, out p, out var name);
            memory.SkipWhiteSpacesAndComments(ref p);
            var raw = memory.Slice(start, p - start).ToString();
            end = p;
            return new Token(TokenType.Parameter, "$" + name, raw, string.Empty);
        }

        // symbol
        if (memory.TryReadSymbol(p, out p, out lexeme))
        {
            memory.SkipWhiteSpacesAndComments(ref p);
            var raw = memory.Slice(start, p - start).ToString();
            end = p;
            return new Token(TokenType.Operator, lexeme, raw, string.Empty);
        }

        // digit
        if (memory.TryReadDigit(p, out p, out lexeme))
        {
            memory.SkipWhiteSpacesAndComments(ref p);
            var raw = memory.Slice(start, p - start).ToString();
            end = p;
            return new Token(TokenType.Constant, lexeme, raw, string.Empty);
        }

        // word
        if (memory.TryReadWord(p, out p, out lexeme))
        {
            memory.SkipWhiteSpacesAndComments(ref p);

            var normalized = lexeme.ToLower();

            // Prioritize constant determination
            if (SqlKeyword.ConstantValueKeywords.ContainsKey(normalized))
            {
                var node = SqlKeyword.ConstantValueKeywords[normalized];
                return memory.ParseKeywordLexeme(start, p, lexeme, TokenType.Constant, node, out end);
            }
            // If not a constant or command, treat as an identifier
            else if (!SqlKeyword.CommandKeywords.ContainsKey(normalized))
            {
                memory.SkipWhiteSpacesAndComments(ref p);
                var raw = memory.Slice(start, p - start).ToString();
                end = p;
                return new Token(TokenType.Identifier, lexeme, raw, string.Empty);
            }
            else
            {
                // Check if the command consists of multiple words
                var node = SqlKeyword.CommandKeywords[normalized];
                return memory.ParseKeywordLexeme(start, p, lexeme, TokenType.Command, node, out end);
            }
        }

        throw new InvalidOperationException($"Invalid lexeme at position {p}");
    }

    public static Token ParseKeywordLexeme(this ReadOnlyMemory<char> memory, int start, int currentPosition, string lexemeBuffer, TokenType tokenType, SqlKeywordNode node, out int end)
    {
        while (true)
        {
            if (!memory.TryReadWord(currentPosition, out var nextPosition, out var nextLexeme))
            {
                var raw = memory.Slice(start, currentPosition - start).ToString();
                end = currentPosition;
                return new Token(tokenType, lexemeBuffer, raw, lexemeBuffer);
            }

            // If the read lexeme does not exist in the child node
            if (!node.Children.ContainsKey(nextLexeme.ToLower()))
            {
                // If it allows itself to be a terminal node
                // Return the cache read up to the previous time
                if (node.IsTerminal)
                {
                    var raw = memory.Slice(start, currentPosition - start).ToString();
                    end = currentPosition;
                    return new Token(tokenType, lexemeBuffer, raw, lexemeBuffer);
                }

                // If it does not allow itself to be a terminal node
                lexemeBuffer += " " + nextLexeme;
                throw new NotSupportedException($"Unsupported keyword '{lexemeBuffer}' of type '{tokenType}' found between positions {start} and {nextPosition}.");
            }

            // Record the coordinates of the lexeme and skip unnecessary characters that follow
            currentPosition = nextPosition;
            memory.SkipWhiteSpacesAndComments(ref currentPosition);

            // concat lexeme
            lexemeBuffer += " " + nextLexeme;
            node = node.Children[nextLexeme.ToLower()];

            // If there are no child nodes, it is clear that it is a terminal node, so return the token
            if (node.Children.Count == 0)
            {
                var raw = memory.Slice(start, currentPosition - start).ToString();
                end = currentPosition;
                return new Token(tokenType, lexemeBuffer, raw, lexemeBuffer);
            }
        }
        throw new NotSupportedException($"Unsupported keyword '{lexemeBuffer}' of type '{tokenType}' found at position {start}.");
    }

    private static bool TrySkipWhiteSpaces(this ReadOnlyMemory<char> memory, ref int position)
    {
        if (position < memory.Span.Length && memory.Span[position].IsWhiteSpace())
        {
            position += 1;
            while (position < memory.Span.Length && memory.Span[position].IsWhiteSpace())
            {
                position++;
            }
            return true;
        }
        return false;
    }

    private static bool TrySkipBlockComment(this ReadOnlyMemory<char> memory, ref int position)
    {
        if (position + 1 < memory.Length && memory.Span[position] == '/' && memory.Span[position + 1] == '*')
        {
            position += 2;
            while (position + 1 < memory.Length)
            {
                if (memory.Span[position] == '*' && memory.Span[position + 1] == '/')
                {
                    position += 2;
                    break;
                }
                position++;
            }
            return true;
        }
        return false;
    }

    private static bool TrySkipLineComment(this ReadOnlyMemory<char> memory, ref int position)
    {
        if (position + 1 < memory.Length && memory.Span[position] == '-' && memory.Span[position + 1] == '-')
        {
            position += 2;
            while (position < memory.Length && !memory.Span[position].IsLineEnd())
            {
                position++;
            }
            return true;
        }
        return false;
    }

    private static void SkipWhiteSpacesAndComments(this ReadOnlyMemory<char> memory, ref int position)
    {
        while (position < memory.Length)
        {
            if (memory.TrySkipWhiteSpaces(ref position))
            {
                continue;
            }
            else if (memory.TrySkipBlockComment(ref position))
            {
                continue;
            }
            else if (memory.TrySkipLineComment(ref position))
            {
                continue;
            }
            break;
        }
    }

    private static bool StartWith(this ReadOnlyMemory<char> memory, string value, int start, out int endPosition, bool ignoreCase = false)
    {
        endPosition = start;

        if (start < 0 || start > memory.Length - value.Length)
        {
            return false;
        }

        var span = memory.Span.Slice(start, value.Length);

        if (ignoreCase)
        {
            if (span.ToString().Equals(value, StringComparison.OrdinalIgnoreCase))
            {
                endPosition = start + value.Length;
                return true;
            }
        }
        else
        {
            if (span.SequenceEqual(value.AsSpan()))
            {
                endPosition = start + value.Length;
                return true;
            }
        }

        return false;
    }

    private static bool TryReadSingleQuoteLexeme(this ReadOnlyMemory<char> memory, int start, out int endPosition, out string lexeme)
    {
        endPosition = start;
        var p = start;

        if (memory.Span[p] != '\'')
        {
            lexeme = string.Empty;
            return false;
        }
        p++;

        while (p < memory.Length)
        {
            if (memory.Span[p] == '\'')
            {
                if (p + 1 < memory.Length && memory.Span[p + 1] == '\'')
                {
                    p += 2;
                }
                else
                {
                    p++;
                    break;
                }
            }
            else
            {
                p++;
            }
        }

        lexeme = memory.Slice(start, p - start).ToString();
        endPosition = p;
        return true;
    }

    private static bool TryReadDigit(this ReadOnlyMemory<char> memory, int start, out int endPosition, out string lexeme)
    {
        endPosition = start;
        var p = start;
        var hasDot = false;

        if (!char.IsDigit(memory.Span[p]))
        {
            lexeme = string.Empty;
            return false;
        }
        p++;

        while (p < memory.Length)
        {
            if (char.IsDigit(memory.Span[p]) || (memory.Span[p] == '.' && !hasDot))
            {
                if (memory.Span[p] == '.')
                {
                    hasDot = true;
                }
                p++;
            }
            else
            {
                break;
            }
        }
        endPosition = p;
        lexeme = memory.Slice(start, p - start).ToString();
        return true;
    }

    private static bool TryReadSymbol(this ReadOnlyMemory<char> memory, int start, out int endPosition, out string lexeme)
    {
        endPosition = start;
        var p = start;

        if (memory.Span[p].IsSingleSymbols())
        {
            p++;
            lexeme = memory.Slice(start, p - start).ToString();
            endPosition = p;
            return true;
        }

        if (!memory.Span[p].IsMultipleSymbols())
        {
            lexeme = string.Empty;
            return false;
        }
        p++;
        while (p < memory.Length && memory.Span[p].IsMultipleSymbols())
        {
            p++;
        }
        lexeme = memory.Slice(start, p - start).ToString();
        endPosition = p;
        return true;
    }

    private static bool TryReadEnclosedLexeme(this ReadOnlyMemory<char> memory, int start, char startChar, char endChar, out int endPosition, out string lexeme)
    {
        endPosition = start;
        var p = start;

        if (memory.Span[p] != startChar)
        {
            lexeme = string.Empty;
            return false;
        }
        p++;

        while (p < memory.Length)
        {
            if (memory.Span[p] == endChar)
            {
                p++;
                break;
            }
            p++;
        }

        lexeme = memory.Slice(start, p - start).ToString();
        endPosition = p;
        return true;
    }

    private static bool TryReadWord(this ReadOnlyMemory<char> memory, int start, out int endPosition, out string word)
    {
        endPosition = start;
        var p = start;

        if (p >= memory.Length)
        {
            word = string.Empty;
            return false;
        }

        if (memory.Span[p].IsWhiteSpace() || memory.Span[p].IsSingleSymbols() || memory.Span[p].IsMultipleSymbols())
        {
            word = string.Empty;
            return false;
        }

        while (p < memory.Length && !memory.Span[p].IsWhiteSpace() && !memory.Span[p].IsSingleSymbols() && !memory.Span[p].IsMultipleSymbols())
        {
            p++;
        }

        word = memory.Slice(start, p - start).ToString();
        endPosition = p;
        return true;
    }
}
