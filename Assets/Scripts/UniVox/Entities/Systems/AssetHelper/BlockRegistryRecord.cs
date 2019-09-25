using System;
using InventorySystem;

namespace UniVox.Entities.Systems.Registry
{
    public class BlockRegistryRecord
    {
        //TODO
        //TO IMPLIMENT
        public BlockRegistryRecord(BaseBlockReference blockRef)
        {
            BlockReference = blockRef;
        }

        public BaseBlockReference BlockReference { get; }
    }
}