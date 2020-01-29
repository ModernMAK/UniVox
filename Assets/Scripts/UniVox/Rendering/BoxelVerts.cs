namespace UniVox.Rendering
{
    public enum BoxelVerts : byte
    {
        FUL = (1 << 0),
        FUR = (1 << 1),
        FDL = (1 << 2),
        FDR = (1 << 3),
        BUL = (1 << 4),
        BUR = (1 << 5),
        BDL = (1 << 6),
        BDR = (1 << 7),


        Forward = FUL | FUR | FDL | FDR,
        Backward = BUL | BUR | BDL | BDR,

        Up = FUL | FUR | BUL | BUR,
        Down = FDL | FDR | BDL | BDR,

        Left = FUL | FDL | BUL | BDL,
        Right = FUR | FDR | BUR | BDR,

        None = 0,
        All = Forward | Backward,
    }
}