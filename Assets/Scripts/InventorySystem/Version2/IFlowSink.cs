namespace InventorySystem.Version2
{
    
    /// <summary>
    /// Represents a 'Sink' which knows it's current demand, and allows demand to be 'Filled'
    /// </summary>
    public interface IFlowSink
    {
        /// <summary>
        /// The current demand of the sink. Expected to be non-negative.
        /// </summary>
        int Demand { get; }
        
        /// <summary>
        /// Decreases the sink's demand. Will return excess supply.
        /// </summary>
        /// <param name="supply">The amount of demand to decrease.</param>
        /// <returns>The excess supply.</returns>
        int FillDemand(int supply);
    }
}