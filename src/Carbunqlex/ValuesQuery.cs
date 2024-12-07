using System.Globalization;
using System.Text;

namespace Carbunqlex;

public class ValuesQuery : IQuery
{
    public readonly List<ValuesRow> Rows = new();
    private int? columnCount;

    public void AddRow(IEnumerable<ValuesColumn> columns)
    {
        var row = columns.ToList();
        if (columnCount == null)
        {
            columnCount = row.Count;
        }
        else if (row.Count != columnCount)
        {
            throw new ArgumentException($"All rows must have the same number of columns. Expected {columnCount}, but got {row.Count}.");
        }

        Rows.Add(new ValuesRow(row));
    }

    public string ToSql()
    {
        var sb = new StringBuilder("values ");
        for (int i = 0; i < Rows.Count; i++)
        {
            sb.Append(Rows[i].ToSql());
            if (i < Rows.Count - 1)
            {
                sb.Append(", ");
            }
        }
        return sb.ToString();
    }

    /// <summary>
    /// The number of lexemes required to represent this query.
    /// </summary>
    internal int Capacity => Rows.Sum(row => row.Capacity) + (Rows.Count - 1) + 1;

    public IEnumerable<Lexeme> GetLexemes()
    {
        var lexemes = new List<Lexeme>(Capacity) { new Lexeme(LexType.Keyword, "VALUES") };

        for (int i = 0; i < Rows.Count; i++)
        {
            lexemes.AddRange(Rows[i].GetLexemes());
            if (i < Rows.Count - 1)
            {
                lexemes.Add(new Lexeme(LexType.Comma, ","));
            }
        }
        return lexemes;
    }
}

public class ValuesRow
{
    public List<ValuesColumn> Columns { get; } = new();

    public ValuesRow()
    {
    }
    public ValuesRow(IEnumerable<ValuesColumn> columns)
    {
        Columns.AddRange(columns);
    }

    public string ToSql()
    {
        var sb = new StringBuilder("(");
        for (int i = 0; i < Columns.Count; i++)
        {
            sb.Append(Columns[i].Value);
            if (i < Columns.Count - 1)
            {
                sb.Append(", ");
            }
        }
        sb.Append(")");
        return sb.ToString();
    }

    /// <summary>
    /// The number of lexemes required to represent this row.
    /// </summary>
    internal int Capacity => Columns.Count * 2 - 1 + 2;

    public IEnumerable<Lexeme> GetLexemes()
    {
        var lexemes = new List<Lexeme>(Capacity) { new Lexeme(LexType.OpenParen, "(") };

        for (int i = 0; i < Columns.Count; i++)
        {
            lexemes.Add(new Lexeme(LexType.Value, Columns[i].Value));

            if (i < Columns.Count - 1)
            {
                lexemes.Add(new Lexeme(LexType.Comma, ","));
            }
        }

        lexemes.Add(new Lexeme(LexType.CloseParen, ")"));
        return lexemes;
    }
}

public readonly struct ValuesColumn
{
    public string Value { get; }
    public ValuesColumn(string value)
    {
        Value = value;
    }
    public static ValuesColumn Create(object? value)
    {
        string columnValue;
        if (value is DateTime dateTimeValue)
        {
            columnValue = "'" + dateTimeValue.ToString("yyyy-MM-dd HH:mm:ss") + "'";
        }
        else if (value is double doubleValue)
        {
            columnValue = doubleValue.ToString("G", CultureInfo.InvariantCulture);
        }
        else if (value is string stringValue)
        {
            columnValue = "'" + stringValue.Replace("'", "''") + "'";
        }
        else
        {
            columnValue = value?.ToString() ?? "null";
        }
        return new ValuesColumn(columnValue);
    }
}
