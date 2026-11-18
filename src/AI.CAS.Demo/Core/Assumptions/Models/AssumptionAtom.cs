using AI.CAS.Core.Ast;
using AI.CAS.Core.Domains;

namespace AI.CAS.Core.Assumptions;

/// <summary>
/// Представляет одно математическое утверждение без истинностного значения.
/// Атом является общей моделью математического утверждения для запросов, фактов и семантических условий: он отвечает только
/// на вопрос "о каком утверждении идет речь", но не отвечает на вопрос "истинно ли это утверждение".
/// </summary>
/// <remarks>
/// <para>
/// Важная архитектурная граница состоит в том, что <see cref="AssumptionAtom"/> не выполняет
/// математический вывод. Создание атома <c>Positive(x)</c> не доказывает <c>NonZero(x)</c>, создание атома
/// <c>Equal(x, y)</c> не симметризует его в <c>Equal(y, x)</c>, а создание атома доменного членства не проверяет
/// решетку областей. Все такие действия принадлежат отдельным поставщикам предположений.
/// </para>
/// <para>
/// Проверка внутри атома является только структурной. Она запрещает бессмысленные формы вроде
/// <c>Positive(x, y)</c>, <c>Equal(x, y, Real)</c>, <c>DomainMember(x)</c> или <c>InInterval(x)</c> без
/// типизированного интервала. Благодаря этому нижестоящий код может сравнивать ключи утверждений напрямую и не
/// повторять одинаковые защитные условия в каждом поставщике.
/// </para>
/// <para>
/// Интервальная принадлежность хранится отдельным полем <see cref="Interval"/>. Это сделано намеренно:
/// интервал не является вторым выражением бинарного отношения и не является доменной областью. Например,
/// <c>x in [-1, 1]</c> требует сохранить открытость или закрытость endpoint, а строковая запись такого условия
/// не является доказательным контрактом ядра CAS.
/// </para>
/// <para>
/// Точное совпадение атомов использует текущую семантику равенства <see cref="AstNode"/>. Это означает, что
/// коммутативные узлы дерева <c>+</c> и <c>*</c> сравниваются так же, как они сравнивались до введения атома:
/// например, <c>Positive(x + y)</c> и <c>Positive(y + x)</c> считаются одним и тем же атомом, если сами узлы
/// равны по правилам <see cref="AstNode"/>.
/// </para>
/// </remarks>
public sealed record AssumptionAtom
{
    /// <summary>Закрытый конструктор с параметрами</summary>
    private AssumptionAtom(AstNode subject, AssumptionPredicateKind predicate, AstNode? comparedTo, IDomain? domain, ScalarInterval? interval)
    {
        // Сохраняем субъект как ссылку на неизменяемый Ast узел без копирования и мутации.
        Subject = subject;

        // Сохраняем вид предиката как часть ключа атома.
        Predicate = predicate;

        // Сохраняем второе выражение только для бинарных отношений.
        ComparedTo = comparedTo;

        // Сохраняем домен только для атомов DomainMember.
        Domain = domain;

        // Сохраняем типизированный интервал только для атомов InInterval.
        Interval = interval;
    }


    /// <summary>
    /// Возвращает выражение, к которому относится утверждение.
    /// Значение является уже созданным неизменяемым узлом дерева выражения и не изменяется атомом.
    /// </summary>
    public AstNode Subject { get; }


    /// <summary>
    /// Возвращает вид математического предиката.
    /// Предикат задает форму утверждения: унарное свойство, бинарное отношение или принадлежность области.
    /// </summary>
    public AssumptionPredicateKind Predicate { get; }


    /// <summary>
    /// Возвращает второе выражение для бинарных отношений <see cref="AssumptionPredicateKind.Equal"/> и
    /// <see cref="AssumptionPredicateKind.NotEqual"/>.
    /// Для унарных и доменных предикатов значение всегда равно <c>null</c>.
    /// </summary>
    public AstNode? ComparedTo { get; }


    /// <summary>
    /// Возвращает область для предиката <see cref="AssumptionPredicateKind.DomainMember"/>.
    /// Для унарных и бинарных предикатов значение всегда равно <c>null</c>.
    /// </summary>
    public IDomain? Domain { get; }


