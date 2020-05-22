using UniVox.Types;

namespace UniVox.Unity
{
    /// <summary>
    /// Useful utility for getting everything surrounding a block/chunk/etc
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DirectionalNeighborhood<T>
    {
        public DirectionalNeighborhood()
        {
            Neighbors = new T[6];
        }

        public T Center { get; set; }
        public T[] Neighbors { get; }

        public T GetNeighbor(Direction direction) => Neighbors[(int) direction];
        public void SetNeighbor(Direction direction, T value) => Neighbors[(int) direction] = value;
    }
}