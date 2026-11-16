namespace AI.CAS.Core.Ast;

/// <summary>
/// Представляет бинарное деление.
/// Узел публикует семантические имена <see cref="Numerator"/> и <see cref="Denominator"/>, потому что порядок
/// числителя и знаменателя имеет самостоятельный математический смысл.
/// </summary>
public sealed class AstDivNode : AstBinaryNode
{
	/// <summary>
	/// Создает бинарное деление.
	/// </summary>
	/// <param name="numerator">
	/// Числитель дробной формы.
	/// </param>
	/// <param name="denominator">
	/// Знаменатель дробной формы.
	/// </param>
	public AstDivNode(AstNode? numerator, AstNode? denominator) : base(numerator, denominator)
	{
		// Базовый класс уже проверил числитель и знаменатель как двух детей.
	}


	/// <summary>
	/// Возвращает числитель деления.
	/// </summary>
	public AstNode Numerator
	{
		get
		{
			// Числитель хранится как левый ребенок бинарного узла.
			return Left;
		}
	}


	/// <summary>
	/// Возвращает знаменатель деления.
	/// </summary>
	public AstNode Denominator
	{
		get
		{
			// Знаменатель хранится как правый ребенок бинарного узла.
			return Right;
		}
	}


	/// <summary>
	/// Создает новое деление с заданными числителем и знаменателем.
	/// </summary>
	/// <param name="left">
	/// Новый числитель.
	/// </param>
	/// <param name="right">
	/// Новый знаменатель.
	/// </param>
	/// <returns>
	/// Новый <see cref="AstDivNode"/>.
	/// </returns>
	protected override AstBinaryNode Rebuild(AstNode left, AstNode right)
	{
		// Возвращаем новый узел деления без сокращения дроби и без проверки нуля в знаменателе.
		return new AstDivNode(left, right);
	}
}
