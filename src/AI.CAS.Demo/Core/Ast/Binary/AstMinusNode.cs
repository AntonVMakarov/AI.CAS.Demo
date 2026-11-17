namespace AI.CAS.Core.Ast;

/// <summary>
/// Представляет бинарное вычитание.
/// Узел является бинарным, потому что <c>a-b-c</c> требует явной группировки и не является n-арной операцией.
/// </summary>
public sealed class AstMinusNode : AstBinaryNode
{
	/// <summary>
	/// Создает бинарное вычитание.
	/// </summary>
	/// <param name="left">
	/// Уменьшаемое выражение.
	/// </param>
	/// <param name="right">
	/// Вычитаемое выражение.
	/// </param>
	public AstMinusNode(AstNode? left, AstNode? right) : base(left, right)
	{
		// Базовый класс уже проверил обе стороны вычитания.
	}


	/// <summary>
	/// Создает новое бинарное вычитание с заданными детьми.
	/// </summary>
	/// <param name="left">
	/// Новое уменьшаемое.
	/// </param>
	/// <param name="right">
	/// Новое вычитаемое.
	/// </param>
	/// <returns>
	/// Новый <see cref="AstMinusNode"/>.
	/// </returns>
	protected override AstBinaryNode Rebuild(AstNode left, AstNode right)
	{
		// Возвращаем новый узел вычитания без преобразования в сложение с унарным минусом.
		return new AstMinusNode(left, right);
	}
}
