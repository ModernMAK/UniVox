namespace UniVox.Rendering
{
    public struct Primitive<TVertex>
    {
        public Primitive(TVertex left, TVertex pivot, TVertex right)
        {
            Left = left;
            Pivot = pivot;
            Right = right;
            Opposite = default;
            IsTriangle = true;
        }

        public Primitive(TVertex left, TVertex pivot, TVertex right, TVertex opposite)
        {
            Left = left;
            Pivot = pivot;
            Right = right;
            Opposite = opposite;
            IsTriangle = false;
        }
        public Primitive(TVertex left, TVertex pivot, TVertex right, TVertex opposite, bool isTriangle)
        {
            Left = left;
            Pivot = pivot;
            Right = right;
            Opposite = opposite;
            IsTriangle = isTriangle;
        }

        public TVertex Left { get; }
        public TVertex Pivot { get; }
        public TVertex Right { get; }
        public TVertex Opposite { get; }
        public bool IsTriangle { get; }


        public DataPrimitive<TVertex, TData> AsDataPrimitive<TData>(TData data)
        {
            return IsTriangle
                ? new DataPrimitive<TVertex, TData>(data, Left, Pivot, Right)
                : new DataPrimitive<TVertex, TData>(data, Left, Pivot, Right, Opposite);
        }
    }
}