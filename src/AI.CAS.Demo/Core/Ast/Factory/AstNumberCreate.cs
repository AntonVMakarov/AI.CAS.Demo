using System.Numerics;

namespace AI.CAS.Core.Ast;

/// <summary>
/// Предоставляет единственный публичный слой фабрик, который сохраняет инварианты числовых объектов-значений Ast.
/// Фабрика отделена от <see cref="AstCreate"/>, потому что числовая нормализация относится к семантическим числовым
/// данным, а не к построению узлов дерева выражений. Она не парсит строки, не выполняет арифметическое вычисление и 
/// не переносит символические константы в <see cref="AstNumber"/>. Её обязанность ограничена созданием точных целых, 
/// канонических точных рациональных и канонических приближенных вещественных значений.
/// </summary>
public static class AstNumberCreate
{
    /// <summary>
    /// Создает точное целое числовое значение Ast.
    /// Метод существует как единый публичный маршрут для данных точного целого числа и используется другими фабриками
    /// при схлопывании рационального значения со знаменателем один.
    /// </summary>
    /// <param name="value">
    /// Точное целое значение произвольного размера.
    /// </param>
    /// <returns>
    /// Новый <see cref="AstInteger"/> с теми же данными <see cref="BigInteger"/>.
    /// </returns>
    public static AstInteger Integer(BigInteger value)
    {
        // Создаем точный целочисленный объект-значение без строковой записи и без ограничения машинным диапазоном.
        return new AstInteger(value);
    }


    /// <summary>
    /// Создает точное рациональное числовое значение Ast через каноническую нормализацию.
    /// Фабрика переносит знак в числитель, сокращает дробь по НОД и возвращает <see cref="AstInteger"/>, если
    /// нормализованный знаменатель равен единице. Например, <c>2/4</c> становится <c>1/2</c>, <c>2/-4</c>
    /// становится <c>-1/2</c>, а <c>6/3</c> становится <see cref="AstInteger"/> со значением два.
    /// </summary>
    /// <param name="numerator">
    /// Входной числитель точной дроби.
    /// </param>
    /// <param name="denominator">
    /// Входной знаменатель точной дроби; ноль запрещен.
    /// </param>
    /// <returns>
    /// <see cref="AstInteger"/>, если дробь является целым числом, иначе канонический <see cref="AstRational"/>.
    /// </returns>
    /// <exception cref="DivideByZeroException">
    /// Выбрасывается, если <paramref name="denominator"/> равен нулю.
    /// </exception>
    public static AstNumber Rational(BigInteger numerator, BigInteger denominator)
    {
        // Проверяем знаменатель ноль до любой нормализации, потому что деление на ноль не является числовыми данными AST.
        if (denominator.IsZero)
        {
            // Используем явный DivideByZeroException как единый контракт ошибки фабрики точных рациональных чисел.
            throw new DivideByZeroException("Знаменатель точной рациональной дроби Ast не может быть равен нулю.");
        }

        // Нулевой числитель всегда задает точный целочисленный ноль независимо от ненулевого знаменателя.
        if (numerator.IsZero)
        {
            // Возвращаем единственное каноническое представление точного нуля.
            return Integer(BigInteger.Zero);
        }

        // Копируем входной числитель в локальную переменную, которую можно безопасно нормализовать без побочных эффектов.
        BigInteger normalizedNumerator = numerator;

        // Копируем входной знаменатель в локальную переменную для нормализации знака.
        BigInteger normalizedDenominator = denominator;

        // Переносим отрицательный знак знаменателя в числитель, чтобы знаменатель канонического рационального числа всегда был положительным.
        if (normalizedDenominator.Sign < 0)
        {
            // Меняем знак числителя, потому что дроби a/(-b) и (-a)/b математически равны.
            normalizedNumerator = -normalizedNumerator;

            // Делаем знаменатель положительным, устраняя вторую возможную запись того же точного рационального числа.
            normalizedDenominator = -normalizedDenominator;
        }

        // Вычисляем НОД абсолютного числителя и положительного знаменателя для обязательного сокращения дроби.
        BigInteger greatestCommonDivisor = BigInteger.GreatestCommonDivisor(BigInteger.Abs(normalizedNumerator), normalizedDenominator);

        // Делим числитель на НОД, чтобы равные дроби имели одинаковые канонические числовые данные.
        BigInteger canonicalNumerator = normalizedNumerator / greatestCommonDivisor;

        // Делим знаменатель на тот же НОД, сохраняя математическое значение дроби.
        BigInteger canonicalDenominator = normalizedDenominator / greatestCommonDivisor;

        // Проверяем схлопывание знаменателя один после сокращения, потому что точное целое число не должно иметь рациональный дубликат.
        if (canonicalDenominator.IsOne)
        {
            // Возвращаем AstInteger через тот же маршрут фабрики, который используется для обычных целых чисел.
            return Integer(canonicalNumerator);
        }

        // Создаем каноническое собственно рациональное число, передавая уже нормализованные компоненты во внутренний проверяющий конструктор.
        return new AstRational(canonicalNumerator, canonicalDenominator);
    }


