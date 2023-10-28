namespace FrameworkDesign
{

    public interface ICanGetModel : ICanGetIOC
    {
    }

    public static class CanGetModelExtension
    {
        public static T GetModel<T>(this ICanGetModel model) where T : class, IModel
        {
            return model.GetIOC().GetModel<T>();
        }
    }
}
