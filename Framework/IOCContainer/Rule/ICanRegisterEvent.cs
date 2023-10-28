using System;
namespace FrameworkDesign
{

    public interface ICanRegisterEvent : ICanGetIOC
    {
    }

    public static class CanRegisterEventExtension
    {
        public static IUnRegister RegisterEvent<T>(this ICanRegisterEvent self, Action<T> action)
        {
            return self.GetIOC().RegisterEvent<T>(action);
        }

        public static void UnRegisterEvent<T>(this ICanRegisterEvent self, Action<T> action)
        {
            self.GetIOC().UnRegisterEvent<T>(action);
        }

    }
}
