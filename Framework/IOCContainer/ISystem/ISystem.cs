namespace FrameworkDesign
{
    //限制System的功能
    public interface ISystem : ICanSetIOC,ICanGetIOC,ICanGetModel,ICanGetUtility,ICanSendEvent,ICanRegisterEvent,ICanGetSystem
    {
        //System层初始化
        void Init();
    }

    public abstract class AbstractISystem : ISystem
    {
        private IIOC iOC;

        IIOC ICanGetIOC.GetIOC()
        {
            return iOC;
        }

        void ISystem.Init()
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
