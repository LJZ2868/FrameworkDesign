namespace FrameworkDesign
{

    public interface ICanSendCommand : ICanGetIOC
    {
    }

    public static class CanSendCommandExtension
    {
        public static void SendCommand<T>(this ICanSendCommand self) where T : ICommand, new()
        {
            self.GetIOC().SendCommand<T>();
        }

        public static void SendCommand<T>(this ICanSendCommand self,T command) where T : ICommand
        {
            self.GetIOC().SendCommand<T>(command);
        }

    }

}
