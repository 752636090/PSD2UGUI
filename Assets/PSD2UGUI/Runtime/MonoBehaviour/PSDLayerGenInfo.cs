using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace PSD2UGUI
{
    [DisallowMultipleComponent]
    public class PSDLayerGenInfo : MonoBehaviour
    {
        #region UNITY_EDITOR
        public string SrcPath;
        public Vector2 SrcPos;
        public Vector2 SrcSize;
        [BoxGroup("LayoutGroup", VisibleIf = "IsLayoutGroupInPsd")]
        public RectOffsetStruct LayoutPadding;
        [BoxGroup("LayoutGroup", VisibleIf = "IsLayoutGroupInPsd")]
        public Vector2 LayoutSpacing;
        [BoxGroup("LayoutGroup", VisibleIf = "IsLayoutGroupInPsd")]
        public Vector2 LayoutItemSize;
        [BoxGroup("LayoutGroup", VisibleIf = "IsLayoutGroupInPsd")]
        [Button("UpdateLayoutGroup")]
        [Tooltip("支持手动新增子物体(Content)作为所有Item的父物体")]
        private void Button_UpdateLayout() => UpdateLayout();

        public Rect SrcRect => new(SrcPos - SrcSize / 2, SrcSize);
        public string SrcName => string.IsNullOrEmpty(SrcPath) ? GetComponent<PSDGenInfos>().SrcFileName : Path.GetFileName(SrcPath).Split('[')[0];
        public bool IsLayoutGroupInPsd => LayoutSpacing != default;

        public void UpdateLayout()
        {
            if (!transform.TryGetComponent(out LayoutGroup layoutGroup))
            {
                LayoutGroup _layoutGroup = transform.GetComponentInChildren<LayoutGroup>();
                if (_layoutGroup != null && !_layoutGroup.transform.TryGetComponent(out PSDLayerGenInfo _))
                {
                    layoutGroup = _layoutGroup;
                }
            }

            if (layoutGroup == null)
            {
                Debug.LogError("找不到LayoutGroup");
                return;
            }

            if (layoutGroup.transform.childCount == 0 || !layoutGroup.transform.GetChild(0).TryGetComponent(out PSDLayerGenInfo itemGenInfo))
            {
                Debug.LogError("找不到Item");
                return;
            }

            UnityEditor.Undo.RecordObject(gameObject, "PSDLayerGenInfo.UpdateLayout");
            layoutGroup.padding.left = LayoutPadding.left;
            layoutGroup.padding.right = LayoutPadding.right;
            layoutGroup.padding.top = LayoutPadding.top;
            layoutGroup.padding.bottom = LayoutPadding.bottom;

            Vector2 itemSize = itemGenInfo.transform.TryGetComponent(out LayoutElement layoutElement) && (layoutElement.preferredWidth > 0 || layoutElement.preferredHeight > 0)
                ? new Vector2(layoutElement.preferredWidth, layoutElement.preferredHeight)
                : LayoutItemSize;
            Vector2 spacingInInspector = LayoutSpacing - itemSize;
            if (layoutGroup is HorizontalLayoutGroup horizontalLayoutGroup)
            {
                horizontalLayoutGroup.spacing = spacingInInspector.x;
            }
            else if (layoutGroup is VerticalLayoutGroup verticalLayoutGroup)
            {
                verticalLayoutGroup.spacing = spacingInInspector.y;
            }
            else if (layoutGroup is GridLayoutGroup gridLayoutGroup)
            {
                gridLayoutGroup.spacing = spacingInInspector;
                gridLayoutGroup.cellSize = itemSize;
            }
        }
        #endregion
    }
}
