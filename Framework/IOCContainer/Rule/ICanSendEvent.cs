namespace FrameworkDesign
{
    public interface ICanSendEvent : ICanSetIOC, ICanGetIOC
    {
    }

    public static class CanSendEventExtension
    {
        public static void SendEvent<T>(this ICanSendEvent self) where T : new()
        {
            self.GetIOC().SendEvent<T>();
        }

        public static void SendEvent<T>(this ICanSendEvent self,T e)
        {
            self.GetIOC().SendEvent<T>(e);
        }
    }

}

