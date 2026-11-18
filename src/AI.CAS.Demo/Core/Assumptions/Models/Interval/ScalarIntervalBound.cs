using AI.CAS.Core.Ast;

namespace AI.CAS.Core.Assumptions;

/// <summary>
/// Представляет одну типизированную границу скалярного интервала.
/// Граница хранит открытость или закрытость как математическое состояние, а endpoint хранит как уже созданный
/// неизменяемый <see cref="AstNode"/>. Такой контракт нужен для доказательного слоя: выражение <c>x in [-1, 1]</c>
/// нельзя надежно доказать через строку <c>"[-1, 1]"</c>, потому что строка смешивает математический смысл,
/// форматирование и локализацию. Открытость endpoint должна быть доступна правилам напрямую, чтобы равенство
/// с концом принималось для закрытого интервала и отклонялось для открытого.
/// </summary>
/// <remarks>
/// <para>
/// Конструкторы <see cref="Open(AstNode)"/> и <see cref="Closed(AstNode)"/> намеренно отклоняют
/// приблизительно выглядящие числовые литералы внутри endpoint. Например, <c>1.0</c> не считается точным концом
/// <c>1</c>, потому что текущий Ast использует десятичные числа как приближенные данные.
/// Для главных интервалов обратных функций допускаются только точные выражения вроде <c>-1</c>,
/// <c>1</c>, <c>pi</c>, <c>pi/2</c> и <c>-pi/2</c>.
/// </para>
/// <para>
/// Неограниченная граница не хранит endpoint. Направление неограниченности определяется тем, используется ли эта
/// граница как нижняя или верхняя в <see cref="ScalarInterval"/>. Поэтому отдельные строковые
/// значения <c>-inf</c> и <c>+inf</c> не вводятся в модель границы.
/// </para>
/// </remarks>
public sealed record ScalarIntervalBound
{
    /// <summary>
    /// Создает типизированную границу интервала с уже проверенной формой.
    /// Закрытый конструктор сохраняет единый инвариант: конечные границы имеют точный endpoint, а неограниченная
    /// граница endpoint не имеет.
    /// </summary>
    /// <param name="kind">
    /// Семантический вид границы.
    /// </param>
    /// <param name="endpoint">
    /// Точный endpoint для открытой или закрытой границы, либо <c>null</c> для неограниченной границы.
    /// </param>
    private ScalarIntervalBound(ScalarIntervalBoundKind kind, AstNode? endpoint)
    {
        // Сохраняем проверенный вид границы как часть equality/hash-контракта.
        Kind = kind;

        // Сохраняем endpoint как ссылку на immutable AST-узел, не копируя и не изменяя исходное дерево.
        Endpoint = endpoint;
    }


    /// <summary>
    /// Возвращает неограниченную границу интервала.
    /// Значение является общим immutable-экземпляром, потому что не хранит endpoint и не содержит изменяемого
    /// состояния.
    /// </summary>
    public static ScalarIntervalBound Unbounded { get; } = new ScalarIntervalBound(ScalarIntervalBoundKind.Unbounded, endpoint: null);


    /// <summary>
    /// Возвращает семантический вид границы.
    /// </summary>
    public ScalarIntervalBoundKind Kind { get; }


    /// <summary>
    /// Возвращает точный endpoint конечной границы.
    /// Для <see cref="Unbounded"/> значение равно <c>null</c>, потому что у неограниченной стороны нет конечного
    /// выражения, которое можно было бы сравнивать с субъектом.
    /// </summary>
    public AstNode? Endpoint { get; }


    /// <summary>
    /// Возвращает признак открытой конечной границы.
    /// </summary>
    public bool IsOpen => Kind == ScalarIntervalBoundKind.Open;


    /// <summary>
    /// Возвращает признак закрытой конечной границы.
    /// </summary>
    public bool IsClosed => Kind == ScalarIntervalBoundKind.Closed;


    /// <summary>
    /// Возвращает признак неограниченной стороны.
    /// </summary>
    public bool IsUnbounded => Kind == ScalarIntervalBoundKind.Unbounded;


