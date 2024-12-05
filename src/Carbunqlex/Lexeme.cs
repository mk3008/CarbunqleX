﻿namespace Carbunqlex;

public readonly struct Lexeme
{
    public Lexeme(LexType type, string value, string clause)
    {
        Type = type;
        Value = value;
        Identifier = clause.ToLower();
    }

    public Lexeme(LexType lexType, string value) : this(lexType, value, value)
    {
    }

    public LexType Type { get; }
    public string Value { get; }
    public string Identifier { get; }
}