    /// <summary>
    /// Создает приближенное десятичное вещественное значение Ast.
    /// Вход интерпретируется как <c>significand * 10^decimalExponent</c> с заданным количеством значащих цифр.
    /// Фабрика удаляет незначащие нули из ненулевой значащей части, увеличивая десятичный показатель, но
    /// не меняет <paramref name="precisionDigits"/>, потому что точность выражает заявленную числовую точность, а не исходную строковую запись.
    /// </summary>
    /// <param name="significand">
    /// Входная значащая часть приближенной величины.
    /// </param>
    /// <param name="decimalExponent">
    /// Входной десятичный показатель.
    /// </param>
    /// <param name="precisionDigits">
    /// Положительное количество заявленных значащих десятичных цифр.
    /// </param>
    /// <returns>
    /// Канонический <see cref="AstApproximateReal"/> с нормализованной величиной и сохраненной точностью.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Выбрасывается, если <paramref name="precisionDigits"/> не является положительным.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Выбрасывается, если заявленная точность меньше числа цифр канонической ненулевой значащей части.
    /// </exception>
    /// <exception cref="OverflowException">
    /// Выбрасывается, если удаление завершающих десятичных нулей потребовало бы увеличить десятичный показатель за предел <see cref="int.MaxValue"/>.
    /// </exception>
    public static AstApproximateReal ApproximateReal(BigInteger significand, int decimalExponent, int precisionDigits)
    {
        // Проверяем точность до нормализации, потому что нулевые и ненулевые приближенные значения одинаково требуют положительную точность.
        if (precisionDigits <= 0)
        {
            // Используем out-of-range контракт для недопустимого количества значащих цифр.
            throw new ArgumentOutOfRangeException(nameof(precisionDigits), "Точность приближенного числа Ast должна быть положительной.");
        }

        // Ноль получает единственный канонический показатель 0, но сохраняет переданную положительную точность.
        if (significand.IsZero)
        {
            // Создаем предпочтительные нулевые числовые данные без сохранения входного decimalExponent как исходной записи.
            return new AstApproximateReal(BigInteger.Zero, 0, precisionDigits);
        }

        // Копируем входную значащую часть, чтобы удалить завершающие десятичные нули локально и не смешивать вход с каноническими числовыми данными.
        BigInteger canonicalSignificand = significand;

        // Копируем входной показатель, который будет увеличиваться при переносе десятичных нулей из significand.
        int canonicalDecimalExponent = decimalExponent;

        // Создаем десятичную базу как BigInteger, чтобы проверка завершающих нулей оставалась в точной целочисленной арифметике.
        BigInteger decimalBase = new(10);

        // Удаляем завершающие десятичные нули из ненулевой значащей части, сохраняя математическую величину.
        while (canonicalSignificand % decimalBase == BigInteger.Zero)
        {
            // Проверяем переполнение до увеличения показателя, потому что циклический переход разрушил бы масштаб приближенного значения.
            if (canonicalDecimalExponent == int.MaxValue)
            {
                // Останавливаем нормализацию явным исключением, а не создаем неверный показатель.
                throw new OverflowException("Нормализация приближенного числа Ast переполнила десятичный показатель.");
            }

            // Делим значащую часть на десять, удаляя один завершающий ноль из представления величины.
            canonicalSignificand /= decimalBase;

            // Увеличиваем показатель на один, чтобы сохранить значение Significand * 10^DecimalExponent.
            canonicalDecimalExponent++;
        }

        // Создаем проверяющий объект-значение приближенного числа из канонических компонентов и исходной заявленной точности.
        return new AstApproximateReal(canonicalSignificand, canonicalDecimalExponent, precisionDigits);
    }
}