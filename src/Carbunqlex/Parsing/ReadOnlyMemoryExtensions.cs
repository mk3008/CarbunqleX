namespace Carbunqlex.Parsing;

/// <summary>
/// Lexeme reading extensions for <see cref="ReadOnlyMemory{T}"/>.
/// </summary>
public static class ReadOnlyMemoryExtensions
{
    /// <summary>
    /// Read a lexeme from the memory.
    /// </summary>
    /// <param name="memory">The memory to read from.</param>
    /// <param name="previousTokenCommandText">The command text of the previous token. Used to distinguish whether square brackets are escape characters in SQL Server or array notation in Postgres.</param>
    /// <param name="start">The start position for reading.</param>
    /// <param name="end">The end position of the lexeme.</param>
    /// <returns>The read token.</returns>
    /// <exception cref="InvalidOperationException">Thrown when an invalid lexeme is encountered.</exception>
    public static Token ReadLexeme(this ReadOnlyMemory<char> memory, Token? previousToken, int start, out int end)
    {
        end = start;
        var p = start;
        var lexeme = string.Empty;

        // Skip white spaces and comments
        memory.SkipWhiteSpacesAndComments(ref p);

        // comma
        if (memory.StartWith(",", p, out p))
        {
            end = p;
            return new Token(TokenType.Comma, ",", ",");
        }

        // numeric prefix (0x, 0b, 0o)
        if (memory.TryReadNumericPrefix(p, out p, out lexeme))
        {
            memory.SkipWhiteSpacesAndComments(ref p);
            var raw = memory.Slice(start, p - start).ToString();
            end = p;
            return new Token(TokenType.Constant, lexeme, raw, string.Empty);
        }

        // digit (Prioritize digit check over dot check because there are numbers that start with a dot)
        if (memory.TryReadDigit(p, out p, out lexeme))
        {
            memory.SkipWhiteSpacesAndComments(ref p);
            var raw = memory.Slice(start, p - start).ToString();
            end = p;
            return new Token(TokenType.Constant, lexeme, raw, string.Empty);
        }

        // dot
        if (memory.StartWith(".", p, out p))
        {
            end = p;
            return new Token(TokenType.Dot, ".", ".");
        }

        // single quote 
        if (memory.TryReadSingleQuoteLexeme(p, out p, out lexeme))
        {
            memory.SkipWhiteSpacesAndComments(ref p);
            var raw = memory.Slice(start, p - start).ToString();
            end = p;
            return new Token(TokenType.Constant, lexeme, raw, string.Empty);
        }

        // double quotes (used as the escape symbol in Postgres)
        if (memory.TryReadEnclosedLexeme(p, '"', '"', out p, out lexeme))
        {
            memory.SkipWhiteSpacesAndComments(ref p);
            var raw = memory.Slice(start, p - start).ToString();
            end = p;
            return new Token(TokenType.Identifier, lexeme, raw, string.Empty);
        }

        // backticks (used as the escape symbol in MySQL)
        if (memory.TryReadEnclosedLexeme(p, '`', '`', out p, out lexeme))
        {
            memory.SkipWhiteSpacesAndComments(ref p);
            var raw = memory.Slice(start, p - start).ToString();
            end = p;
            return new Token(TokenType.Identifier, lexeme, raw, string.Empty);
        }

        // square brackets (used as the escape symbol in SQL Server)
        if ((previousToken.HasValue == false || previousToken.Value.CommandOrOperatorText != "array") && memory.TryReadEnclosedLexeme(p, '[', ']', out p, out lexeme))
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
            return new Token(TokenType.OpenParen, "(", "(");
        }

        // close parenthesis
        if (memory.StartWith(")", p, out p))
        {
            memory.SkipWhiteSpacesAndComments(ref p);
            end = p;
            return new Token(TokenType.CloseParen, ")", ")");
        }

        // open bracket
        if (memory.StartWith("[", p, out p))
        {
            memory.SkipWhiteSpacesAndComments(ref p);
            end = p;
            return new Token(TokenType.OpenBracket, "[", "[");
        }

        // close bracket
        if (memory.StartWith("]", p, out p))
        {
            memory.SkipWhiteSpacesAndComments(ref p);
            end = p;
            return new Token(TokenType.CloseBracket, "]", "]");
        }

