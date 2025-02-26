namespace Carbunqlex.Expressions;

public interface IWindowFrameBoundaryExpression : ISqlComponent
{
    string BoundaryKeyword { get; }
    bool MightHaveQueries { get; }
}
