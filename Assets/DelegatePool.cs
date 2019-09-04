using System;
using System.Collections.Generic;

public class DelegatePool<T> : Pool<T>
{
    private readonly Func<T> _builder;
    private readonly Action<T[]> _builderMany;

    public DelegatePool(Func<T> buildFunc = null, Action<T[]> builderMany = null)
    {
        _builder = buildFunc;
        _builderMany = builderMany;
    }

    protected override bool TryAcquireNew(out T value)
    {
        if (_builder != null)
        {
            value = _builder();
            return true;
        }

        value = default;
        return false;
    }

    protected override bool TryAcquireManyNew(T[] value)
    {
        if (_builderMany != null)
        {
            _builderMany(value);
            return true;
        }

        return false;
    }
}

public abstract class Pool<T>
{
    protected readonly Queue<T> _queue;

    public Pool()
    {
        _queue = new Queue<T>();
    }

    public int Count => _queue.Count;

    public T Acquire()
    {
        TryAcquire(out var item, true);
        return item;
    }

    public T[] Acquire(int count)
    {
        T[] acquired = new T[count];
        Acquire(acquired);
        return acquired;
    }

    public void Acquire(T[] items)
    {
    }


    public bool TryAcquire(out T item, bool allowCreation = false)
    {
        if (_queue.Count > 0)
        {
            item = _queue.Dequeue();
            return true;
        }

        if (allowCreation)
        {
            return TryAcquireNew(out item);
        }

        item = default;
        return false;
    }

    public int TryAcquire(T[] items, bool allowCreation = false)
    {
        for (var i = 0; i < items.Length; i++)
        {
            if (_queue.Count > 0)
            {
                items[i] = _queue.Dequeue();
            }
            else if (allowCreation)
            {
                var temp = new T[items.Length - i];
                if (TryAcquireManyNew(temp))
                {
                    for (var j = 0; j < items.Length - i; j++)
                    {
                        items[i + j] = temp[j];
                    }

                    return items.Length;
                }

                return i;
            }
            else
                return i;
        }

        return items.Length;
    }


    public void Release(T item)
    {
        _queue.Enqueue(item);
    }

    protected abstract bool TryAcquireNew(out T value);
    protected abstract bool TryAcquireManyNew(T[] value);
}