        // Postgres parameter $ (記号が連続出現する場合はパラメータとはみなさない)
        if (memory.StartWith("$", p, out _) && !memory.IsMultipleSymbol(p + 1))
        {
            p++;
            memory.TryReadWord(p, out p, out var name);
            memory.SkipWhiteSpacesAndComments(ref p);
            var raw = memory.Slice(start, p - start).ToString();
            end = p;
            return new Token(TokenType.Parameter, "$" + name, raw, string.Empty);
        }

        // SQLServe parameter @ (記号が連続出現する場合はパラメータとはみなさない)
        if (memory.StartWith("@", p, out _) && !memory.IsMultipleSymbol(p + 1))
        {
            p++;
            memory.TryReadWord(p, out p, out var name);
            memory.SkipWhiteSpacesAndComments(ref p);
            var raw = memory.Slice(start, p - start).ToString();
            end = p;
            return new Token(TokenType.Parameter, "@" + name, raw, string.Empty);
        }

        // Postgres parameter : (記号が連続出現する場合はパラメータとはみなさない)
        if (memory.StartWith(":", p, out _) && !memory.IsMultipleSymbol(p + 1))
        {
            p++;
            memory.TryReadWord(p, out p, out var name);
            memory.SkipWhiteSpacesAndComments(ref p);
            var raw = memory.Slice(start, p - start).ToString();
            end = p;
            return new Token(TokenType.Parameter, ":" + name, raw, string.Empty);
        }

        // SQLite parameter ? (記号のあとは空白または終端であること)
        if (memory.StartWith("?", p, out _) && (memory.IsWhiteSpace(p + 1) || memory.IsEnd(p + 1)))
        {
            p++;
            // ただし、previous が identifier の 場合は 演算子として扱うこと
            if (previousToken?.Type == TokenType.Identifier)
            {
                end = p;
                return new Token(TokenType.Operator, "?");
            }
            memory.SkipWhiteSpacesAndComments(ref p);
            var raw = memory.Slice(start, p - start).ToString();
            end = p;
            return new Token(TokenType.Parameter, "?", raw, string.Empty);
        }

        // symbol
        if (memory.TryReadSymbol(p, out p, out lexeme))
        {
            if (previousToken?.Type == TokenType.Dot)
            {
                // Recognize `*` as an identifier for retrieving all columns
                memory.SkipWhiteSpacesAndComments(ref p);
                var raw = memory.Slice(start, p - start).ToString();
                end = p;
                return new Token(TokenType.Identifier, lexeme, raw, string.Empty);
            }
            else
            {
                memory.SkipWhiteSpacesAndComments(ref p);
                var raw = memory.Slice(start, p - start).ToString();
                end = p;
                return new Token(TokenType.Operator, lexeme, raw, string.Empty);
            }
        }

        // escaped string (e.g. E'abc', U&'abc')
        if (memory.TryReadEscapedString(p, out p, out lexeme))
        {
            end = p;
            return new Token(TokenType.EscapedStringConstant, lexeme, lexeme, string.Empty);
        }

        // word
        if (memory.TryReadWord(p, out p, out lexeme))
        {
            memory.SkipWhiteSpacesAndComments(ref p);

            var normalized = lexeme.ToLower();

            if (SqlKeyword.IdentifierKeywordNodes.ContainsKey(normalized))
            {
                // e.g. double precision, timestamp with time zone
                var node = SqlKeyword.IdentifierKeywordNodes[normalized];
                return memory.ParseKeywordLexeme(start, p, lexeme, TokenType.Identifier, node, out end);
            }
            else if (SqlKeyword.OperatorKeywordNodes.ContainsKey(normalized))
            {
                var node = SqlKeyword.OperatorKeywordNodes[normalized];
                return memory.ParseKeywordLexeme(start, p, lexeme, TokenType.Operator, node, out end);
            }
            else if (SqlKeyword.ConstantValueKeywordNodes.ContainsKey(normalized))
            {
                var node = SqlKeyword.ConstantValueKeywordNodes[normalized];
                return memory.ParseKeywordLexeme(start, p, lexeme, TokenType.Constant, node, out end);
            }
            else if (SqlKeyword.JoinCommandKeywordNodes.ContainsKey(normalized))
            {
                var node = SqlKeyword.JoinCommandKeywordNodes[normalized];
                return memory.ParseKeywordLexeme(start, p, lexeme, TokenType.Command, node, out end);
            }
            else if (SqlKeyword.UnionCommandKeywordNodes.ContainsKey(normalized))
            {
                var node = SqlKeyword.UnionCommandKeywordNodes[normalized];
                return memory.ParseKeywordLexeme(start, p, lexeme, TokenType.Command, node, out end);
            }
            else if (!SqlKeyword.CommandKeywordNodes.ContainsKey(normalized))
            {
                memory.SkipWhiteSpacesAndComments(ref p);
                var raw = memory.Slice(start, p - start).ToString();
                end = p;
                return new Token(TokenType.Identifier, lexeme, raw, string.Empty);
            }
            else
            {
                // Check if the command consists of multiple words
                var node = SqlKeyword.CommandKeywordNodes[normalized];
                return memory.ParseKeywordLexeme(start, p, lexeme, TokenType.Command, node, out end);
            }
        }

