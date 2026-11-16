using System.Collections.Immutable;

namespace AI.CAS.Core.Ast;

/// <summary>
/// Представляет общую основу для бинарных узлов Ast.
/// Класс закрепляет ровно двух детей в порядке <see cref="Left"/>, затем <see cref="Right"/>; этот порядок
/// обязателен для некоммутативных операций, отношений и логической импликации.
/// </summary>
public abstract class AstBinaryNode : AstNode
{
	/// <summary>
	/// Создает бинарный узел с левым и правым ребенком.
	/// </summary>
	/// <param name="left">
	/// Левый дочерний узел.
	/// </param>
	/// <param name="right">
	/// Правый дочерний узел.
	/// </param>
	protected AstBinaryNode(AstNode? left, AstNode? right)
	{
		// Проверяем левый дочерний узел до сохранения, чтобы структура не содержала пустую левую сторону.
		Left = ValidateChild(left, nameof(left));

		// Проверяем правый дочерний узел до сохранения, чтобы структура не содержала пустую правую сторону.
		Right = ValidateChild(right, nameof(right));
	}


	/// <summary>
	/// Возвращает левый дочерний узел бинарной конструкции.
	/// Для отношений это левая часть, для степени базовый класс предоставляет дополнительный семантический псевдоним.
	/// </summary>
	public AstNode Left { get; }


	/// <summary>
	/// Возвращает правый дочерний узел бинарной конструкции.
	/// Для отношений это правая часть, для степени базовый класс предоставляет дополнительный семантический псевдоним.
	/// </summary>
	public AstNode Right { get; }


	/// <summary>
	/// Возвращает неизменяемую коллекцию из двух детей в порядке <see cref="Left"/>, затем <see cref="Right"/>.
	/// </summary>
	public override ImmutableArray<AstNode> Children
	{
		get
		{
			// Создаем двухэлементную неизменяемую коллекцию, сохраняя точный упорядоченный бинарный контракт.
			return ImmutableArray.Create(Left, Right);
		}
	}


	/// <summary>
	/// Создает новый бинарный узел того же конкретного CLR-типа с двумя новыми детьми.
	/// </summary>
	/// <param name="children">
	/// Новая коллекция детей, которая обязана содержать ровно два узла в исходном порядке.
	/// </param>
	/// <returns>
	/// Новый бинарный узел того же конкретного CLR-типа.
	/// </returns>
	public override AstNode WithChildren(ImmutableArray<AstNode> children)
	{
		// Проверяем точную бинарную арность до создания нового семантического узла.
		ImmutableArray<AstNode> validatedChildren = ValidateChildren(children, 2, nameof(children));

		// Делегируем создание конкретному наследнику, чтобы тип операции или отношения был сохранен.
		return Rebuild(validatedChildren[0], validatedChildren[1]);
	}


	/// <summary>
	/// Создает конкретный бинарный узел с новыми левым и правым детьми.
	/// </summary>
	/// <param name="left">
	/// Новый левый ребенок.
	/// </param>
	/// <param name="right">
	/// Новый правый ребенок.
	/// </param>
	/// <returns>
	/// Новый бинарный узел конкретного наследника.
	/// </returns>
	protected abstract AstBinaryNode Rebuild(AstNode left, AstNode right);
}