    /// <summary>
    /// Возвращает типизированный интервал для предиката <see cref="AssumptionPredicateKind.InInterval"/>.
    /// Для всех старых унарных, бинарных и доменных атомов значение равно <c>null</c>, чтобы существующее
    /// поведение оставалось совместимым. Интервал хранится здесь как семантическая модель, потому что proof-слой
    /// должен различать <c>[-1, 1]</c> и <c>(-1, 1)</c> без чтения строк отображения.
    /// </summary>
    public ScalarInterval? Interval { get; }


    /// <summary>
    /// Создает атом положительности <c>subject &gt; 0</c>.
    /// </summary>
    /// <param name="subject">
    /// Проверяемое выражение.
    /// </param>
    /// <returns>
    /// Валидный унарный атом положительности.
    /// </returns>
    public static AssumptionAtom Positive(AstNode subject)
    {
        return Create(subject, AssumptionPredicateKind.Positive);
    }


    /// <summary>
    /// Создает атом отрицательности <c>subject &lt; 0</c>.
    /// </summary>
    /// <param name="subject">
    /// Проверяемое выражение.
    /// </param>
    /// <returns>
    /// Валидный унарный атом отрицательности.
    /// </returns>
    public static AssumptionAtom Negative(AstNode subject)
    {
        return Create(subject, AssumptionPredicateKind.Negative);
    }


    /// <summary>
    /// Создает атом неотрицательности <c>subject &gt;= 0</c>.
    /// </summary>
    /// <param name="subject">
    /// Проверяемое выражение.
    /// </param>
    /// <returns>
    /// Валидный унарный атом неотрицательности.
    /// </returns>
    public static AssumptionAtom NonNegative(AstNode subject)
    {
        return Create(subject, AssumptionPredicateKind.NonNegative);
    }


    /// <summary>
    /// Создает атом неположительности <c>subject &lt;= 0</c>.
    /// </summary>
    /// <param name="subject">
    /// Проверяемое выражение.
    /// </param>
    /// <returns>
    /// Валидный унарный атом неположительности.
    /// </returns>
    public static AssumptionAtom NonPositive(AstNode subject)
    {
        return Create(subject, AssumptionPredicateKind.NonPositive);
    }


    /// <summary>
    /// Создает атом не равенства нулю <c>subject != 0</c>.
    /// </summary>
    /// <param name="subject">
    /// Проверяемое выражение.
    /// </param>
    /// <returns>
    /// Валидный унарный атом ненулевости.
    /// </returns>
    public static AssumptionAtom NonZero(AstNode subject)
    {
        return Create(subject, AssumptionPredicateKind.NonZero);
    }


    /// <summary>
    /// Создает атом точного равенства нулю <c>subject == 0</c>.
    /// </summary>
    /// <param name="subject">
    /// Проверяемое выражение.
    /// </param>
    /// <returns>
    /// Валидный унарный атом нулевости.
    /// </returns>
    public static AssumptionAtom Zero(AstNode subject)
    {
        return Create(subject, AssumptionPredicateKind.Zero);
    }


    /// <summary>
    /// Создает атом вещественности выражения.
    /// </summary>
    /// <param name="subject">
    /// Проверяемое выражение.
    /// </param>
    /// <returns>
    /// Валидный унарный атом вещественности.
    /// </returns>
    public static AssumptionAtom Real(AstNode subject)
    {
        return Create(subject, AssumptionPredicateKind.Real);
    }


    /// <summary>
    /// Создает атом целочисленности выражения.
    /// </summary>
    /// <param name="subject">
    /// Проверяемое выражение.
    /// </param>
    /// <returns>
    /// Валидный унарный атом целочисленности.
    /// </returns>
    public static AssumptionAtom Integer(AstNode subject)
    {
        return Create(subject, AssumptionPredicateKind.Integer);
    }


    /// <summary>
    /// Создает атом конечности выражения.
    /// </summary>
    /// <param name="subject">
    /// Проверяемое выражение.
    /// </param>
    /// <returns>
    /// Валидный унарный атом конечности.
    /// </returns>
    public static AssumptionAtom Finite(AstNode subject)
    {
        return Create(subject, AssumptionPredicateKind.Finite);
    }