        throw new InvalidOperationException($"Invalid lexeme at position {p}");
    }

    /// <summary>
    /// Parse a keyword lexeme.
    /// </summary>
    /// <param name="memory">The memory to read from.</param>
    /// <param name="start">The start position for reading.</param>
    /// <param name="currentPosition">The current position in the memory.</param>
    /// <param name="lexemeBuffer">The buffer to store the lexeme.</param>
    /// <param name="tokenType">The type of the token.</param>
    /// <param name="node">The current SQL keyword node.</param>
    /// <param name="end">The end position of the lexeme.</param>
    /// <returns>The parsed token.</returns>
    /// <exception cref="NotSupportedException">Thrown when an unsupported keyword is encountered.</exception>
    private static Token ParseKeywordLexeme(this ReadOnlyMemory<char> memory, int start, int currentPosition, string lexemeBuffer, TokenType tokenType, SqlKeywordNode node, out int end)
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

    /// <summary>
    /// Attempts to skip white spaces. 
    /// If the current position is a white space, it advances to the next non-white space character.
    /// Returns true if any skipping occurred.
    /// </summary>
    /// <param name="memory">The memory to read from.</param>
    /// <param name="position">The position to start reading from.</param>
    /// <returns>Indicates whether skipping occurred.</returns>
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

    /// <summary>
    /// Attempts to skip a block comment. 
    /// If the current position is a block comment, it advances to the end of the comment.
    /// Returns true if any skipping occurred.
    /// </summary>
    /// <param name="memory">The memory to read from.</param>
    /// <param name="position">The position to start reading from.</param>
    /// <returns>Indicates whether skipping occurred.</returns>
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

    /// <summary>
    /// Attempts to skip a line comment.
    /// If the current position is a line comment, it advances to the end of the line.
    /// Returns true if any skipping occurred.
    /// </summary>
    /// <param name="memory">The memory to read from.</param>
    /// <param name="position">The position to start reading from.</param>
    /// <returns>Indicates whether skipping occurred.</returns>
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

    /// <summary>
    /// Skip white spaces and comments.
    /// </summary>
    /// <param name="memory">The memory to read from.</param>
    /// <param name="position">The position to start reading from.</param>
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

    /// <summary>
    /// Determines whether the memory starts with the specified value.
    /// </summary>
    /// <param name="memory">The memory to read from.</param>
    /// <param name="expectedValue">The expected string.</param>
    /// <param name="start">The start position.</param>
    /// <param name="endPosition">The detected end position.</param>
    /// <param name="ignoreCase">Indicates whether to ignore case when comparing strings.</param>
    /// <returns>True if the memory starts with the specified value; otherwise, false.</returns>
    private static bool StartWith(this ReadOnlyMemory<char> memory, string expectedValue, int start, out int endPosition, bool ignoreCase = false)
    {
        endPosition = start;

        if (start < 0 || start > memory.Length - expectedValue.Length)
        {
            return false;
        }

        var span = memory.Span.Slice(start, expectedValue.Length);

        if (ignoreCase)
        {
            if (span.ToString().Equals(expectedValue, StringComparison.OrdinalIgnoreCase))
            {
                endPosition = start + expectedValue.Length;
                return true;
            }
        }
        else
        {
            if (span.SequenceEqual(expectedValue.AsSpan()))
            {
                endPosition = start + expectedValue.Length;
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Read a single quote lexeme from the memory.
    /// </summary>
    /// <param name="memory">The memory to read from.</param>
    /// <param name="start">The start position.</param>
    /// <param name="endPosition">The detected end position.</param>
    /// <param name="lexeme">The detected lexeme.</param>
    /// <returns>True if a single quote lexeme is successfully read; otherwise, false.</returns>
    private static bool TryReadSingleQuoteLexeme(this ReadOnlyMemory<char> memory, int start, out int endPosition, out string lexeme)
    {
        endPosition = start;
        var p = start;

        // Check if the first character is a single quote
        if (memory.Span[p] != '\'')
        {
            lexeme = string.Empty;
            return false;
        }
        p++;

        // Read until the closing single quote is found
        while (p < memory.Length)
        {
            if (memory.Span[p] == '\'')
            {
                // Handle escaped single quotes
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

        // Extract the lexeme
        lexeme = memory.Slice(start, p - start).ToString();
        endPosition = p;
        return true;
    }

    /// <summary>
    /// Read a numeric prefix from the memory.
    /// e.g. 0x, 0b, 0o
    /// </summary>
    /// <param name="memory">The memory to read from.</param>
    /// <param name="start">The start position.</param>
    /// <param name="endPosition">The detected end position.</param>
    /// <param name="lexeme">The detected lexeme.</param>
    /// <returns>True if a numeric prefix is successfully read; otherwise, false.</returns>
    /// <exception cref="InvalidOperationException">Thrown when an invalid numeric prefix is encountered.</exception>
    private static bool TryReadNumericPrefix(this ReadOnlyMemory<char> memory, int start, out int endPosition, out string lexeme)
    {
        foreach (var keyword in SqlKeyword.NumericPrefixKeywords)
        {
            if (memory.StartWith(keyword, start, out var p, true))
            {
                // Move by the length of the prefix
                p += keyword.Length;

                if (TryReadDigit(memory, p, out p, out _))
                {
                    lexeme = memory.Slice(start, p - start).ToString();
                    endPosition = p;
                    return true;
                }
                throw new InvalidOperationException($"Invalid numeric prefix at position {start}");
            }
        }
        lexeme = string.Empty;
        endPosition = start;
        return false;
    }

    /// <summary>
    /// Read a digit from the memory.
    /// Also read notation starting with a dot as a number.
    /// Underscores are recognized as digit separators.
    /// </summary>
    /// <param name="memory">The memory to read from.</param>
    /// <param name="start">The start position.</param>
    /// <param name="endPosition">The detected end position.</param>
    /// <param name="lexeme">The detected lexeme.</param>
    /// <returns>True if a digit is successfully read; otherwise, false.</returns>
    private static bool TryReadDigit(this ReadOnlyMemory<char> memory, int start, out int endPosition, out string lexeme)
    {
        endPosition = start;
        var p = start;
        var hasDot = false;
        var hasExponent = false;
        var isFirstCharDot = memory.Span[p] == '.';

        // Check if the first character is not a digit or a dot
        if (!char.IsDigit(memory.Span[p]) && !isFirstCharDot)
        {
            lexeme = string.Empty;
            return false;
        }

        // If the first character is a dot, check the next character
        if (isFirstCharDot)
        {
            p++;
            if (p < memory.Length && char.IsDigit(memory.Span[p]))
            {
                p++;
            }
            else
            {
                lexeme = string.Empty;
                return false;
            }
        }

        // Read digits, dots, and exponents
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
            else if ((memory.Span[p] == 'e' || memory.Span[p] == 'E') && !hasExponent)
            {
                hasExponent = true;
                p++;
                if (p < memory.Length && (memory.Span[p] == '+' || memory.Span[p] == '-'))
                {
                    p++;
                }
            }
            else if (memory.Span[p] == '_')
            {
                // Skip the underscore
                p++;
            }
            else
            {
                break;
            }
        }
        lexeme = memory.Slice(start, p - start).ToString();
        endPosition = p;
        return true;
    }

    /// <summary>
    /// Read a symbol from the memory.
    /// </summary>
    /// <param name="memory">The memory to read from.</param>
    /// <param name="start">The start position.</param>
    /// <param name="endPosition">The detected end position.</param>
    /// <param name="lexeme">The detected lexeme.</param>
    /// <returns>True if a symbol is successfully read; otherwise, false.</returns>
    private static bool TryReadSymbol(this ReadOnlyMemory<char> memory, int start, out int endPosition, out string lexeme)
    {
        endPosition = start;
        var p = start;

        if (memory.Span[p].IsSingleSymbol())
        {
            p++;
            lexeme = memory.Slice(start, p - start).ToString();
            endPosition = p;
            return true;
        }

        if (!memory.Span[p].IsMultipleSymbol())
        {
            lexeme = string.Empty;
            return false;
        }
        p++;
        while (p < memory.Length && memory.Span[p].IsMultipleSymbol())
        {
            p++;
        }
        lexeme = memory.Slice(start, p - start).ToString();
        endPosition = p;
        return true;
    }

    /// <summary>
    /// Read an enclosed lexeme from the memory.
    /// e.g. "abc", 'abc', [abc]
    /// </summary>
    /// <param name="memory">The memory to read from.</param>
    /// <param name="start">The start position.</param>
    /// <param name="startChar">The character that marks the beginning of the enclosed lexeme.</param>
    /// <param name="endChar">The character that marks the end of the enclosed lexeme.</param>
    /// <param name="endPosition">The detected end position.</param>
    /// <param name="lexeme">The detected lexeme.</param>
    /// <returns>True if an enclosed lexeme is successfully read; otherwise, false.</returns>
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

    /// <summary>
    /// Read an escaped string from the memory.
    /// e.g. E'abc', U&'abc'
    /// </summary>
    /// <param name="memory">The memory to read from.</param>
    /// <param name="start">The start position.</param>
    /// <param name="endPosition">The detected end position.</param>
    /// <param name="lexeme">The detected lexeme.</param>
    /// <returns>True if an escaped string is successfully read; otherwise, false.</returns>
    private static bool TryReadEscapedString(this ReadOnlyMemory<char> memory, int start, out int endPosition, out string lexeme)
    {
        // Keywords such as "E" and "U&" are included here.
        // Strings starting with these keywords are treated as string literals.
        foreach (var keyword in SqlKeyword.EscapeLiteralKeywords)
        {
            if (memory.StartWith(keyword, start, out var p, true))
            {
                // Read until a single quote is found
                // However, if single quotes are consecutive, treat them as escape characters
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
        }
        lexeme = string.Empty;
        endPosition = start;
        return false;
    }

    /// <summary>
    /// Read a word from the memory.
    /// </summary>
    /// <param name="memory">The memory to read from.</param>
    /// <param name="start">The start position.</param>
    /// <param name="endPosition">The detected end position.</param>
    /// <param name="word">The detected word.</param>
    /// <returns>True if a word is successfully read; otherwise, false.</returns>
    private static bool TryReadWord(this ReadOnlyMemory<char> memory, int start, out int endPosition, out string word)
    {
        endPosition = start;
        var p = start;

        if (p >= memory.Length)
        {
            word = string.Empty;
            return false;
        }

        // Check if the first character is not a letter
        if (memory.Span[p].IsWhiteSpace() || memory.Span[p].IsSingleSymbol() || memory.Span[p].IsMultipleSymbol())
        {
            word = string.Empty;
            return false;
        }

        //　Read until a white space or symbol is found
        while (p < memory.Length && !memory.Span[p].IsWhiteSpace() && !memory.Span[p].IsSingleSymbol() && !memory.Span[p].IsMultipleSymbol())
        {
            p++;
        }

        word = memory.Slice(start, p - start).ToString();
        endPosition = p;
        return true;
    }

    private static bool IsMultipleSymbol(this ReadOnlyMemory<char> memory, int position)
    {
        if (position >= memory.Length)
        {
            return false;
        }
        return memory.Span[position].IsMultipleSymbol();
    }

    private static bool IsWhiteSpace(this ReadOnlyMemory<char> memory, int position)
    {
        if (position >= memory.Length)
        {
            return false;
        }
        return memory.Span[position].IsWhiteSpace();
    }

    private static bool IsEnd(this ReadOnlyMemory<char> memory, int position)
    {
        return position >= memory.Length;
    }
}
