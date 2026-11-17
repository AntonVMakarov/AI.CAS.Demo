using System.Collections.Immutable;

namespace AI.CAS.Core.Ast;

/// <summary>
/// Представляет общую основу для унарных узлов Ast.
/// Класс закрепляет ровно одного ребенка и свойство <see cref="Argument"/> для математических функций и унарных
/// операторов, которым достаточно имени аргумента. Конкретный смысл остается в наследнике, например
/// <see cref="AstSinNode"/> или <see cref="AstUnaryMinusNode"/>.
/// </summary>
public abstract class AstUnaryNode : AstNode
{
    /// <summary>
    /// Создает унарный узел с одним аргументом.
    /// </summary>
    /// <param name="argument">
    /// Единственный дочерний узел унарной конструкции.
    /// </param>
    protected AstUnaryNode(AstNode? argument)
    {
        // Проверяем аргумент на null, потому что унарный узел без ребенка не имеет определенной структуры.
        Argument = ValidateChild(argument, nameof(argument));
    }


    /// <summary>
    /// Возвращает единственный аргумент унарного узла.
    /// Для функций вроде синуса это математический аргумент, а для унарного минуса это выражение под знаком минуса.
    /// </summary>
    public AstNode Argument { get; }


    /// <summary>
    /// Возвращает упорядоченную неизменяемую коллекцию из единственного аргумента.
    /// </summary>
    public override ImmutableArray<AstNode> Children
    {
        get
        {
            // Создаем одноэлементную неизменяемую коллекцию, которая отражает фиксированную арность унарного узла.
            return ImmutableArray.Create(Argument);
        }
    }


    /// <summary>
    /// Создает новый унарный узел того же конкретного CLR-типа с новым единственным аргументом.
    /// </summary>
    /// <param name="children">
    /// Новая коллекция детей, которая обязана содержать ровно один узел.
    /// </param>
    /// <returns>
    /// Новый унарный узел того же конкретного CLR-типа.
    /// </returns>
    public override AstNode WithChildren(ImmutableArray<AstNode> children)
    {
        // Проверяем, что общая пересборка передала ровно одного ребенка.
        ImmutableArray<AstNode> validatedChildren = ValidateChildren(children, 1, nameof(children));

        // Делегируем создание конкретному наследнику, чтобы конкретный CLR-тип был сохранен.
        return Rebuild(validatedChildren[0]);
    }


    /// <summary>
    /// Создает конкретный унарный узел с новым аргументом.
    /// Метод является единственной точкой, где базовый класс просит наследника сохранить свой математический тип.
    /// </summary>
    /// <param name="argument">
    /// Новый аргумент унарного узла.
    /// </param>
    /// <returns>
    /// Новый унарный узел конкретного наследника.
    /// </returns>
    protected abstract AstUnaryNode Rebuild(AstNode argument);
}