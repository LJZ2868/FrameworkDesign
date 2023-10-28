namespace FrameworkDesign
{
    //限制Model的功能
    public interface IModel : ICanSetIOC,ICanGetIOC,ICanGetUtility,ICanSendEvent
    {
        //Model层初始化
        void Init();
    }


    //写一个抽象类 避免子类重复写样本代码
    public abstract class AbstractIModel : IModel
    {
        private IIOC iOC;

        IIOC ICanGetIOC.GetIOC()
        {
            return iOC;
        }

        void IModel.Init()
        {
            OnInit();
        }

        protected abstract void OnInit();

        void ICanSetIOC.SetIOC(IIOC iOC)
        {
            this.iOC = iOC;
        }
    }

}
