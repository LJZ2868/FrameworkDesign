using System;
using System.Collections.Generic;
using UnityEngine;

namespace FrameworkDesign
{

    public interface ITypeEventSystem 
    {
        void Send<T>() where T : new();
        void Send<T>(T e);

        IUnRegister Register<T>(Action<T> action);
        void UnRegister<T>(Action<T> action);
    }

    //注册时返回的对象的类型 用于注销
    public interface IUnRegister
    {
        void UnRegister();
    }
    public class TypeEventSystemUnRegister<T> : IUnRegister
    {
        public ITypeEventSystem typeEventSystem;
        public Action<T> action;

        public void UnRegister()
        {
            typeEventSystem.UnRegister<T>(action);

            typeEventSystem = null;
            action = null;
        }
    }


    //注销事件的触发器(挂载时 对象被销毁会自动注销注册的事件) 
    public class UnRegisterOnDestroyTrigger : MonoBehaviour
    {
        HashSet<IUnRegister> unRegisters = new HashSet<IUnRegister>();

        public void AddUnRegister(IUnRegister unRegister)
        {
            unRegisters.Add(unRegister);
        }

        private void OnDestroy()
        {
            foreach (var item in unRegisters)
            {
                item.UnRegister();
            }

            unRegisters.Clear();
        }
    }

    public static class UnRegisterExtension
    {
        public static void UnRegisterWhenGameObjectOnDestroy(this IUnRegister unRegister,GameObject gameObject)
        {
            //获取是否挂载触发器脚本
            var trigger = gameObject.GetComponent<UnRegisterOnDestroyTrigger>();
            //没有就加上
            if (!trigger)
            {
                trigger = gameObject.AddComponent<UnRegisterOnDestroyTrigger>();
            }

            trigger.AddUnRegister(unRegister);
        }
    }


    public class TypeEventSystem : MonoBehaviour,ITypeEventSystem
    {
        public interface IRegistration
        {
        }

        public class Registration<T> : IRegistration
        {
            public Action<T> action = e => { };
        }

        //存贮注册的事件
        Dictionary<Type, IRegistration> mResgistration = new Dictionary<Type, IRegistration>();

        public void Send<T>() where T : new()
        {
            T e = new T();
            Send<T>(e);
        }

        public void Send<T>(T e)
        {
            Type t = typeof(T);
            IRegistration registration;

            //如果事件已经注册 registration就直接获取并发送
            if (mResgistration.TryGetValue(t, out registration))
            {
                (registration as Registration<T>).action(e);
            }
        }

        public IUnRegister Register<T>(Action<T> action)
        {
            Type t = typeof(T);
            IRegistration registration;

            //如果事件已经注册 registration就直接获取
            if (mResgistration.TryGetValue(t, out registration))
            {
            }
            else
            {
                //没注册就新建一个并添加到字典中
                registration = new Registration<T>();
                mResgistration.Add(t, registration);
            }

            //添加事件
            (registration as Registration<T>).action += action;

            return new TypeEventSystemUnRegister<T>()
            {
                typeEventSystem = this,
                action = action 
            };


        }

        public void UnRegister<T>(Action<T> action)
        {
            Type t = typeof(T);
            IRegistration registration;

            //如果事件已经注册 registration就直接获取并注销
            if (mResgistration.TryGetValue(t, out registration))
            {
                (registration as Registration<T>).action -= action;
            }
        }
    }
}
