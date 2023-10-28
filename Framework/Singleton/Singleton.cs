using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FrameworkDesign
{

    /// <summary>
    /// 一个普通的单例
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class Singleton<T> where T : new()
    {
        /// <summary>
        /// 单例自身
        /// </summary>
        private static T instance;

        /// <summary>
        /// 线程锁
        /// </summary>
        private static readonly object locked = typeof(T);

        public static T Instance
        {
            get
            {
                if (instance == null)
                    lock (locked)
                        instance ??= new T();
                return instance;
            }
        }
    }


    /// <summary>
    /// 继承Mono的一个单例
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
    {
        private static T instance;

        private static readonly object locked = typeof(T);

        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (locked)
                    {
                        instance ??= FindObjectOfType<T>();

                        instance ??= new GameObject(typeof(T).Name).AddComponent<T>();
                    }
                }
                return instance;
            }
        }

        protected virtual void Awake()
        {
            if (instance == null)
            {
                instance = (T)this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }

        }

    }
}
