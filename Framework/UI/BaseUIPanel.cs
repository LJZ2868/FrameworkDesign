using UnityEngine;
namespace FrameworkDesign
{
    //挂载游戏UI对象 派生多种面板
    public class BaseUIPanel : MonoBehaviour
    {
        //获取面板关闭按钮
        private Transform closeBtn;
        public Transform CloseBtn 
        {
            get 
            {
                if (closeBtn == null)
                    closeBtn = transform.Find("CloseBtn");
                return closeBtn;
            }
        }

        private void Awake()
        {
            //面板关闭按钮添加监听
            //CloseBtn.GetComponent<Button>()?.onClick.AddListener(Hide);
            //Debug.Log(CloseBtn.name);
        }

        //显示UIPanel
        public virtual void Show()
        {
            gameObject.SetActive(true);
            //transform.localPosition = Vector3.zero;
            //transform.localScale = new Vector3(1, 1, 1);
        }

        //隐藏UIPanel
        public virtual void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}