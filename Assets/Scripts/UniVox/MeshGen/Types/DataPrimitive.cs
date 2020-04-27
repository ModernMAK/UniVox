namespace UniVox.MeshGen
{
    public struct DataPrimitive<TVertex, TData>
    {
        public DataPrimitive(TData data, TVertex left, TVertex pivot, TVertex right)
        {
            Data = data;
            Left = left;
            Pivot = pivot;
            Right = right;
            Opposite = default;
            Triangle = true;
        }

        public DataPrimitive(TData data, TVertex left, TVertex pivot, TVertex right, TVertex opposite)
        {
            Data = data;
            Left = left;
            Pivot = pivot;
            Right = right;
            Opposite = opposite;
            Triangle = false;
        }

        public TData Data { get; }
        public TVertex Left { get; }
        public TVertex Pivot { get; }
        public TVertex Right { get; }
        public TVertex Opposite { get; }
        public bool Triangle { get; }

        public Primitive<TVertex> AsPrimitive()
        {
            return Triangle
                ? new Primitive<TVertex>(Left, Pivot, Right)
                : new Primitive<TVertex>(Left, Pivot, Right, Opposite);
        }
    }
}