    /// <summary>
    /// Создает открытую конечную границу.
    /// Endpoint не включается в интервал, поэтому доказательство принадлежности должно требовать строгого
    /// сравнения с этой точкой.
    /// </summary>
    /// <param name="endpoint">
    /// Точный Ast endpoint границы.
    /// </param>
    /// <returns>
    /// Типизированная открытая граница.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Возникает, если endpoint отсутствует.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Возникает, если endpoint содержит приблизительно выглядящий числовой литерал.
    /// </exception>
    public static ScalarIntervalBound Open(AstNode endpoint)
    {
        // Проверяем endpoint до сохранения, потому что конечная граница без выражения не имеет математического смысла.
        ValidateFiniteEndpoint(endpoint);

        // Создаем открытую границу с точным endpoint.
        return new ScalarIntervalBound(ScalarIntervalBoundKind.Open, endpoint);
    }


    /// <summary>
    /// Создает закрытую конечную границу.
    /// Endpoint входит в интервал, поэтому доказательный слой может принимать равенство с этой точкой как достаточное
    /// доказательство соответствующей стороны.
    /// </summary>
    /// <param name="endpoint">
    /// Точный Ast endpoint границы.
    /// </param>
    /// <returns>
    /// Типизированная закрытая граница.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Возникает, если endpoint отсутствует.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Возникает, если endpoint содержит приблизительно выглядящий числовой литерал.
    /// </exception>
    public static ScalarIntervalBound Closed(AstNode endpoint)
    {
        // Проверяем endpoint до сохранения, потому что закрытая граница также требует точного конца.
        ValidateFiniteEndpoint(endpoint);

        // Создаем закрытую границу с точным endpoint.
        return new ScalarIntervalBound(ScalarIntervalBoundKind.Closed, endpoint);
    }


    /// <summary>
    /// Проверяет endpoint конечной границы на отсутствие приблизительно выглядящих числовых литералов.
    /// Метод не доказывает математическую точность произвольного символьного выражения. Он лишь запрещает
    /// те формы, которые текущий CAS уже распознает как небезопасные endpoints, например десятичные числа.
    /// Символические константы вроде <c>pi</c> остаются допустимыми, потому что они являются точными Ast-семантиками, 
    /// а не численными приближениями.
    /// </summary>
    /// <param name="endpoint">
    /// Проверяемый endpoint.
    /// </param>
    private static void ValidateFiniteEndpoint(AstNode endpoint)
    {
        // Проверяем endpoint до рекурсивного обхода.
        ArgumentNullException.ThrowIfNull(endpoint);

        // Если внутри endpoint найден приближенно выглядящий числовой литерал, не создаем ложный exact interval.
        if (ContainsApproximateNumericLiteral(endpoint))
        {
            // Бросаем ошибку модели, потому что такой endpoint нельзя безопасно использовать в доказательствах S12.
            throw new ArgumentException(
                "Граница интервала должна содержать только точные числовые литералы и точные символические AST-узлы.",
                nameof(endpoint));
        }
    }


    /// <summary>
    /// Проверяет, содержит ли Ast endpoint приближенный числовой литерал.
    /// Рекурсивный обход нужен для форм вроде <c>pi / 2</c>: сама дробь является символическим endpoint,
    /// но каждый числовой лист внутри нее должен оставаться точным числом, а не десятичной приближенной записью.
    /// </summary>
    /// <param name="node">
    /// Текущий узел endpoint.
    /// </param>
    /// <returns>
    /// <c>true</c>, если найден небезопасный числовой литерал, иначе <c>false</c>.
    /// </returns>
    private static bool ContainsApproximateNumericLiteral(AstNode node)
    {
        // Числовой узел endpoint проверяется через типизированную числовую модель Ast, а не через legacy-дискриминатор Type.
        if (node is AstNumberNode numberNode)
        {
            // Приближенное десятичное число вроде 1.0 отклоняется, потому что интервальная граница должна быть доказательно точной.
            if (numberNode.Number is AstApproximateReal)
            {
                // Сообщаем вызывающему валидатору, что endpoint содержит небезопасный числовой лист.
                return true;
            }

            // Точные целые и точные рациональные числа являются допустимыми endpoint-листьями native AST.
            if (numberNode.Number is AstInteger or AstRational)
            {
                // Продолжаем проверку как успешную для текущего числового листа.
                return false;
            }

            // Будущие числовые семейства консервативно отклоняются, пока их exact-семантика не будет описана явно.
            return true;
        }

        // Обходим дочерние узлы endpoint в исходном порядке без каноникализации.
        for (int i = 0; i < node.Count; i++)
        {
            // Если дочерний узел содержит approximate-число, весь endpoint небезопасен.
            if (ContainsApproximateNumericLiteral(node[i]))
            {
                // Сообщаем найденный небезопасный числовой лист.
                return true;
            }
        }

        // В endpoint не найдено approximate-числовых листьев.
        return false;
    }
}