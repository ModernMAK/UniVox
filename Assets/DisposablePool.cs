using System;

public class DisposablePool<T> : Pool<T>, IDisposable where T : IDisposable
{
    public DisposablePool(Func<T> buildFunc = null) : base(buildFunc)
    {
    }

    public void Dispose()
    {
        while (TryAcquire(out var item))
            item.Dispose();
    }
}