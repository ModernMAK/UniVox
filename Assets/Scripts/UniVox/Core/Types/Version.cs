using Unity.Entities;

namespace UniVox.Core
{
    public class Version
    {
        public Version()
        {
            _versionId = ChangeVersionUtility.InitialGlobalSystemVersion;
        }


        private uint _versionId;
        public uint VersionId => _versionId;
        public void WriteTo() => ChangeVersionUtility.IncrementGlobalSystemVersion(ref _versionId);
        public bool DidChange(uint cachedVersion) => ChangeVersionUtility.DidChange(cachedVersion, _versionId);

        public void CopyFrom(Version version)
        {
            _versionId = version._versionId;
        }
        
        public static implicit operator uint(Version version) => version._versionId;
    }
}