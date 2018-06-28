using System;
using Random = UnityEngine.Random;

public class SafeRandom : IDisposable
{
    public SafeRandom()
    {
        _state = Random.state;
    }

    public SafeRandom(int seed)
    {
        _state = Random.state;
        Random.InitState(seed);
    }

    private readonly Random.State _state;

    public void Dispose()
    {
        Random.state = _state;
    }
}