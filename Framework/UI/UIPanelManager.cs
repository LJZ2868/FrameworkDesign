using System.Collections.Generic;
using System.Xml;
using UnityEngine;

namespace FrameworkDesign
{
    public class UIPanelManager 
    {
        private static Transform uiRoot;
        public static Transform UIRoot
        {
            get
            {
                if (uiRoot == null)
                {
                    //uiRoot = GameObject.Instantiate(Resources.Load<Transform>("UI/Canvas"));
                    //标签查找
                    uiRoot = GameObject.FindGameObjectWithTag("UIRoot")?.transform;
                    if (uiRoot == null)
                    {
                        //加载Resources文件夹下的UI资源
                        uiRoot = GameObject.Instantiate(Resources.Load<Transform>("UI/Canvas"));

                    }
                    Object.DontDestroyOnLoad(uiRoot);
                    //把EventSystem放在Canvas下就可以不用写下面
                    //Object.DontDestroyOnLoad(GameObject.Find("EventSystem"));
                }
                return uiRoot;
            }
        }

        private static UIPanelManager instance;
        public static UIPanelManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new UIPanelManager();
                }
                return instance;
            }
        }

        UIPanelManager()
        {
            Init(APPConst.uiPrefabXML);
        }

        //面板对象缓存
        Dictionary<string, BaseUIPanel> uiPanels = new Dictionary<string, BaseUIPanel>();

        //面板对象资源
        Dictionary<string, BaseUIPanel> uiPanelDic = new Dictionary<string, BaseUIPanel>();

        //初始化 加载xml文件  获取对象资源
        private void Init(string pathName)
        {
            //读取根标签
            XmlDocument doc = new XmlDocument();

            //doc.LoadXml();
            doc.Load(pathName);

            //获取XML文件的节点 
            var root = doc.SelectSingleNode("UIRoot") as XmlElement;

            //遍历根标签 加入资源
            foreach (XmlElement item in root.ChildNodes)
            {
                string name = item.GetAttribute("name");
                string uiPlanelPath = item.GetAttribute("uiPanelPath");
                //Debug.Log(uiPlanelPath);

                var uiPanel = Resources.Load<BaseUIPanel>(uiPlanelPath);

                //Object.Instantiate(uiPanel);

                if (!uiPanelDic.ContainsKey(name))
                {
                    uiPanelDic.Add(name,uiPanel); 
                }
            }
        }

        public void ShowUIPanel(string uiPanelName)
        {
            //如果在缓存中则直接激活
            if (uiPanels.ContainsKey(uiPanelName))
            {
                uiPanels[uiPanelName].Show();
                return;
            }

            //如果不在缓存中则判断是否存在
            if (uiPanelDic.ContainsKey(uiPanelName))
            {
                //Debug.Log("123");
                //实例化对象
                var uiPanel = GameObject.Instantiate(uiPanelDic[uiPanelName],UIRoot);
                //设置父对象(位置)
                //uiPanel.transform.SetParent(UIRoot);
                //设置起始位置
                uiPanel.Show();
                //存入缓存中
                uiPanels.Add(uiPanelName, uiPanel);
                //uiPanels[uiPanelName].Show();
            }
        }

        public void HideUIPanel(string uiPanelName)
        {
            //如果在缓存中则直接隐藏
            if (uiPanels.ContainsKey(uiPanelName))
            {
                uiPanels[uiPanelName].Hide();
            }
        }



    }
}