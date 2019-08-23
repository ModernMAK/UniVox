namespace Rendering
{
    public static class IPipelineJobX
    {
        public static void Complete(this IPipelineHandle pj)
        {
            pj.Handle.Complete();
        }

        public static bool IsCompleted(this IPipelineHandle pj)
        {
            return pj.Handle.IsCompleted;
        }

        public static void CompleteAndDispose(this IPipelineHandle pj)
        {
            pj.Complete();
            pj.Dispose();
        }
    }
}