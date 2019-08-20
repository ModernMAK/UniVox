namespace Types
{
    /// <summary>
    ///     Specifies which of the 4 Atlases and 8 Meshes are to be used
    /// </summary>
    public enum RenderModeFlags : byte
    {
        Default = 0,

        //Searates Transparent from Opaque
        //These will Use a Separate Atlas and Mesh
        Transparent = 1 << 0,

        //Separates Emissive from Non-Emissive
        //These will Use a Separate Atlas and Mesh
        Emissive = 1 << 1,

        //Separates Non-Solids from Solids
        //These will Use a Separate Mesh
        NonSolid = 1 << 2
    }
}