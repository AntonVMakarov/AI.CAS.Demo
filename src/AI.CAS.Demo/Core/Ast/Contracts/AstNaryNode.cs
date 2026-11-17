using System.Collections.Immutable;

namespace AI.CAS.Core.Ast;

/// <summary>
/// Представляет общую основу для n-арных узлов Ast.
/// Класс закрепляет минимум двух детей и сохраняет их исходный порядок. 
/// </summary>
public abstract class AstNaryNode : AstNode
{
    /// <summary>
    /// Создает n-арный узел с двумя или более детьми.
    /// </summary>
    /// <param name="children">
    /// Упорядоченная неизменяемая коллекция детей.
    /// </param>
    protected AstNaryNode(ImmutableArray<AstNode> children)
    {
        // Проверяем минимальную арность и отсутствие null-детей до сохранения коллекции.
        Children = ValidateChildrenAtLeast(children, 2, nameof(children));
    }


    /// <summary>
    /// Возвращает упорядоченную неизменяемую коллекцию детей n-арного узла.
    /// </summary>
    public override ImmutableArray<AstNode> Children { get; }


    /// <summary>
    /// Создает новый n-арный узел того же конкретного CLR-типа с указанными детьми.
    /// </summary>
    /// <param name="children">
    /// Новая коллекция детей, которая обязана содержать два или более узла.
    /// </param>
    /// <returns>
    /// Новый n-арный узел того же конкретного CLR-типа.
    /// </returns>
    public override AstNode WithChildren(ImmutableArray<AstNode> children)
    {
        // Проверяем, что общая пересборка не нарушает минимальную арность n-арного узла.
        ImmutableArray<AstNode> validatedChildren = ValidateChildrenAtLeast(children, 2, nameof(children));

        // Делегируем создание наследнику, чтобы сохранить конкретный CLR-тип.
        return Rebuild(validatedChildren);
    }


    /// <summary>
    /// Создает конкретный n-арный узел с новым упорядоченным набором детей.
    /// </summary>
    /// <param name="children">
    /// Проверенная неизменяемая коллекция из двух или более детей.
    /// </param>
    /// <returns>
    /// Новый n-арный узел конкретного наследника.
    /// </returns>
    protected abstract AstNaryNode Rebuild(ImmutableArray<AstNode> children);
}