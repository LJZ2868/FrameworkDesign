using System;
using System.Collections.Generic;
namespace FrameworkDesign
{
    //IOC容器  给数据访问添加限制
    public abstract class IOCContainer<T> : IIOC where T:IOCContainer<T>,new()
    {
        private static Dictionary<Type, object> mInstances = new Dictionary<Type, object>();

        private static T mContainer;

        //事件系统
        private ITypeEventSystem mTypeEventSystem = new TypeEventSystem();

        //是否完成初始化
        private bool mInited = false;

        //初始化Model缓存
        private List<IModel> mModels = new List<IModel>();

        //初始化System缓存
        private List<ISystem> mSystems = new List<ISystem>();

        //初始化
        protected abstract void Init();

        public static IIOC Interface
        {
            get
            {
                if (mContainer == null)
                {
                    MakeSureIOC();
                }
                return mContainer;
            }
        }


        //确保容器里有数据(对象)
        static void MakeSureIOC()
        {
            if (mContainer == null)
            {
                mContainer = new T();

                //容器初始化
                mContainer.Init();


                //Model里的数据初始化
                foreach (var item in mContainer.mModels)
                {
                    item.Init();
                }

                //清除缓存
                mContainer.mModels.Clear();
                
                //System初始化(System层为底层初始化时可能会调用Model层的数据所以在Model初始化后)
                foreach (var item in mContainer.mSystems)
                {
                    item.Init();
                }
                mContainer.mSystems.Clear();

                mContainer.mInited = true;
            }
        }



        //注册实列(模块),同一接口的实现会被替换
        private  void Register<T>(T instance)
        {
            MakeSureIOC();

            Type key = typeof(T);

            if (!mInstances.ContainsKey(key))
                mInstances.Add(key, instance);
            else
                mInstances[key] = instance;
        }
        //实例化实例(模块)
        private T Get<T>() where T:class
        {
            MakeSureIOC();

            Type key = typeof(T);

            if (mInstances.ContainsKey(key))
                return mInstances[typeof(T)] as T;

            return null;
        }

        public T GetUtility<T>() where T : class,IUtility
        {
            return Get<T>();
        }

        public void RegisterModel<T>(T model) where T : IModel
        {
            //IOC可以在Model层注册前通过访问IOCContainer获取其他层 避免其他层在初始化前访问Model层 避免重复初始化
            model.SetIOC(this);
            mContainer.Register<T>(model);
            
            if (!mInited)
            {
                //添加到Model缓存中 用于初始化
                mContainer.mModels.Add(model);
            }
            //初始化过了(防止重复初始化)
            else
            {
                model.Init();
            }
        }

        public void RegisterUtility<T>(T utility) where T : IUtility
        {
            mContainer.Register<T>(utility);
        }

        public T GetModel<T>() where T : class,IModel
        {
            return Get<T>();
        }

        public void RegisterSystem<T>(T system) where T : ISystem
        {
            system.SetIOC(this);
            mContainer.Register<T>(system);

            if (!mInited)
            {
                mContainer.mSystems.Add(system);
            }
            else
            {
                system.Init();
            }
        }

        public void SendCommand<T>() where T : ICommand, new()
        {
            var command = new T();
            command.SetIOC(this);
            command.Execute();
        }

        public void SendCommand<T>(T command) where T : ICommand
        {
            command.SetIOC(this);
            command.Execute();
        }

        public T GetSystem<T>() where T : class, ISystem
        {
            return Get<T>();
        }

        public void SendEvent<T>() where T : new()
        {
            mTypeEventSystem.Send<T>();
        }

        public void SendEvent<T>(T e)
        {
            mTypeEventSystem.Send<T>(e);
        }

        public IUnRegister RegisterEvent<T>(Action<T> action)
        {
           return mTypeEventSystem.Register<T>(action);
        }

        public void UnRegisterEvent<T>(Action<T> action)
        {
            mTypeEventSystem.UnRegister<T>(action);
        }

        public TResult SendQuery<TResult>(IQuery<TResult> query)
        {
            query.SetIOC(this);
            return query.Do();
        }
    }
}
