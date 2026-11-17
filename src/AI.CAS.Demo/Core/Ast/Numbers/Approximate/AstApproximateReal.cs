using System.Globalization;
using System.Numerics;
using System.Text;

namespace AI.CAS.Core.Ast;

/// <summary>
/// Представляет каноническое приближенное десятичное вещественное значение Ast.
/// Математическая величина интерпретируется как <c>Significand * 10^DecimalExponent</c>, а
/// <see cref="PrecisionDigits"/> хранит заявленное количество значащих десятичных цифр. Эта модель намеренно не
/// использует <c>double</c>, <c>float</c>, <c>decimal</c> или исходную строку как каноническое хранилище: двоичная
/// арифметика с плавающей точкой добавила бы округление, <c>decimal</c> добавил бы ограниченный диапазон, а исходная
/// запись принадлежит метаданным представления.
/// </summary>
public sealed class AstApproximateReal : AstNumber
{
    /// <summary>
    /// Создает уже каноническое приближенное десятичное значение.
    /// Нормализация завершающих десятичных нулей и выбор нулевого показателя выполняются в <see cref="AstNumberCreate"/>,
    /// а этот конструктор только проверяет, что внутренний вызывающий код передал действительно канонические числовые данные.
    /// </summary>
    /// <param name="significand">
    /// Каноническая значащая часть. Для ненулевого значения она не должна оканчиваться десятичным нулем.
    /// </param>
    /// <param name="decimalExponent">
    /// Десятичный показатель в формуле <c>Significand * 10^DecimalExponent</c>.
    /// </param>
    /// <param name="precisionDigits">
    /// Положительное количество заявленных значащих десятичных цифр.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Выбрасывается, если точность не является положительной.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Выбрасывается, если канонические числовые данные противоречат требованиям приближенного числа.
    /// </exception>
    internal AstApproximateReal(BigInteger significand, int decimalExponent, int precisionDigits)
    {
        // Проверяем положительную точность до любых других проверок, потому что приближенное значение без значащих цифр не определено.
        if (precisionDigits <= 0)
        {
            // Сообщаем о некорректной заявленной точности через стандартный контракт выхода за допустимый диапазон.
            throw new ArgumentOutOfRangeException(nameof(precisionDigits), "Точность приближенного числа AST должна быть положительной.");
        }

        // Для нуля проверяем единственную предпочтительную каноническую форму с нулевым десятичным показателем.
        if (significand.IsZero)
        {
            // Проверяем, что фабрика не оставила альтернативный показатель для нулевой величины.
            if (decimalExponent != 0)
            {
                // Сообщаем о неканоническом нуле, потому что иначе ноль получил бы бесконечно много равных числовых данных.
                throw new ArgumentException("Канонический ноль приближенного числа AST должен иметь десятичный показатель 0.", nameof(decimalExponent));
            }

            // Сохраняем каноническую нулевую значащую часть.
            Significand = significand;

            // Сохраняем канонический нулевой показатель для нулевой величины.
            DecimalExponent = decimalExponent;

            // Сохраняем заявленную положительную точность, потому что приближенный ноль тоже может иметь заявленную точность.
            PrecisionDigits = precisionDigits;

            // Завершаем создание нуля после проверки его отдельного канонического инварианта.
            return;
        }

        // Проверяем отсутствие нулей у ненулевой значащей части.
        // Это необходимо для канонического представления числа, например:
        // 12*10^3 - каноническая форма, а 120*10^2 - нет.
        if (significand % new BigInteger(10) == BigInteger.Zero)
        {
            // Сообщаем о неканонической величине, потому что завершающий ноль должен быть перенесен в DecimalExponent.
            throw new ArgumentException("Каноническая ненулевая значащая часть приближенного числа AST не должна оканчиваться нулем.", nameof(significand));
        }

        // Считаем десятичные цифры абсолютной значащей части для проверки согласованности заявленной точности.
        int digitCount = CountDecimalDigits(significand);

        // Проверяем, что точность не меньше количества цифр, реально сохраненных в канонической значащей части.
        if (precisionDigits < digitCount)
        {
            // Сообщаем о противоречивых числовых данных, где заявлено меньше значащих цифр, чем хранит значение.
            throw new ArgumentException("Точность приближенного числа AST не может быть меньше числа цифр канонической значащей части.", nameof(precisionDigits));
        }

        // Сохраняем каноническую ненулевую значащую часть.
        Significand = significand;

        // Сохраняем десятичный показатель, уже проверенный фабрикой на переполнение во время нормализации.
        DecimalExponent = decimalExponent;

        // Сохраняем заявленную точность как часть семантических данных приближенного числа.
        PrecisionDigits = precisionDigits;
    }


    /// <summary>
    /// Возвращает каноническую значащую часть приближенного десятичного значения.
    /// Ненулевая значащая часть не оканчивается десятичным нулем, потому что такие нули переносятся в
    /// <see cref="DecimalExponent"/> без изменения <see cref="PrecisionDigits"/>.
    /// </summary>
    public BigInteger Significand { get; }


    /// <summary>
    /// Возвращает десятичный показатель в интерпретации <c>Significand * 10^DecimalExponent</c>.
    /// Показатель хранится как <see cref="int"/> и никогда не изменяется через переполнение при нормализации.
    /// </summary>
    public int DecimalExponent { get; }


