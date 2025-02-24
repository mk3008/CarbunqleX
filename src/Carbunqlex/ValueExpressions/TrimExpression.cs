namespace Carbunqlex.ValueExpressions;

public class TrimExpression : IValueExpression
{
    public IValueExpression OriginalText { get; set; }
    public IValueExpression Characters { get; set; }
    public string TrimType { get; set; }

    public TrimExpression(IValueExpression originalText)
    {
        OriginalText = originalText;
        Characters = new LiteralExpression("' '");
        TrimType = "both";
    }

    public TrimExpression(string trimType, IValueExpression originalText)
    {
        TrimType = trimType;
        Characters = new LiteralExpression("' '");
        OriginalText = originalText;
    }

    /// <summary>
    /// e.g. BOTH characters FROM original
    /// </summary>
    /// <param name="trimType"></param>
    /// <param name="characters"></param>
    /// <param name="original"></param>
    /// <param name=""></param>
    public TrimExpression(string trimType, IValueExpression characters, IValueExpression original)
    {
        TrimType = trimType;
        Characters = characters;
        OriginalText = original;
    }

    private bool IsDefaultTrimType()
    {
        return TrimType.Equals("both", StringComparison.InvariantCultureIgnoreCase);
    }

    private bool IsDefaultCharacters()
    {
        return Characters.ToSqlWithoutCte() == "' '";
    }

    public string ToSqlWithoutCte()
    {
        if (IsDefaultTrimType())
        {
            if (IsDefaultCharacters())
            {
                return $"trim({OriginalText.ToSqlWithoutCte()})";
            }
            return $"trim({Characters.ToSqlWithoutCte()} from {OriginalText.ToSqlWithoutCte()})";
        }
        return $"trim({TrimType} {Characters.ToSqlWithoutCte()} from {OriginalText.ToSqlWithoutCte()})";
    }

    public IEnumerable<Token> GenerateTokensWithoutCte()
    {
        if (IsDefaultTrimType())
        {
            if (IsDefaultCharacters())
            {
                var tokens = new List<Token>
                {
                    new Token(TokenType.Command, "trim"),
                    new Token(TokenType.OpenParen, "(")
                };
                foreach (var lexeme in OriginalText.GenerateTokensWithoutCte())
                {
                    tokens.Add(lexeme);
                }
                tokens.Add(new Token(TokenType.CloseParen, ")"));
                return tokens;
            }
            else
            {
                var tokens = new List<Token>
                {
                    new Token(TokenType.Command, "trim"),
                    new Token(TokenType.OpenParen, "("),
                    new Token(TokenType.Literal, Characters.ToSqlWithoutCte()),
                    new Token(TokenType.Command, "from")
                };
                foreach (var lexeme in OriginalText.GenerateTokensWithoutCte())
                {
                    tokens.Add(lexeme);
                }
                tokens.Add(new Token(TokenType.CloseParen, ")"));
                return tokens;
            }
        }
        else
        {
            var tokens = new List<Token>
            {
                new Token(TokenType.Command, "trim"),
                new Token(TokenType.OpenParen, "("),
                new Token(TokenType.Identifier, TrimType)
            };
            foreach (var lexeme in Characters.GenerateTokensWithoutCte())
            {
                tokens.Add(lexeme);
            }
            tokens.Add(new Token(TokenType.Command, "from"));
            foreach (var lexeme in OriginalText.GenerateTokensWithoutCte())
            {
                tokens.Add(lexeme);
            }
            tokens.Add(new Token(TokenType.CloseParen, ")"));
            return tokens;
        }
    }

    public string DefaultName => string.Empty;

    public bool MightHaveQueries => false;

    public IEnumerable<ColumnExpression> ExtractColumnExpressions()
    {
        yield break;
    }

    public IEnumerable<ISelectQuery> GetQueries()
    {
        yield break;
    }
}

public class PostgresTrimExpression : IValueExpression
{
    /// <summary>
    /// e.g. 'leading from', 'trailing from'
    /// </summary>
    public string TrimType { get; set; }

    public ArgumentExpression Argument { get; set; }

    public PostgresTrimExpression(ArgumentExpression argument)
    {
        TrimType = "both from";
        Argument = argument;
    }

    public PostgresTrimExpression(string trimType, ArgumentExpression argument)
    {
        TrimType = trimType;
        Argument = argument;
    }

    private bool IsDefaultTrimType()
    {
        return TrimType.Equals("both from", StringComparison.InvariantCultureIgnoreCase);
    }

    public string ToSqlWithoutCte()
    {
        if (IsDefaultTrimType())
        {
            return $"trim({Argument.ToSqlWithoutCte()})";
        }
        return $"trim({TrimType} {Argument.ToSqlWithoutCte()})";
    }

    public IEnumerable<Token> GenerateTokensWithoutCte()
    {
        if (IsDefaultTrimType())
        {
            var tokens = new List<Token>
            {
                new Token(TokenType.Command, "trim"),
                new Token(TokenType.OpenParen, "(")
            };
            foreach (var lexeme in Argument.GenerateTokensWithoutCte())
            {
                tokens.Add(lexeme);
            }
            tokens.Add(new Token(TokenType.CloseParen, ")"));
            return tokens;
        }
        else
        {
            var tokens = new List<Token>
            {
                new Token(TokenType.Command, "trim"),
                new Token(TokenType.OpenParen, "(")
            };
            tokens.AddRange(TrimType.Split(' ').Select(s => new Token(TokenType.Command, s)));
            foreach (var lexeme in Argument.GenerateTokensWithoutCte())
            {
                tokens.Add(lexeme);
            }
            tokens.Add(new Token(TokenType.CloseParen, ")"));
            return tokens;
        }
    }

    public string DefaultName => string.Empty;

    public bool MightHaveQueries => false;

    public IEnumerable<ColumnExpression> ExtractColumnExpressions()
    {
        yield break;
    }

    public IEnumerable<ISelectQuery> GetQueries()
    {
        yield break;
    }
}
