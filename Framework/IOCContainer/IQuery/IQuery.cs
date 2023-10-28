
using UnityEngine;

namespace FrameworkDesign
{

    public interface IQuery<TResult> : ICanGetIOC, ICanSetIOC,ICanGetModel,ICanSendQuery
    {
        TResult Do();
    }

    public abstract class AbstractQuery<T> : IQuery<T>
    {
        private IIOC iOC;

        T IQuery<T>.Do()
        {
            return OnDo();
        }

        protected abstract T OnDo();

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
