using System.Globalization;
using System.Numerics;
using System.Text;

namespace AI.CAS.Core.Ast;

/// <summary>
/// Представляет точное целое число Ast на основе <see cref="BigInteger"/>.
/// Целые числа являются точными значениями и не имеют отдельного знаменателя, точности или исходной записи. Рациональная
/// фабрика обязана возвращать этот тип при знаменателе один, чтобы у точного целого числа не появлялось второе
/// каноническое представление в виде рациональной дроби <c>n/1</c>.
/// </summary>
public sealed class AstInteger : AstNumber
{
    /// <summary>
    /// Создает точное целое числовое значение Ast.
    /// <see cref="BigInteger"/> используется как неизменяемое фундаментальное хранилище целых чисел произвольного размера.
    /// </summary>
    /// <param name="integer">
    /// Точное целое значение, которое должно храниться без потери точности.
    /// </param>
    public AstInteger(BigInteger integer)
    {
        // Сохраняем точное целое значение без нормализации, потому что BigInteger уже имеет каноническое представление.
        Integer = integer;
    }


    /// <summary>
    /// Возвращает точное целое значение.
    /// Свойство намеренно называется <c>Integer</c>, а не универсально, чтобы числовые данные оставались типизированными.
    /// </summary>
    public BigInteger Integer { get; }


    /// <summary>
    /// Сравнивает собственное целое значение с другим <see cref="AstInteger"/>.
    /// </summary>
    /// <param name="other">
    /// Другое число того же конкретного CLR-типа.
    /// </param>
    /// <returns>
    /// <see langword="true"/>, если оба целых значения совпадают.
    /// </returns>
    protected override bool PayloadEquals(AstNumber other)
    {
        // Приводим к AstInteger после проверки конкретного типа в базовом классе.
        AstInteger otherInteger = (AstInteger)other;

        // Сравниваем точные BigInteger-значения без строковых представлений и округления.
        return Integer == otherInteger.Integer;
    }


    /// <summary>
    /// Добавляет точное целое значение в накопитель хеша.
    /// </summary>
    /// <param name="hashCode">
    /// Накопитель хеша числового значения.
    /// </param>
    protected override void AddPayloadHashCode(ref HashCode hashCode)
    {
        // Добавляем BigInteger напрямую, чтобы хеш был согласован с точным числовым равенством.
        hashCode.Add(Integer);
    }


    /// <summary>
    /// Печатает целое число цифрами со знаком минус для отрицательных значений: <c>42</c>, <c>-3</c>.
    /// </summary>
    /// <param name="builder">
    /// Буфер формы.
    /// </param>
    internal override void AppendDebugPayload(StringBuilder builder)
    {
        builder.Append(Integer.ToString(CultureInfo.InvariantCulture));
    }
}
