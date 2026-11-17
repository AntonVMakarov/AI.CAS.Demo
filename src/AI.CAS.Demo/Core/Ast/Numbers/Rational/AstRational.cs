using System.Globalization;
using System.Numerics;
using System.Text;

namespace AI.CAS.Core.Ast;

/// <summary>
/// Представляет каноническое точное рациональное число Ast.
/// Этот объект-значение хранит только рациональные числовые данные: числитель и положительный знаменатель,
/// которые уже сокращены по НОД и не образуют целое число. Конструктор намеренно доступен только внутри сборки, потому что
/// внешний код должен создавать рациональные значения только через <see cref="AstNumberCreate.Rational(BigInteger, BigInteger)"/>.
/// Такая граница предотвращает появление нескольких точных представлений одного числа, например <c>2/4</c>,
/// <c>1/2</c> и <c>AstRational(2, 4)</c> как разных объектов-значений.
/// </summary>
public sealed class AstRational : AstNumber
{
    /// <summary>
    /// Создает уже каноническое рациональное значение.
    /// Метод не выполняет нормализацию входа: нормализация принадлежит <see cref="AstNumberCreate"/>, а этот
    /// конструктор только защищает внутренний инвариант на случай будущего расширения кода внутри сборки.
    /// </summary>
    /// <param name="numerator">
    /// Канонический ненулевой числитель. Отрицательный знак хранится именно здесь, потому что знаменатель всегда положителен.
    /// </param>
    /// <param name="denominator">
    /// Канонический положительный знаменатель больше единицы.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Выбрасывается, если компоненты не образуют каноническую собственно рациональную форму.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Выбрасывается, если знаменатель не является положительным.
    /// </exception>
    internal AstRational(BigInteger numerator, BigInteger denominator)
    {
        // Проверяем нулевой числитель, потому что точный ноль обязан быть AstInteger(0), а не отдельной рациональной формой.
        if (numerator.IsZero)
        {
            // Сообщаем о нарушении канонической границы, чтобы внутренний вызывающий код не создал второй точный ноль.
            throw new ArgumentException("Каноническая рациональная дробь AST не может иметь нулевой числитель.", nameof(numerator));
        }

        // Проверяем знак знаменателя до проверки НОД, потому что отрицательный знаменатель является отдельным нарушением канонической формы знака.
        if (denominator.Sign <= 0)
        {
            // Сообщаем, что знаменатель должен быть строго положительным после нормализации знака.
            throw new ArgumentOutOfRangeException(nameof(denominator), "Каноническая рациональная дробь AST должна иметь положительный знаменатель.");
        }

        // Проверяем равенство знаменателя единице, потому что точное целое число обязано храниться только как AstInteger.
        if (denominator.IsOne)
        {
            // Сообщаем о попытке создать второе каноническое представление целого числа.
            throw new ArgumentException("Рациональная дробь со знаменателем 1 должна быть представлена как AstInteger.", nameof(denominator));
        }

        // Вычисляем точный НОД абсолютного числителя и положительного знаменателя для проверки сокращенности числовых данных.
        BigInteger greatestCommonDivisor = BigInteger.GreatestCommonDivisor(BigInteger.Abs(numerator), denominator);

        // Проверяем, что фабрика уже сократила дробь, потому что равенство и хеш зависят от канонических числовых данных.
        if (greatestCommonDivisor != BigInteger.One)
        {
            // Сообщаем о несокращенной форме, чтобы внутренний обход не мог обойти обязательную GCD reduction.
            throw new ArgumentException("Каноническая рациональная дробь AST должна быть сокращена по НОД.", nameof(numerator));
        }

        // Сохраняем канонический числитель без копирования, потому что BigInteger является неизменяемым значением.
        Numerator = numerator;

        // Сохраняем канонический положительный знаменатель без дополнительной нормализации.
        Denominator = denominator;
    }


    /// <summary>
    /// Возвращает канонический числитель точной рациональной дроби.
    /// Числитель может быть отрицательным. Именно он хранит знак рационального числа после нормализации знаменателя.
    /// </summary>
    public BigInteger Numerator { get; }


    /// <summary>
    /// Возвращает канонический положительный знаменатель точной рациональной дроби.
    /// Знаменатель всегда больше единицы, потому что знаменатель равный единице схлопывается через фабрику 
    /// в <see cref="AstInteger"/>.
    /// </summary>
    public BigInteger Denominator { get; }


    /// <summary>
    /// Сравнивает канонические числовые данные двух рациональных чисел одного конкретного CLR-типа.
    /// Базовый <see cref="AstNumber"/> уже проверил тип, поэтому здесь достаточно сравнить числитель и знаменатель.
    /// </summary>
    /// <param name="other">
    /// Другое числовое значение того же конкретного типа.
    /// </param>
    /// <returns>
    /// <see langword="true"/>, если числители и знаменатели совпадают в канонической форме.
    /// </returns>
    protected override bool PayloadEquals(AstNumber other)
    {
        // Приводим значение к AstRational после проверки конкретного типа в базовом классе.
        AstRational otherRational = (AstRational)other;

        // Сравниваем числитель и знаменатель как точные BigInteger-компоненты без десятичного округления.
        return Numerator == otherRational.Numerator && Denominator == otherRational.Denominator;
    }


    /// <summary>
    /// Добавляет канонические рациональные числовые данные в накопитель хеша.
    /// Хеш использует те же компоненты, что и равенство, поэтому сокращенные равные дроби получают одинаковый хеш.
    /// </summary>
    /// <param name="hashCode">
    /// Накопитель хеша числового значения.
    /// </param>
    protected override void AddPayloadHashCode(ref HashCode hashCode)
    {
        // Добавляем канонический числитель как первую часть рациональных числовых данных.
        hashCode.Add(Numerator);

        // Добавляем канонический знаменатель как вторую часть рациональных числовых данных.
        hashCode.Add(Denominator);
    }


    /// <summary>
    /// Печатает рациональное число как <c>Rational[p, q]</c>, например <c>Rational[-1, 2]</c>. Запись <c>p/q</c>
    /// намеренно не используется: в структурной форме она выглядела бы как узел деления.
    /// </summary>
    /// <param name="builder">
    /// Буфер формы.
    /// </param>
    internal override void AppendDebugPayload(StringBuilder builder)
    {
        builder.Append("Rational[")
            .Append(Numerator.ToString(CultureInfo.InvariantCulture))
            .Append(", ")
            .Append(Denominator.ToString(CultureInfo.InvariantCulture))
            .Append(']');
    }
}