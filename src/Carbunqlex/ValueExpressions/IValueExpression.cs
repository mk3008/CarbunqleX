﻿namespace Carbunqlex.ValueExpressions;

public interface IValueExpression : ISqlComponent
{
    string DefaultName { get; }
    bool MightHaveQueries { get; }
    IEnumerable<ColumnExpression> ExtractColumnExpressions();
}
