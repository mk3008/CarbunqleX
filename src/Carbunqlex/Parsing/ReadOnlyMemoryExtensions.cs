﻿namespace Carbunqlex.Parsing;

public static class ReadOnlyMemoryExtensions
{
    public static Token ReadLexeme(this ReadOnlyMemory<char> memory, int start, out int end)
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
            return new Token(TokenType.Value, lexeme, raw, string.Empty);
        }

        // double quote
        if (memory.TryReadEnclosedLexeme(p, '"', '"', out p, out lexeme))
        {
            memory.SkipWhiteSpacesAndComments(ref p);
            var raw = memory.Slice(start, p - start).ToString();
            end = p;
            return new Token(TokenType.Value, lexeme, raw, string.Empty);
        }

        // back quote
        if (memory.TryReadEnclosedLexeme(p, '`', '`', out p, out lexeme))
        {
            memory.SkipWhiteSpacesAndComments(ref p);
            var raw = memory.Slice(start, p - start).ToString();
            end = p;
            return new Token(TokenType.Value, lexeme, raw, string.Empty);
        }

        // square brackets
        if (memory.TryReadEnclosedLexeme(p, '[', ']', out p, out lexeme))
        {
            memory.SkipWhiteSpacesAndComments(ref p);
            var raw = memory.Slice(start, p - start).ToString();
            end = p;
            return new Token(TokenType.Value, lexeme, raw, string.Empty);
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
            return new Token(TokenType.Value, lexeme, raw, string.Empty);
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

            // Check if it exists in the keyword dictionary
            if (!SqlKeyword.AllKeywords.ContainsKey(normalized))
            {
                memory.SkipWhiteSpacesAndComments(ref p);
                var raw = memory.Slice(start, p - start).ToString();
                end = p;
                return new Token(TokenType.Identifier, lexeme, raw, string.Empty);
            }

            var node = SqlKeyword.AllKeywords[normalized];

            while (true)
            {
                if (!memory.TryReadWord(p, out var tmpPosition, out var tmplexeme))
                {
                    var raw = memory.Slice(start, p - start).ToString();
                    end = p;
                    return new Token(TokenType.Keyword, lexeme, raw, string.Empty);
                }

                // If the read lexeme does not exist in the child node
                if (!node.Children.ContainsKey(tmplexeme.ToLower()))
                {
                    // If it allows itself to be a terminal node
                    // Return the cache read up to the previous time
                    if (node.IsTerminal)
                    {
                        var raw = memory.Slice(start, p - start).ToString();
                        end = p;
                        return new Token(TokenType.Keyword, lexeme, raw, lexeme);
                    }

                    // Keyword error not dictionary-ized
                    lexeme += " " + tmplexeme;
                    throw new NotSupportedException($"Unsupported keyword '{lexeme}' found at position {start}.");
                }

                // Record the coordinates of the lexeme and skip unnecessary characters that follow
                p = tmpPosition;
                memory.SkipWhiteSpacesAndComments(ref p);

                // concat lexeme
                lexeme += " " + tmplexeme;
                node = node.Children[tmplexeme.ToLower()];

                // If there are no child nodes, it is clear that it is a terminal node, so return the token
                if (node.Children.Count == 0)
                {
                    var raw = memory.Slice(start, p - start).ToString();
                    end = p;
                    return new Token(TokenType.Keyword, lexeme, raw, lexeme);
                }
            }
        }

        throw new InvalidOperationException($"Invalid lexeme at position {p}");
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
        if (!memory.Span[p].IsSymbols())
        {
            lexeme = string.Empty;
            return false;
        }
        p++;
        while (p < memory.Length && memory.Span[p].IsSymbols())
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

        if (memory.Span[p].IsWhiteSpace() || memory.Span[p].IsSymbols())
        {
            word = string.Empty;
            return false;
        }

        while (p < memory.Length && !memory.Span[p].IsWhiteSpace() && !memory.Span[p].IsSymbols())
        {
            p++;
        }

        word = memory.Slice(start, p - start).ToString();
        endPosition = p;
        return true;
    }
}
