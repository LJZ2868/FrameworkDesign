namespace FrameworkDesign
{
    public interface IUtility : ICanSetIOC, ICanGetIOC
    {
        //Utility≤„≥ı ºªØ
        void Init();
    }

    public abstract class AbstractIUtility : IUtility
    {
        private IIOC iOC; 

        IIOC ICanGetIOC.GetIOC()
        {
            return iOC;
        }

        void IUtility.Init()
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
