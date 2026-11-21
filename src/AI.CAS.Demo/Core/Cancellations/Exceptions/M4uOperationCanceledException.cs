namespace AI.CAS.Core.Cancellations;

/// <summary>Конкретная реализация абстрактного класса для описания исключения, возникающего 
/// когда операция (вычисления/отрисовки) была отменена из-за того, что она превысила время, отведённое на неё</summary>
public class M4uOperationCanceledException : OperationCanceledException
{
    /// <summary>Описание исключения</summary>
    private const string DefaultMessage = "Операция была отменена по причине того, что время, отведённое на неё, закончилось.";


    /// <summary>Исключение без комментариев</summary>
    public M4uOperationCanceledException() : base(DefaultMessage) { }


    /// <summary>Исключение с комментариями</summary>
    /// <param name="comments">Комментарии - это может быть любая техническая информация</param>
    public M4uOperationCanceledException(string comments) : base($"{DefaultMessage} {comments}") { }
}