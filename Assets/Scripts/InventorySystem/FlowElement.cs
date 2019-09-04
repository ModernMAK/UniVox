using System;

namespace InventorySystem
{
    public class FlowElement
    {
        public Guid Contents { get; set; }
        public int Count { get; set; }
        public int Capacity { get; set; }
    }
}