namespace FrameworkDesign
{

    public interface ICommand : ICanSetIOC,ICanGetIOC,ICanGetModel,ICanGetSystem,ICanGetUtility,ICanSendEvent,ICanSendCommand,ICanSendQuery
    {
        public void Execute();
    }

    public abstract class AbstractICommand : ICommand
    {
        private IIOC iOC;

        void ICommand.Execute()
        {
            OnExecute();
        }

        protected abstract void OnExecute();

        IIOC ICanGetIOC.GetIOC()
        {
            return iOC;
        }

        void ICanSetIOC.SetIOC(IIOC iOC)
        {
            this.iOC = iOC;
        }
    }
}
