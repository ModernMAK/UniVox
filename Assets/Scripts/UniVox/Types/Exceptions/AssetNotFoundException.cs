using System;

namespace UniVox.Types.Exceptions
{
    public class AssetNotFoundException : Exception
    {
        public AssetNotFoundException(string assetName) : base($"'{assetName}' was not found!")
        {
        }

        public AssetNotFoundException(string variableName, string assetName) : base(
            $"{variableName} - '{assetName}' was not found!")
        {
        }
    }
}