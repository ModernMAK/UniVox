using Unity.Jobs;

public abstract class AbstractGenerator<TKey, TValue>
{
    public abstract JobHandle Generate(TKey key, TValue value, JobHandle depends);
    public JobHandle Generate(TKey key, TValue value) => Generate(key, value, new JobHandle());
}