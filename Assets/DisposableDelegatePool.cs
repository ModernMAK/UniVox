using System;

public class DisposableDelegatePool<T> : DelegatePool<T>, IDisposable where T : IDisposable
{
    public DisposableDelegatePool(Func<T> buildFunc = null) : base(buildFunc)
    {
    }

    public void Dispose()
    {
        while (TryAcquire(out var item))
            item.Dispose();
    }
}