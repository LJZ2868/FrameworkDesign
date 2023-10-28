using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FrameworkDesign
{
    public class SceneMgr : MonoBehaviour
    {

        IEnumerator AsyncLoadScene(string resName)
        {
            var operation = SceneManager.LoadSceneAsync(resName);
            operation.allowSceneActivation = false;
            while (!operation.isDone)
            {
                yield return null;
            }
            operation.allowSceneActivation = true;

        }
    }
}