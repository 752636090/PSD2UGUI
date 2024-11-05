using UnityEngine;

namespace PSD2UGUI
{
    public class PSD2UICustomHandler
    {
        /// <summary>
        /// 自动设置Layout.spacing时需要知道Item的大小，
        /// 如果Item不在group参数物体的子孙里面的话，这里可以通过自己的循环列表找到Item预设
        /// </summary>
        /// <param name="group">psd中图层组对应的ui物体，不是手动添加的Content</param>
        /// <returns></returns>
        public virtual RectTransform GetItemPrefab(Transform group)
        {
            return default;
        }
    }
}