    /// <summary>
    /// Создает атом неконечности выражения.
    /// </summary>
    /// <param name="subject">
    /// Проверяемое выражение.
    /// </param>
    /// <returns>
    /// Валидный унарный атом неконечности.
    /// </returns>
    public static AssumptionAtom NonFinite(AstNode subject)
    {
        return Create(subject, AssumptionPredicateKind.NonFinite);
    }


    /// <summary>
    /// Создает атом принадлежности значения образу главной ветви комплексного логарифма.
    /// Для субъекта <c>c</c> этот атом означает условие <c>-pi &lt; Im(c) &lt;= pi</c> в принятом
    /// в CAS соглашении <c>Arg(z) in (-pi, pi]</c>. Атом хранит именно математический смысл главной полосы,
    /// а не строку <c>"-pi &lt; Im(c) &lt;= pi"</c>, чтобы условные результаты различных модулей могли переносить это
    /// обязательство через другие слои без разбора отображаемого текста.
    /// </summary>
    /// <param name="subject">
    /// Значение главного логарифма, мнимая часть которого должна лежать в образе выбранной ветви.
    /// </param>
    /// <returns>
    /// Валидный унарный атом принадлежности principal-strip образу.
    /// </returns>
    public static AssumptionAtom PrincipalLogImageStrip(AstNode subject)
    {
        // Создаем атом через единую фабрику, чтобы сохранить общую структурную проверку AssumptionAtom.
        return Create(subject, AssumptionPredicateKind.PrincipalLogImageStrip);
    }


    /// <summary>
    /// Создает атом принадлежности расширенной вещественной прямой.
    /// Атом не доказывает конечную вещественность: <c>ExtendedReal(x)</c> и <c>Real(x)</c> намеренно остаются
    /// разными предикатами, потому что <c>ExtendedReal</c> допускает направленные бесконечности.
    /// </summary>
    /// <param name="subject">
    /// Проверяемое выражение.
    /// </param>
    /// <returns>
    /// Валидный унарный атом расширенной вещественности.
    /// </returns>
    public static AssumptionAtom ExtendedReal(AstNode subject)
    {
        return Create(subject, AssumptionPredicateKind.ExtendedReal);
    }


    /// <summary>
    /// Создает атом расширенной положительности.
    /// Такой атом может описывать как обычное конечное положительное число, так и <c>+∞</c>, поэтому он не
    /// должен использоваться потребителями как замена обычного <see cref="Positive(AstNode)"/>.
    /// </summary>
    /// <param name="subject">
    /// Проверяемое выражение.
    /// </param>
    /// <returns>
    /// Валидный унарный атом расширенной положительности.
    /// </returns>
    public static AssumptionAtom ExtendedPositive(AstNode subject)
    {
        return Create(subject, AssumptionPredicateKind.ExtendedPositive);
    }


    /// <summary>
    /// Создает атом расширенной отрицательности.
    /// Такой атом может описывать как обычное конечное отрицательное число, так и <c>-∞</c>.
    /// </summary>
    /// <param name="subject">
    /// Проверяемое выражение.
    /// </param>
    /// <returns>
    /// Валидный унарный атом расширенной отрицательности.
    /// </returns>
    public static AssumptionAtom ExtendedNegative(AstNode subject)
    {
        return Create(subject, AssumptionPredicateKind.ExtendedNegative);
    }


    /// <summary>
    /// Создает атом расширенной неотрицательности.
    /// Предикат допускает <c>+∞</c>, поэтому он не является обычным конечным утверждением <c>subject >= 0</c>.
    /// </summary>
    /// <param name="subject">
    /// Проверяемое выражение.
    /// </param>
    /// <returns>
    /// Валидный унарный атом расширенной неотрицательности.
    /// </returns>
    public static AssumptionAtom ExtendedNonNegative(AstNode subject)
    {
        return Create(subject, AssumptionPredicateKind.ExtendedNonNegative);
    }


    /// <summary>
    /// Создает атом расширенной неположительности.
    /// Предикат допускает <c>-∞</c>, поэтому он отделен от обычного конечного
    /// <see cref="NonPositive(AstNode)"/>.
    /// </summary>
    /// <param name="subject">
    /// Проверяемое выражение.
    /// </param>
    /// <returns>
    /// Валидный унарный атом расширенной неположительности.
    /// </returns>
    public static AssumptionAtom ExtendedNonPositive(AstNode subject)
    {
        return Create(subject, AssumptionPredicateKind.ExtendedNonPositive);
    }


