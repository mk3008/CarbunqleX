namespace Carbunqlex.Parsing
{
    internal static class CharExtenstions
    {
        /// <summary>
        /// Defines a set of characters considered as symbols that terminate an identifier.
        /// </summary>
        private static readonly HashSet<char> CharacterSymbols = new HashSet<char>
        {
            '(', ')', '[', ']', '{', '}', // Brackets and braces
            '`', '"', '\'', // Quotation marks
            '*', // Select all
            ';', // Statement terminator
        };

        /// <summary>
        /// Defines a set of characters considered as symbols that terminate an identifier.
        /// </summary>
        private static readonly HashSet<char> MultipleSymbols = new HashSet<char>
        {
            '+', '-', '*', '/', '%', // Arithmetic operators
            '~', '@', '#', '$', '^', '&', // Special symbols
            '!', '?', ':', ',', '.', '<', '>', '=', '|', '\\', // Other symbols
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

        public static bool IsSingleSymbol(this char c)
        {
            return CharacterSymbols.Contains(c);
        }

        public static bool IsMultipleSymbol(this char c)
        {
            return MultipleSymbols.Contains(c);
        }

        public static bool IsLineEnd(this char c)
        {
            return LineEnds.ContainsKey(c);
        }
    }
}
