namespace FrameworkDesign
{
    //表现层
    //限制Controller的功能
    public interface IController : ICanGetIOC,ICanSendCommand,ICanGetModel,ICanGetSystem,ICanRegisterEvent,ICanSendQuery
    {

    }

    //可以通过继承AbstractIController用GetIOC获取IOCContainer对象直接获取容器中的数据
    //public abstract class AbstractIController : IController
    //{

    //    public IIOC GetIOC()
    //    {
    //        return IOCManager.Interface;
    //    }

    //    public void SetIOC(IIOC iOC)
    //    {
    //        //GetIOC().GetModel<>
    //    }

        
    //}
}

