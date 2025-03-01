namespace Carbunqlex.Lexing
{
    internal static class CharExtenstions
    {
        private static readonly char[] CharacterSymbols =
        [
                '(', ')', '[', ']', '{', '}', // Brackets and braces
                '`', '"', '\'', // Quotation marks
                '*', // Select all
                ';', // Statement terminator
        ];

        private static readonly char[] MultipleSymbols =
        [
                '+', '-', '*', '/', '%', // Arithmetic operators
                '~', '@', '#', '$', '^', '&', // Special symbols
                '!', '?', ':', ',', '.', '<', '>', '=', '|', '\\', // Other symbols
        ];

        private static readonly char[] WhiteSpaces =
        [
                ' ', '\t', '\r', '\n',
        ];

        private static readonly char[] LineEnds =
        [
                '\r', '\n'
        ];

        public static bool IsWhiteSpace(this char c)
        {
            foreach (var ws in WhiteSpaces)
            {
                if (ws == c)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool IsSingleSymbol(this char c)
        {
            foreach (var symbol in CharacterSymbols)
            {
                if (symbol == c)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool IsMultipleSymbol(this char c)
        {
            foreach (var symbol in MultipleSymbols)
            {
                if (symbol == c)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool IsLineEnd(this char c)
        {
            foreach (var le in LineEnds)
            {
                if (le == c)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
