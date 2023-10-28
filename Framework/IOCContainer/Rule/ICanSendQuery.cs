namespace FrameworkDesign
{

    public interface ICanSendQuery : ICanGetIOC
    {

    }

    public static class CanSendQueryExtension
    {
        public static Tresult SendQuery<Tresult>(this ICanSendQuery self, IQuery<Tresult> query)
        {
            return self.GetIOC().SendQuery(query);
        }
    }
}
