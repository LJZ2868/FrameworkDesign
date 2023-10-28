namespace FrameworkDesign
{

    public interface ICanGetUtility : ICanSetIOC,ICanGetIOC
    {
    }

    public static class CanGetUtilityExtension
    {
        public static T GetUtility<T>(this ICanGetUtility utility) where T : class, IUtility
        {
            return utility.GetIOC().GetUtility<T>();
        }
    }
}
