using Carbunqlex.Expressions;

namespace Carbunqlex;

public class ColumnEditor(ISelectQuery query, IValueExpression expression, SelectExpression? selectExpression = null)
{
    public ISelectQuery Query { get; } = query;

    public IValueExpression Value { get; internal set; } = expression;

    private SelectExpression? _selectExpression = selectExpression;
    public SelectExpression? SelectExpression => _selectExpression ??= Query.GetSelectExpressions().Where(e => e.Value == Value).FirstOrDefault();

    private SelectEditor? _selectModifier;
    public SelectEditor SelectModifier => _selectModifier ??= new(this);

    private FromEditor? _fromModifier;
    public FromEditor FromModifier => _fromModifier ??= new(Query, new Dictionary<string, IValueExpression>() { { Value.DefaultName.ToLowerInvariant(), Value } });

    public override string ToString()
    {
        try
        {
            return $"{Value.ToSqlWithoutCte()} : {Query.ToSqlWithoutCte()}";
        }
        catch (Exception ex)
        {
            return ex.ToString();
        }
    }

    //internal void AddCondition(string operatorSymbol, IValueExpression rightValue)
    //{
    //    var condition = new BinaryExpression(operatorSymbol, Value, rightValue);
    //    AddCondition(condition);
    //}

    public ParameterExpression AddParameter(string name, object value)
    {
        return Query.AddParameter(name, value);
    }

    public SelectEditor AddColumn(SelectExpression expr)
    {
        Query.AddColumn(expr);
        return new SelectEditor(new ColumnEditor(Query, expr.Value, expr));
    }

    public SelectEditor AddColumn(IValueExpression value)
    {
        return AddColumn(value, value.DefaultName);
    }

    public SelectEditor AddColumn(IValueExpression value, string alias)
    {
        var expr = new SelectExpression(value, alias);
        Query.AddColumn(expr);
        return new SelectEditor(new ColumnEditor(Query, expr.Value, expr));
    }

    internal static IValueExpression ToValueExpression(object value)
    {
        return value is IValueExpression expr
            ? expr
            : ValueBuilder.Constant(value);
    }
}
