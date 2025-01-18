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
            { '"', '"' }, // Double quote
            { '[', ']' }, // Square brackets
            { '`', '`' }  // Backquote
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
