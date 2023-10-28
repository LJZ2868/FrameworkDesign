namespace FrameworkDesign
{

    public interface ICanGetSystem : ICanGetIOC
    {
    }

    public static class CanGetSystemExtension
    {
        public static T GetSystem<T>(this ICanGetSystem system) where T : class, ISystem
        {
            return system.GetIOC().GetSystem<T>();
        }
    }
}
