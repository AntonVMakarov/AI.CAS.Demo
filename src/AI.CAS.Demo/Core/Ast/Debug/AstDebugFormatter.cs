using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Globalization;
using System.Numerics;
using System.Text;

namespace AI.CAS.Core.Ast;

/// <summary>
/// Печатает точную структуру дерева <see cref="AstNode"/> в отладочной текстовой форме в духе <c>FullForm</c> из
/// Mathematica: короткий вид узла, его собственная нагрузка и дети в квадратных скобках, например
/// <c>Plus[Times[2, x], 1]</c>.
/// </summary>
/// <remarks>
/// <para>
/// <b>Только для разработчика.</b> Форма служит сообщениям об ошибках тестов, отладчику и журналам диагностики. Она
/// никогда не должна попадать в текст, который видит учащийся: для этого существуют рендереры слоя
/// <c>Presentation</c>. Архитектурный страж запрещает <c>Presentation</c> обращаться к форматтеру и к
/// <see cref="AstNode.ToString"/>, кроме прокси отладчика.
/// </para>
/// <para>
/// <b>Структура, а не красота.</b> Форма различает деревья, которые человеку показываются одинаково: <c>Minus[x, 1]</c>,
/// <c>Plus[x, UnaryMinus[1]]</c> и <c>Plus[x, -1]</c> — это три разных дерева, хотя все они читаются как
/// <c>x − 1</c>. Именно такие различия и ломают структурное равенство в тестах.
/// </para>
/// <para>
/// <b>Правила печати.</b> Вид узла выводится из имени CLR-типа без префикса <c>Ast</c> и суффикса <c>Node</c>
/// (<c>AstPlusNode</c> → <c>Plus</c>). Собственную нагрузку каждый узел добавляет сам через внутренний хук
/// <see cref="AstNode.AppendDebugPayload"/>, по образцу <c>PayloadEquals</c> и <c>AddPayloadHashCode</c>; дети берутся
/// из <see cref="AstNode.Children"/>. Числа и переменные печатаются одной нагрузкой: переменная — именем, целое —
/// цифрами (<c>-3</c>), рациональное — как <c>Rational[p, q]</c> (никогда <c>p/q</c>, что выглядело бы делением),
/// приближенное вещественное — как <c>Real[значение, точность]</c>. Лист без нагрузки печатается одним видом
/// (<c>Pi</c>), лист с нагрузкой — видом и нагрузкой (<c>RealRootOf[{-2, 0, 1}, 1]</c>). Новый тип узла печатается
/// правильно без изменения форматтера: достаточно переопределить хук нагрузки, если у узла есть собственные данные.
/// </para>
/// <para>
/// <b>Ограничения.</b> Имена переменных и функций печатаются как есть, без экранирования, поэтому переменная с
/// именем, совпадающим с видом листа без нагрузки (например <c>Pi</c>), печатается так же, как этот лист.
/// Форматтер не читает члены узлов отражением: только имя типа, детей и хук нагрузки.
/// </para>
/// <para>
/// <b>Глубина и размер.</b> Обход идет по явному стеку, поэтому очень глубокие деревья не переполняют стек вызовов.
/// Перегрузка с ограничением длины прекращает обход, как только результат превысил лимит, и не строит полную строку
/// огромного выражения. Числа форматируются независимо от культуры потока.
/// </para>
/// </remarks>
internal sealed class AstDebugFormatter
{
    /// <summary>
    /// Маркер обрезанной формы.
    /// </summary>
    internal const string TruncationMarker = "…";

    /// <summary>
    /// Префикс имени CLR-типа узла, который не входит в вид узла.
    /// </summary>
    private const string TypeNamePrefix = "Ast";

    /// <summary>
    /// Суффикс имени CLR-типа узла, который не входит в вид узла.
    /// </summary>
    private const string TypeNameSuffix = "Node";

    /// <summary>
    /// Запомненные виды узлов по CLR-типу: вид вычисляется из имени типа один раз на тип.
    /// </summary>
    private readonly ConcurrentDictionary<Type, string> _kindByType = new();

    /// <summary>
    /// Возвращает общий экземпляр форматтера, которым пользуются <see cref="AstNode.ToString"/> и отображение узла в
    /// отладчике. Общий экземпляр допустим: у форматтера нет соавторов, а его единственное состояние — потокобезопасный
    /// кэш видов узлов, не влияющий на результат.
    /// </summary>
    internal static AstDebugFormatter Shared { get; } = new();

    /// <summary>
    /// Печатает полную структурную форму дерева.
    /// </summary>
    /// <param name="node">
    /// Корень печатаемого дерева.
    /// </param>
    /// <returns>
    /// Полная структурная форма, например <c>Plus[Times[2, x], 1]</c>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Возникает, если узел отсутствует.
    /// </exception>
    internal string Format(AstNode node)
    {
        return Format(node, int.MaxValue);
    }