    /// <summary>
    /// Создает атом равенства двух выражений.
    /// Метод не симметризует операнды и не упрощает выражения: ориентация сохраняется буквально как часть атома.
    /// </summary>
    /// <param name="left">
    /// Левая часть равенства.
    /// </param>
    /// <param name="right">
    /// Правая часть равенства.
    /// </param>
    /// <returns>
    /// Валидный бинарный атом равенства.
    /// </returns>
    public static AssumptionAtom Equal(AstNode left, AstNode right)
    {
        ArgumentNullException.ThrowIfNull(right);

        return Create(left, AssumptionPredicateKind.Equal, comparedTo: right);
    }


    /// <summary>
    /// Создает атом неравенства двух выражений.
    /// Метод сохраняет заданный порядок операндов, потому что симметрия относится к
    /// <see cref="RelationalAssumptionProvider"/>, а не к контейнеру данных математического утверждения.
    /// </summary>
    /// <param name="left">
    /// Левая часть неравенства.
    /// </param>
    /// <param name="right">
    /// Правая часть неравенства.
    /// </param>
    /// <returns>
    /// Валидный бинарный атом неравенства.
    /// </returns>
    public static AssumptionAtom NotEqual(AstNode left, AstNode right)
    {
        ArgumentNullException.ThrowIfNull(right);

        return Create(left, AssumptionPredicateKind.NotEqual, comparedTo: right);
    }


    /// <summary>
    /// Создает атом принадлежности выражения математической области.
    /// </summary>
    /// <param name="subject">
    /// Проверяемое выражение.
    /// </param>
    /// <param name="domain">
    /// Область, принадлежность которой формулируется.
    /// </param>
    /// <returns>
    /// Валидный доменный атом.
    /// </returns>
    public static AssumptionAtom DomainMember(AstNode subject, IDomain domain)
    {
        ArgumentNullException.ThrowIfNull(domain);

        return Create(subject, AssumptionPredicateKind.DomainMember, domain: domain);
    }


    /// <summary>
    /// Создает атом принадлежности выражения точному скалярному интервалу.
    /// Интервал сохраняется в виде типизированных данных, а не в виде <see cref="ComparedTo"/>, <see cref="Domain"/>,
    /// строки, XML, LaTeX или локализованного описания.
    /// </summary>
    /// <param name="subject">
    /// Проверяемое выражение.
    /// </param>
    /// <param name="interval">
    /// Точный типизированный интервал, принадлежность которому формулируется.
    /// </param>
    /// <returns>
    /// Валидный интервальный атом.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Возникает, если субъект или интервал отсутствует.
    /// </exception>
    public static AssumptionAtom InInterval(AstNode subject, ScalarInterval interval)
    {
        // Проверяем интервал до делегирования, чтобы ошибка вызывающего кода была связана с интервальным payload.
        ArgumentNullException.ThrowIfNull(interval);

        // Создаем атом через единую фабрику структурной проверки.
        return Create(subject, AssumptionPredicateKind.InInterval, interval: interval);
    }


    /// <summary>
    /// Создает атом предположений из общей формы и проверяет структурную корректность полей.
    /// Метод является единственной точкой логики проверки для старых совместимых конструкторов и новых
    /// специализированных фабрик.
    /// </summary>
    /// <param name="subject">
    /// Субъект атома. Значение обязательно для всех предикатов.
    /// </param>
    /// <param name="predicate">
    /// Вид предиката.
    /// </param>
    /// <param name="comparedTo">
    /// Второе выражение для <see cref="AssumptionPredicateKind.Equal"/> и
    /// <see cref="AssumptionPredicateKind.NotEqual"/>.
    /// </param>
    /// <param name="domain">
    /// Область для <see cref="AssumptionPredicateKind.DomainMember"/>.
    /// </param>
    /// <param name="interval">
    /// Типизированный интервал для <see cref="AssumptionPredicateKind.InInterval"/>.
    /// </param>
    /// <returns>
    /// Валидный неизменяемый атом утверждения.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Возникает, если субъект отсутствует.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Возникает, если набор полей не соответствует форме предиката.
    /// </exception>
    public static AssumptionAtom Create(
        AstNode subject,
        AssumptionPredicateKind predicate,
        AstNode? comparedTo = null,
        IDomain? domain = null,
        ScalarInterval? interval = null)
    {
        // Проверяем субъект до структурной проверки, потому что все формы атомов требуют выражение.
        ArgumentNullException.ThrowIfNull(subject);

        // Проверяем согласованность дополнительных полей с выбранным предикатом.
        ValidatePredicateShape(predicate, comparedTo, domain, interval);

        // Создаем immutable-атом с уже проверенным payload.
        return new AssumptionAtom(subject, predicate, comparedTo, domain, interval);
    }


