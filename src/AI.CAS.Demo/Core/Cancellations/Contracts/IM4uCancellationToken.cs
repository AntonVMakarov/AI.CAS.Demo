namespace AI.CAS.Core.Cancellations;

/// <summary>Мой токен отмены</summary>
public abstract class IM4uCancellationToken
{
    /// <summary>Логическая переменная, равная true, если необходимо отменить операцию и false - в противном случае</summary>
    public abstract bool IsCancellationRequested { get; }


    /// <summary>Бросает исключение если необходимо отменить операцию</summary>
    /// <exception cref="OperationCanceledCasException">Исключение, обозначающее, что операция 
    /// отменена по причине того, что время, отведённое на эту операцию, закончилось</exception>
    public void ThrowIfCancellationRequested()
    {
        if (IsCancellationRequested) throw new M4uOperationCanceledException();
    }
}