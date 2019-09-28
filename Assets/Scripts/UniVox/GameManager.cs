using System;
using UniVox.Core.Types;
using UniVox.Entities.Systems;
using UniVox.Entities.Systems.Registry;

namespace UnityEdits
{
    public static class GameManager
    {
        public static readonly GameRegistry Registry = new GameRegistry();
        public static readonly Universe Universe = new Universe();
    }
}