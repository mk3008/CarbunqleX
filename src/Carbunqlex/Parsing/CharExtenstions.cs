//namespace Carbunqlex.Parsing;

//internal class QueryLexer
//{
//    private readonly ReadOnlyMemory<char> memory;
//    private int position;

//    public QueryLexer(string sql)
//    {
//        memory = sql.AsMemory();
//        position = 0;
//    }

//    public static Dictionary<string, List<string[]>> JoinKeywords { get; } = new Dictionary<string, List<string[]>>
//    {
//        { "inner join", new List<string[]> { new[] { "inner", "join" } } },
//        { "left join", new List<string[]>
//            {
//                new[] { "left", "outer", "join" },
//                new[] { "left", "join" }
//            }
//        },
//        { "right join", new List<string[]>
//            {
//                new[] { "right", "outer", "join" },
//                new[] { "right", "join" }
//            }
//        },
//        { "lateral", new List<string[]> { new[] { "lateral" } } },
//        { "left join lateral", new List<string[]>
//            {
//                new[] { "left", "join", "lateral" },
//                new[] { "left", "outer", "join", "lateral" }
//            }
//        },
//        { "right join lateral", new List<string[]>
//            {
//                new[] { "right", "join", "lateral" },
//                new[] { "right", "outer", "join", "lateral" }
//            }
//        },
//        { "cross join lateral", new List<string[]>
//            {
//                new[] { "cross", "join", "lateral" },
//                new[] { ",", "lateral" }
//            }
//        },
//        { "cross join", new List<string[]>
//            {
//                new[] { "cross", "join" },
//                new[] { "," }
//            }
//        },
//    };

//    public static Dictionary<string, List<string[]>> UnionKeywords { get; } = new Dictionary<string, List<string[]>>
//    {
//        { "union all", new List<string[]> { new[] { "union", "all" } } },
//        { "union", new List<string[]> { new[] { "union"} } },
//        { "intersect", new List<string[]> { new[] { "intersect" } } },
//        { "except", new List<string[]> { new[] { "except" } } },
//    };

//    public static Dictionary<string, List<string[]>> SelectKeywords { get; } = new Dictionary<string, List<string[]>>
//    {
//        { "select", new List<string[]>
//            {
//                new[] { "select", "distinct", "on" } ,
//                new[] { "select", "distinct" } ,
//                new[] { "select", } ,
//            }
//        }
//    };

//    //public void SkipWhiteSpaces()
//    //{
//    //    memory.SkipWhiteSpaces(ref position);
//    //}

//    public bool TryParseLineComment(int start, out int endPosition, out IEnumerable<Token> tokens)
//    {
//        endPosition = start;
//        tokens = Enumerable.Empty<Token>();

//        if (memory.StartWith("--", start, out var p))
//        {
//            return false;
//        }

//        while (p < memory.Length && !memory.Span[p].IsLineEnd())
//        {
//            p++;
//        }
//        endPosition = p;

//        if (endPosition == memory.Length)
//        {
//            tokens =
//            [
//                new Token(TokenType.Comment,  memory.Slice(start, p - start).ToString()),
//            ];
//        }
//        else
//        {
//            tokens =
//            [
//                new Token(TokenType.Comment,  memory.Slice(start, p - start - 1).ToString()),
//            ];
//            memory.SkipWhiteSpaces(ref p);
//        }

//        return true;
//    }

//    public bool TryParseBlockComment(int start, out int endPosition, out IEnumerable<Token> tokens)
//    {
//        endPosition = start;
//        tokens = Enumerable.Empty<Token>();

//        if (!memory.StartWith("/*", start, out var p))
//        {
//            return false;
//        }

//        if (!memory.ReadUntil(p, "*/", out p))
//        {
//            return false;
//        }

//        tokens = new List<Token>
//        {
//            new Token(TokenType.Comment, memory.Slice(start, p - start).ToString()),
//        };

//        memory.SkipWhiteSpaces(ref p);
//        endPosition = p;
//        return true;
//    }

//    public bool TryParseSelect(int start, out int endPosition, out Token token)
//    {
//        foreach (var keywordPair in SelectKeywords)
//        {
//            foreach (var keywordSet in keywordPair.Value)
//            {
//                if (TryParseKeywords(start, out endPosition, keywordPair.Key, keywordSet, out token))
//                {
//                    return true;
//                }
//            }
//        }

//        endPosition = start;
//        token = Token.Empty;
//        return false;
//    }

//    public bool TryParseFrom(int start, out int endPosition, out Token token)
//    {
//        return TryParseKeyword(start, out endPosition, "from", out token);
//    }

//    public bool TryParseWhere(int start, out int endPosition, out Token token)
//    {
//        return TryParseKeyword(start, out endPosition, "where", out token);
//    }

//    public bool TryParseGroupBy(int start, out int endPosition, out Token token)
//    {
//        return TryParseKeywords(start, out endPosition, "group by", ["group", "by"], out token);
//    }

//    public bool TryParseHaving(int start, out int endPosition, out Token token)
//    {
//        return TryParseKeyword(start, out endPosition, "having", out token);
//    }

//    public bool TryParseOrderBy(int start, out int endPosition, out Token token)
//    {
//        return TryParseKeywords(start, out endPosition, "order by", ["order", "by"], out token);
//    }

//    public bool TryParseLimit(int start, out int endPosition, out Token token)
//    {
//        return TryParseKeyword(start, out endPosition, "limit", out token);
//    }

