/// <summary>
/// Представляет символьную константу <c>pi</c> в виде Ast.
/// Узел не хранит десятичное приближение числа пи и не выполняет вычислений. Будущие численные или аналитические
/// подсистемы должны распознавать его по конкретному типу <see cref="AstPiNode"/>.
/// </summary>
public sealed class AstPiNode : AstSymbolicConstantNode
{
    /// <summary>
    /// Создает новый узел <c>pi</c> без детей.
    /// </summary>
    public AstPiNode()
    {
    }


    /// <summary>
    /// Создает новый узел <c>pi</c> для общего функционального перестроения.
    /// </summary>
    /// <returns>
    /// Новый узел <see cref="AstPiNode"/>.
    /// </returns>
    protected override AstSymbolicConstantNode Recreate()
    {
        // Возвращаем новый экземпляр того же конкретного типа, чтобы не смешивать пи с другими атомарными константами.
        return new AstPiNode();
    }
}