    /// <summary>
    /// Печатает структурную форму дерева, обрезанную до заданной длины.
    /// </summary>
    /// <param name="node">
    /// Корень печатаемого дерева.
    /// </param>
    /// <param name="maxLength">
    /// Наибольшее число символов формы до маркера обрезки. Если полная форма длиннее, возвращаются ее первые
    /// <paramref name="maxLength"/> символов и <see cref="TruncationMarker"/>.
    /// </param>
    /// <returns>
    /// Полная форма, если она не длиннее лимита, иначе ее начало с маркером обрезки.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Возникает, если узел отсутствует.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Возникает, если лимит отрицателен.
    /// </exception>
    internal string Format(AstNode node, int maxLength)
    {
        ArgumentNullException.ThrowIfNull(node);
        ArgumentOutOfRangeException.ThrowIfNegative(maxLength);

        StringBuilder builder = new();

        // Кадры открытых составных узлов: вместо рекурсии, чтобы глубина дерева не ограничивалась стеком вызовов.
        List<OpenNodeFrame> openNodes = [];

        if (AppendNodeOpening(builder, node, out bool rootHasPayload))
        {
            openNodes.Add(new OpenNodeFrame(node, rootHasPayload));
        }

        while (openNodes.Count > 0)
        {
            // Обрезанную форму дальше не строим: огромное выражение не должно обходиться целиком ради отладчика.
            if (builder.Length > maxLength)
            {
                break;
            }

            int topIndex = openNodes.Count - 1;
            OpenNodeFrame top = openNodes[topIndex];
            ImmutableArray<AstNode> children = top.Node.Children;

            if (top.NextChildIndex == children.Length)
            {
                builder.Append(']');
                openNodes.RemoveAt(topIndex);
                continue;
            }

            // Разделитель нужен перед каждым ребенком, кроме первого, если до него не было собственной нагрузки.
            if (top.NextChildIndex > 0 || top.HasPayload)
            {
                builder.Append(", ");
            }

            AstNode child = children[top.NextChildIndex];
            openNodes[topIndex] = top with { NextChildIndex = top.NextChildIndex + 1 };

            if (AppendNodeOpening(builder, child, out bool childHasPayload))
            {
                openNodes.Add(new OpenNodeFrame(child, childHasPayload));
            }
        }

        if (builder.Length > maxLength)
        {
            builder.Length = maxLength;
            builder.Append(TruncationMarker);
        }

        return builder.ToString();
    }

    /// <summary>
    /// Печатает последовательность точных целых чисел в фигурных скобках, например <c>{-2, 0, 1}</c>.
    /// Используется нагрузкой узлов, которые хранят коэффициенты задающего многочлена.
    /// </summary>
    /// <param name="builder">
    /// Буфер формы.
    /// </param>
    /// <param name="values">
    /// Печатаемые числа в исходном порядке.
    /// </param>
    internal static void AppendIntegerSequence(StringBuilder builder, IReadOnlyList<BigInteger> values)
    {
        builder.Append('{');
        for (int index = 0; index < values.Count; index++)
        {
            if (index > 0)
            {
                builder.Append(", ");
            }

            builder.Append(values[index].ToString(CultureInfo.InvariantCulture));
        }

        builder.Append('}');
    }

    /// <summary>
    /// Печатает начало узла: для листа — его полную форму, для составного узла — вид, открывающую скобку и нагрузку.
    /// </summary>
    /// <param name="builder">
    /// Буфер формы.
    /// </param>
    /// <param name="node">
    /// Печатаемый узел.
    /// </param>
    /// <param name="hasPayload">
    /// Для составного узла — признак того, что после открывающей скобки уже напечатана собственная нагрузка и перед
    /// первым ребенком нужен разделитель.
    /// </param>
    /// <returns>
    /// <see langword="true"/>, если узел составной и его детей еще предстоит напечатать.
    /// </returns>
    private bool AppendNodeOpening(StringBuilder builder, AstNode node, out bool hasPayload)
    {
        hasPayload = false;

        if (node.IsLeaf && node.PrintsDebugPayloadWithoutKind)
        {
            node.AppendDebugPayload(builder);
            return false;
        }

        builder.Append(GetKind(node));
        int openingBracketPosition = builder.Length;
        builder.Append('[');
        int payloadStart = builder.Length;
        node.AppendDebugPayload(builder);
        hasPayload = builder.Length > payloadStart;

        if (!node.IsLeaf)
        {
            return true;
        }

        // Лист без нагрузки печатается одним видом (Pi), а не Pi[].
        if (hasPayload)
        {
            builder.Append(']');
        }
        else
        {
            builder.Length = openingBracketPosition;
        }

        return false;
    }

    /// <summary>
    /// Возвращает вид узла: имя CLR-типа без префикса <c>Ast</c> и суффикса <c>Node</c>.
    /// </summary>
    /// <param name="node">
    /// Узел, вид которого нужен.
    /// </param>
    /// <returns>
    /// Вид узла, например <c>Plus</c> для <see cref="AstPlusNode"/>.
    /// </returns>
    private string GetKind(AstNode node)
    {
        return _kindByType.GetOrAdd(node.GetType(), static type => ComputeKind(type.Name));
    }

    /// <summary>
    /// Вычисляет вид узла из имени CLR-типа. Префикс и суффикс снимаются только тогда, когда после этого остается
    /// непустое имя, поэтому имя типа вне соглашения печатается как есть.
    /// </summary>
    /// <param name="typeName">
    /// Короткое имя CLR-типа узла.
    /// </param>
    /// <returns>
    /// Вид узла.
    /// </returns>
    private static string ComputeKind(string typeName)
    {
        string kind = typeName;

        if (kind.Length > TypeNamePrefix.Length && kind.StartsWith(TypeNamePrefix, StringComparison.Ordinal))
        {
            kind = kind[TypeNamePrefix.Length..];
        }

        if (kind.Length > TypeNameSuffix.Length && kind.EndsWith(TypeNameSuffix, StringComparison.Ordinal))
        {
            kind = kind[..^TypeNameSuffix.Length];
        }

        return kind;
    }

    /// <summary>
    /// Кадр открытого составного узла в явном стеке обхода.
    /// </summary>
    /// <param name="Node">
    /// Открытый узел.
    /// </param>
    /// <param name="HasPayload">
    /// Признак того, что у узла напечатана собственная нагрузка.
    /// </param>
    private readonly record struct OpenNodeFrame(AstNode Node, bool HasPayload)
    {
        /// <summary>
        /// Возвращает позицию следующего ребенка, которого нужно напечатать.
        /// </summary>
        public int NextChildIndex { get; init; }
    }
}
