namespace AI.CAS.Core.Ast;

/// <summary>
/// Представляет квадратный корень как конкретный унарный узел Ast.
/// Узел хранит известную встроенную семантику <c>sqrt(argument)</c> отдельным CLR-типом и не использует
/// <see cref="AstNamedFunctionNode"/>, который остается резервным представлением пользовательских функций.
/// </summary>
public sealed class AstSqrtNode : AstUnaryNode
{
    /// <summary>
    /// Создает квадратный корень с заданным подкоренным выражением.
    /// </summary>
    /// <param name="argument">
    /// Подкоренное выражение.
    /// </param>
    public AstSqrtNode(AstNode? argument) : base(argument)
    {
        // Базовый унарный класс проверяет null, фиксирует единственного ребенка и сохраняет порядок Children.
    }


    /// <summary>
    /// Создает новый квадратный корень с другим подкоренным выражением.
    /// </summary>
    /// <param name="argument">
    /// Новое подкоренное выражение.
    /// </param>
    /// <returns>
    /// Новый <see cref="AstSqrtNode"/>.
    /// </returns>
    protected override AstUnaryNode Rebuild(AstNode argument)
    {
        // Перестраиваем тот же математический тип без вычисления значения корня.
        return new AstSqrtNode(argument);
    }
}