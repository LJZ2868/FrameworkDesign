using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace FrameworkDesign
{
    public interface ITimeSystem : ISystem
    {
        //持续记录当前的时间
        float CurrentSeconds { get; }
        //延时后的委托
        void AddDelayTask(float seconds, Action onDelayFinish);
    }

    public class TimeSyetem : AbstractISystem, ITimeSystem
    {

        public class TimeSyetemUpdate : MonoBehaviour
        {
            public Action OnUpdate;

            private void Update()
            {
                OnUpdate?.Invoke();
            }
        }

        public float CurrentSeconds { get; private set; }

        //存储计时及任务的链表
        private LinkedList<DelayTask> mDelayTasks = new LinkedList<DelayTask>();

        //初始化
        public void AddDelayTask(float seconds, Action onDelayFinish)
        {
            var mDelayTask = new DelayTask
            {
                Seconds = seconds,
                OnFinsh = onDelayFinish,
                DelayTaskState = DelayTaskState.NoStart
            };

            //加入任务列表
            mDelayTasks.AddLast(mDelayTask);
        }

        protected override void OnInit()
        {
            //创建一个GameObject记录时间
            var mTimeSyetemUpdate = new GameObject(nameof(TimeSyetemUpdate));
            var updateBehaviour = mTimeSyetemUpdate.AddComponent<TimeSyetemUpdate>();
            UnityEngine.Object.DontDestroyOnLoad(mTimeSyetemUpdate);

            updateBehaviour.OnUpdate += OnUpdate;
        }

        void OnUpdate()
        {
            CurrentSeconds += Time.deltaTime;

            if (mDelayTasks.Count > 0)
            {
                //有延时任务
                var currentNode = mDelayTasks.First;
                while (currentNode != null)
                {
                    //记录下个节点
                    var nextNode = currentNode.Next;
                    var delayTask = currentNode.Value;


                    if (delayTask.DelayTaskState == DelayTaskState.NoStart)
                    {
                        delayTask.DelayTaskState = DelayTaskState.Started;
                        //记录开始时间
                        delayTask.StartSeconds = CurrentSeconds;
                        //设置完成时间
                        delayTask.FinishSeconds = CurrentSeconds + delayTask.Seconds;
                    }
                    else if (delayTask.DelayTaskState == DelayTaskState.Started)
                    {
                        //延时完成后
                        if (delayTask.FinishSeconds <= CurrentSeconds)
                        {
                            delayTask.DelayTaskState = DelayTaskState.Finish;
                            //执行延时任务
                            delayTask.OnFinsh?.Invoke();
                            //滞空
                            delayTask.OnFinsh = null;
                            //删除节点
                            mDelayTasks.Remove(currentNode);
                        }
                        
                    }
                    //遍历下个节点
                    currentNode = nextNode;
                }
            }
        }
    }

    public class DelayTask
    {
        //延时时间
        public float Seconds { get; set; }
        //延时任务
        public Action OnFinsh { get; set; }

        //记录开始时间
        public float StartSeconds { get; set; }
        //记录完成时间
        public float FinishSeconds { get; set; }
        //状态
        public DelayTaskState DelayTaskState { get; set; }
    }

    public enum DelayTaskState
    {
        NoStart,
        Started,
        Finish
    }
}