    /// <summary>
    /// Проверяет, что дополнительные поля соответствуют форме предиката.
    /// Метод намеренно не смотрит на математическое содержимое выражений: например, он не проверяет, является ли
    /// <c>ComparedTo</c> нулем для специальных нулевых правил. Его задача только структурная.
    /// </summary>
    /// <param name="predicate">
    /// Вид предиката.
    /// </param>
    /// <param name="comparedTo">
    /// Второе выражение, если оно задано.
    /// </param>
    /// <param name="domain">
    /// Область, если она задана.
    /// </param>
    /// <param name="interval">
    /// Типизированный интервал, если он задан.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Возникает, если поля не соответствуют форме предиката.
    /// </exception>
    private static void ValidatePredicateShape(
        AssumptionPredicateKind predicate,
        AstNode? comparedTo,
        IDomain? domain,
        ScalarInterval? interval)
    {
        // Проверяем, что вызывающий код не передал несуществующее enum-значение через приведение.
        if (!Enum.IsDefined(predicate))
        {
            // Бросаем точную ошибку модели, чтобы неизвестный предикат не стал молчаливой унарной формой.
            throw new ArgumentOutOfRangeException(
                nameof(predicate),
                predicate,
                "Предикат атома предположений должен быть определенным значением.");
        }

        if (predicate is AssumptionPredicateKind.Equal or AssumptionPredicateKind.NotEqual)
        {
            if (comparedTo is null)
            {
                throw new ArgumentException(
                    "Бинарные предикаты равенства и неравенства требуют второго выражения.",
                    nameof(comparedTo));
            }

            if (domain is not null)
            {
                throw new ArgumentException(
                    "Бинарные предикаты равенства и неравенства не должны содержать доменную область.",
                    nameof(domain));
            }

            if (interval is not null)
            {
                throw new ArgumentException(
                    "Бинарные предикаты равенства и неравенства не должны содержать интервальную модель.",
                    nameof(interval));
            }

            return;
        }

        if (predicate == AssumptionPredicateKind.DomainMember)
        {
            if (domain is null)
            {
                throw new ArgumentException(
                    "Предикат принадлежности области требует указанную область.",
                    nameof(domain));
            }

            if (comparedTo is not null)
            {
                throw new ArgumentException(
                    "Предикат принадлежности области не должен содержать второе выражение сравнения.",
                    nameof(comparedTo));
            }

            if (interval is not null)
            {
                throw new ArgumentException(
                    "Предикат принадлежности области не должен содержать интервальную модель.",
                    nameof(interval));
            }

            return;
        }

        if (predicate == AssumptionPredicateKind.InInterval)
        {
            if (interval is null)
            {
                throw new ArgumentException(
                    "Предикат интервальной принадлежности требует типизированный интервал.",
                    nameof(interval));
            }

            if (comparedTo is not null)
            {
                throw new ArgumentException(
                    "Предикат интервальной принадлежности не должен содержать второе выражение сравнения.",
                    nameof(comparedTo));
            }

            if (domain is not null)
            {
                throw new ArgumentException(
                    "Предикат интервальной принадлежности не должен содержать доменную область.",
                    nameof(domain));
            }

            return;
        }

        if (comparedTo is not null)
        {
            throw new ArgumentException(
                "Унарные предикаты предположений не должны содержать второе выражение сравнения.",
                nameof(comparedTo));
        }

        if (domain is not null)
        {
            throw new ArgumentException(
                "Унарные предикаты предположений не должны содержать доменную область.",
                nameof(domain));
        }

        if (interval is not null)
        {
            throw new ArgumentException(
                "Унарные предикаты предположений не должны содержать интервальную модель.",
                nameof(interval));
        }
    }
}