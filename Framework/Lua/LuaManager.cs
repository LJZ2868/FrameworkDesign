using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using XLua;

namespace FrameworkDesign
{

    public class LuaManager : MonoSingleton<LuaManager>
    {
        LuaEnv luaEnv;

        public override void Awake()
        {
            luaEnv = new LuaEnv();
            luaEnv.AddLoader(MyLoader);
            //Lua代码入口
            luaEnv.DoString("require 'MainLua'");
            base.Awake();
        }

        private void Start()
        {

        }

        private byte[] MyLoader(ref string fileName)
        {
#if UNITY_STANDALONE_WIN
            string targetPlatform = "/PC/";
#elif UNITY_UNITY_ANDROID
        string targetPlatform = "/Android/";
#elif UNITY_IOS
        string targetPlatform = "/IOS/";
#endif
            var filePath = Application.streamingAssetsPath + targetPlatform + fileName + ".lua";
            return File.ReadAllBytes(filePath);
        }

        private void OnDisable()
        {
            //lua中热更新的代码注销
            luaEnv.DoString("require 'LuaDispose'");
        }

        private void OnDestroy()
        {
            luaEnv.Dispose();
        }
    }
}
