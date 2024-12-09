namespace Carbunqlex.ValueExpressions;

public interface IValueExpression : ISqlComponent
{
    string DefaultName { get; }
    bool MightHaveCommonTableClauses { get; }
}
