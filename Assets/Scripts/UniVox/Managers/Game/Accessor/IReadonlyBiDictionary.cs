using System.Collections.Generic;

namespace UniVox.Managers.Game.Accessor
{
    public interface IReadonlyBiDictionary<TForward, TBackward>
    {
        IReadOnlyDictionary<TForward, TBackward> Forward { get; }
        IReadOnlyDictionary<TForward, TBackward> Backward { get; }
    }
}