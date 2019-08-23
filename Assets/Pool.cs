using System;
using System.Collections.Generic;

public class Pool<T>
{
    private readonly Func<T> _builder;
    protected readonly Queue<T> _queue;

    public Pool(Func<T> buildFunc = null)
    {
        _queue = new Queue<T>();
        _builder = buildFunc;
    }


    public T Acquire()
    {
        TryAcquire(out var item, true);
        return item;
    }

    public bool TryAcquire(out T item, bool allowCreation = false)
    {
        if (_queue.Count > 0)
        {
            item = _queue.Dequeue();
            return true;
        }
        else if (allowCreation && _builder != null)
        {
            item = _builder();
            return true;
        }

        item = default;
        return false;
    }

    public void Release(T item)
    {
        _queue.Enqueue(item);
    }
}