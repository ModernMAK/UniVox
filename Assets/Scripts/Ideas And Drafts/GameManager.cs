using System;
using UniVox.Core.Types;
using UniVox.Entities.Systems;
using UniVox.Entities.Systems.Registry;

namespace UnityEdits
{
    public static class GameManager
    {
        [Obsolete]
        public static readonly MasterRegistry MasterRegistry = new MasterRegistry();
        public static readonly ModRegistry Registry = new ModRegistry();
        public static readonly Universe Universe = new Universe();
    }
}