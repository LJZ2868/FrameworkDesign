using System;
namespace FrameworkDesign
{

    public interface IIOC
    {
        T GetUtility<T>() where T : class, IUtility;
        T GetModel<T>() where T : class, IModel;
        T GetSystem<T>() where T : class, ISystem;

        void RegisterModel<T>(T model) where T : IModel;
        void RegisterSystem<T>(T system) where T : ISystem;
        void RegisterUtility<T>(T utility) where T : IUtility;

        void SendCommand<T>() where T : ICommand, new();
        void SendCommand<T>(T command) where T : ICommand;

        TResult SendQuery<TResult>(IQuery<TResult> query);

        void SendEvent<T>() where T : new();
        void SendEvent<T>(T e);
        IUnRegister RegisterEvent<T>(Action<T> action);
        void UnRegisterEvent<T>(Action<T> action);
    }
            
}
