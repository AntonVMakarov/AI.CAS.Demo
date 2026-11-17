namespace AI.CAS.Core.Ast;


/// <summary>
/// Представляет бинарную степень.
/// Узел публикует семантические имена <see cref="Base"/> и <see cref="Exponent"/>, потому что основание
/// и показатель имеют разные роли и не могут быть переставлены.
/// </summary>
public sealed class AstPowNode : AstBinaryNode
{
	/// <summary>
	/// Создает бинарную степень.
	/// </summary>
	/// <param name="baseNode">
	/// Основание степени.
	/// </param>
	/// <param name="exponent">
	/// Показатель степени.
	/// </param>
	public AstPowNode(AstNode? baseNode, AstNode? exponent) : base(baseNode, exponent)
	{
		// Базовый класс уже проверил основание и показатель.
	}


	/// <summary>
	/// Возвращает основание степени.
	/// </summary>
	public AstNode Base
	{
		get
		{
			// Основание хранится как левый ребенок бинарного узла.
			return Left;
		}
	}


	/// <summary>
	/// Возвращает показатель степени.
	/// </summary>
	public AstNode Exponent
	{
		get
		{
			// Показатель хранится как правый ребенок бинарного узла.
			return Right;
		}
	}


	/// <summary>
	/// Создает новую степень с заданными основанием и показателем.
	/// </summary>
	/// <param name="left">
	/// Новое основание.
	/// </param>
	/// <param name="right">
	/// Новый показатель.
	/// </param>
	/// <returns>
	/// Новый <see cref="AstPowNode"/>.
	/// </returns>
	protected override AstBinaryNode Rebuild(AstNode left, AstNode right)
	{
		// Возвращаем новый узел степени.
		return new AstPowNode(left, right);
	}
}