    /// <summary>
    /// Возвращает заявленное количество значащих десятичных цифр.
    /// Точность является частью идентичности приближенного числа, поэтому одинаковая величина с разной 
    /// точностью остается разными объектами-значениями.
    /// </summary>
    public int PrecisionDigits { get; }


    /// <summary>
    /// Сравнивает канонические приближенные числовые данные двух приближенных чисел одного конкретного CLR-типа.
    /// Сравнение является точным сравнением компонентов и не вводит допуск или округление.
    /// </summary>
    /// <param name="other">
    /// Другое числовое значение того же конкретного типа.
    /// </param>
    /// <returns>
    /// <see langword="true"/>, если значащая часть, показатель и заявленная точность совпадают.
    /// </returns>
    protected override bool PayloadEquals(AstNumber other)
    {
        // Приводим значение к AstApproximateReal после проверки конкретного типа в базовом классе.
        AstApproximateReal otherApproximateReal = (AstApproximateReal)other;

        // Сравниваем все три компонента приближенных числовых данных без равенства по допуску.
        return Significand == otherApproximateReal.Significand
            && DecimalExponent == otherApproximateReal.DecimalExponent
            && PrecisionDigits == otherApproximateReal.PrecisionDigits;
    }


    /// <summary>
    /// Добавляет приближенные числовые данные в накопитель хеша.
    /// Хеш включает точность, потому что заявленная точность является частью семантической идентичности приближенного 
    /// числа.
    /// </summary>
    /// <param name="hashCode">
    /// Накопитель хеша числового значения.
    /// </param>
    protected override void AddPayloadHashCode(ref HashCode hashCode)
    {
        // Добавляем каноническую значащую часть.
        hashCode.Add(Significand);

        // Добавляем десятичный показатель величины.
        hashCode.Add(DecimalExponent);

        // Добавляем заявленную точность, чтобы одинаковая величина с разной точностью не смешивалась в хеше.
        hashCode.Add(PrecisionDigits);
    }


    /// <summary>
    /// Печатает приближенное число как <c>Real[значение, точность]</c>, например <c>Real[1.25, 3]</c> для
    /// <c>125 * 10^-2</c> с тремя значащими цифрами. Значение печатается точно, без округления: позиционной 
    /// десятичной записью, когда модуль показателя не больше <see cref="MaxPositionalDebugExponent"/>, и записью
    /// <c>значащая_часть e показатель</c> (<c>125e-40</c>) для больших показателей, чтобы форма не разрасталась нулями.
    /// </summary>
    /// <param name="builder">
    /// Буфер формы.
    /// </param>
    internal override void AppendDebugPayload(StringBuilder builder)
    {
        builder.Append("Real[");
        AppendExactDebugValue(builder);
        builder.Append(", ").Append(PrecisionDigits.ToString(CultureInfo.InvariantCulture)).Append(']');
    }


    /// <summary>
    /// Наибольший модуль десятичного показателя, при котором отладочная форма печатает значение позиционно.
    /// </summary>
    private const int MaxPositionalDebugExponent = 20;


    /// <summary>
    /// Печатает точное значение <c>Significand * 10^DecimalExponent</c> для отладочной формы.
    /// </summary>
    /// <param name="builder">
    /// Буфер формы.
    /// </param>
    private void AppendExactDebugValue(StringBuilder builder)
    {
        string digits = BigInteger.Abs(Significand).ToString(CultureInfo.InvariantCulture);

        if (Significand.Sign < 0)
        {
            builder.Append('-');
        }

        if (Math.Abs(DecimalExponent) > MaxPositionalDebugExponent)
        {
            builder.Append(digits).Append('e').Append(DecimalExponent.ToString(CultureInfo.InvariantCulture));
            return;
        }

        if (DecimalExponent >= 0)
        {
            builder.Append(digits).Append('0', DecimalExponent);
            return;
        }

        // Отрицательный показатель ставит десятичную точку внутрь цифр или добавляет ведущие нули: 125e-2 -> 1.25,
        // 125e-4 -> 0.0125.
        int fractionalDigitCount = -DecimalExponent;
        if (digits.Length > fractionalDigitCount)
        {
            builder.Append(digits, 0, digits.Length - fractionalDigitCount)
                .Append('.')
                .Append(digits, digits.Length - fractionalDigitCount, fractionalDigitCount);
            return;
        }

        builder.Append("0.").Append('0', fractionalDigitCount - digits.Length).Append(digits);
    }


    /// <summary>
    /// Считает количество десятичных цифр абсолютной ненулевой значащей части.
    /// Метод использует локальное десятичное форматирование только как способ точного подсчета цифр
    /// <see cref="BigInteger"/>; полученная строка не сохраняется в AST и не становится исходной записью.
    /// </summary>
    /// <param name="significand">
    /// Ненулевая значащая часть, для которой нужно посчитать десятичные цифры.
    /// </param>
    /// <returns>
    /// Количество десятичных цифр в абсолютном значении значащей части.
    /// </returns>
    private static int CountDecimalDigits(BigInteger significand)
    {
        // Берем абсолютное значение, потому что знак не является десятичной цифрой данных точности.
        BigInteger absoluteSignificand = BigInteger.Abs(significand);

        // Преобразуем BigInteger в десятичную строку только локально для точного подсчета цифр.
        string decimalDigits = absoluteSignificand.ToString(CultureInfo.InvariantCulture);

        // Возвращаем длину локальной строки как количество десятичных цифр.
        return decimalDigits.Length;
    }
}
