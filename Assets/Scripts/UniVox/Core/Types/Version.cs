using Unity.Entities;

namespace UniVox.Core.Types
{
    public class Version
    {
        private uint _versionId;

        public Version()
        {
            _versionId = ChangeVersionUtility.InitialGlobalSystemVersion;
        }

        public uint VersionId => _versionId;

        public void WriteTo()
        {
            ChangeVersionUtility.IncrementGlobalSystemVersion(ref _versionId);
        }

        public bool DidChange(uint cachedVersion)
        {
            return ChangeVersionUtility.DidChange(cachedVersion, _versionId);
        }

        public void CopyFrom(Version version)
        {
            _versionId = version._versionId;
        }

        public static implicit operator uint(Version version)
        {
            return version._versionId;
        }
    }
}