//    public bool TryParseOffset(int start, out int endPosition, out Token token)
//    {
//        return TryParseKeyword(start, out endPosition, "offset", out token);
//    }

//    public bool TryParseFetch(int start, out int endPosition, out Token token)
//    {
//        return TryParseKeywords(start, out endPosition, "fetch", ["fetch", "first", "rows", "only"], out token);
//    }

//    public bool TryParseFor(int start, out int endPosition, out Token token)
//    {
//        return TryParseKeywords(start, out endPosition, "for", ["for", "update"], out token);
//    }

//    public bool TryParseWith(int start, out int endPosition, out Token token)
//    {
//        return TryParseKeyword(start, out endPosition, "with", out token);
//    }

//    public bool TryParseAs(int start, out int endPosition, out Token token)
//    {
//        return TryParseKeyword(start, out endPosition, "as", out token);
//    }

//    public bool TryParseUnion(int start, out int endPosition, out Token token)
//    {
//        foreach (var keywordPair in UnionKeywords)
//        {
//            foreach (var keywordSet in keywordPair.Value)
//            {
//                if (TryParseKeywords(start, out endPosition, keywordPair.Key, keywordSet, out token))
//                {
//                    return true;
//                }
//            }
//        }

//        endPosition = start;
//        token = Token.Empty;
//        return false;
//    }

//    public bool TryParseJoin(int start, out int endPosition, out Token token)
//    {
//        foreach (var keywordPair in JoinKeywords)
//        {
//            foreach (var keywordSet in keywordPair.Value)
//            {
//                if (TryParseKeywords(start, out endPosition, keywordPair.Key, keywordSet, out token))
//                {
//                    return true;
//                }
//            }
//        }

//        endPosition = start;
//        token = Token.Empty;
//        return false;
//    }

//    private bool TryParseKeyword(int start, out int endPosition, string keyword, out Token token)
//    {
//        endPosition = start;
//        token = Token.Empty;

//        if (!memory.StartWith(keyword, start, out var p, ignoreCase: true))
//        {
//            return false;
//        }

//        var tokenList = new List<Token>
//        {
//            new Token(TokenType.Keyword, keyword)
//        };

//        memory.SkipWhiteSpaces(ref p);
//        ParseComments(ref p, tokenList);

//        endPosition = p;
//        token = new Token(TokenType.Keyword, string.Join(" ", tokenList.Select(x => x.Value)), keyword);
//        return true;
//    }

//    private bool TryParseKeywords(int start, out int endPosition, string identifier, IEnumerable<string> keywords, out Token token)
//    {
//        endPosition = start;
//        token = Token.Empty;

//        var p = start;
//        var tokenList = new List<Token>();
//        foreach (var keyword in keywords)
//        {
//            if (memory.StartWith(keyword, start, out p, ignoreCase: true))
//            {
//                tokenList.Add(new Token(TokenType.Keyword, keyword));

//                memory.SkipWhiteSpaces(ref p);
//                ParseComments(ref p, tokenList);
//            }
//            else
//            {
//                return false;
//            }
//        }

//        endPosition = p;
//        token = new Token(TokenType.Keyword, string.Join(" ", tokenList.Select(x => x.Value)), identifier);
//        return true;
//    }

//    private void ParseComments(ref int position, List<Token> tokenList)
//    {
//        while (true)
//        {
//            if (TryParseLineComment(position, out position, out var lineCommentTokens))
//            {
//                tokenList.AddRange(lineCommentTokens);
//                continue;
//            }
//            if (TryParseBlockComment(position, out position, out var blockCommentTokens))
//            {
//                tokenList.AddRange(blockCommentTokens);
//                continue;
//            }
//            break;
//        }
//    }
//}

namespace Carbunqlex.Parsing
{
    internal static class CharExtenstions
    {
        /// <summary>
        /// Defines a set of characters considered as symbols that terminate an identifier.
        /// </summary>
        private static readonly HashSet<char> Symbols = new HashSet<char>
    {
        '+', '-', '*', '/', '%', // Arithmetic operators
        '(', ')', '[', ']', '{', '}', // Brackets and braces
        '~', '@', '#', '$', '^', '&', // Special symbols
        '!', '?', ':', ';', ',', '.', '<', '>', '=', '|', '\\', // Other symbols
        '`', '"', '\'' // Quotation marks
    };

        private static readonly HashSet<char> WhiteSpaces = new HashSet<char>
    {
        ' ', '\t', '\r', '\n',
    };

        private static readonly Dictionary<char, char> ValueEscapePairs = new Dictionary<char, char>
    {
        { '"', '"' }, // ダブルクォート
        { '[', ']' }, // 角括弧
        { '`', '`' }  // バッククォート
    };

        private static readonly Dictionary<char, char> LineEnds = new Dictionary<char, char>
    {
        { '\r', '\n' },
        { '\n', '\r' }
    };

        public static bool TryGetDbmsValueEscapeChar(this char c, out char closeChar)
        {
            return ValueEscapePairs.TryGetValue(c, out closeChar);
        }

        public static bool IsWhiteSpace(this char c)
        {
            return WhiteSpaces.Contains(c);
        }

        public static bool IsSymbols(this char c)
        {
            return Symbols.Contains(c);
        }

        public static bool IsLineEnd(this char c)
        {
            return LineEnds.ContainsKey(c);
        }
    }
}
