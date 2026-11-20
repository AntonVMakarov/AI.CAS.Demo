namespace AI.CAS.Core.Cancellations;

/// <summary>Конкретная реализация абстрактного класса для описания исключения, возникающего 
/// когда операция (вычисления/отрисовки) была отменена из-за того, что она превысила время, отведённое на неё</summary>
public class M4uOperationCanceledException : Exception
{
    /// <summary>Уникальный идентификатор исключения</summary>
    private readonly int Id = Convert.ToInt32(M4uExceptions.OperationTimeout);


    /// <summary>Описание исключения</summary>
    private const string message = "Операция была отменена по причине того, что время, отведённое на неё, закончилось ...";


    /// <summary>Исключение без комментариев</summary>
    public M4uOperationCanceledException() : base(message) { this.HResult = Id; }


    /// <summary>Исключение с комментариями</summary>
    /// <param name="comments">Комментарии - это может быть любая техническая информация</param>
    public M4uOperationCanceledException(string comments) : base(message) { this.Data["comments"] = comments; this.HResult = Id; }
}