using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor.Internal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace PSD2UGUI
{
    //[CreateAssetMenu(fileName = "PSD2UISetting", menuName = "ScriptableObject/Temp/PSD2UISetting")]
    [HideMonoScript]
    public class PSD2UISetting : SerializedScriptableObject
    {
        public SceneAsset UIScene;
        [Tooltip("关闭编辑界面时自动跳转，如果为空则会跳转回之前的场景")]
        public SceneAsset InitScene;

        [Tooltip("UI编辑场景的UI父物体路径")]
        public string UIRoot;

        [FolderPath(RequireExistingPath = true)]
        [LabelText("Prefab一级目录")]
        public string UIPrefabFolder;

        [TableList]
        public List<UIAssetTypeSetting> AssetTypeSettings = new();

        [FolderPath(RequireExistingPath = true)]
        [LabelText("图片导出目录")]
        public string UIImageFolder;

        [LabelText("手动管理的图片目录")]
        public List<string> ImageManualFolders = new();

        [TypeDrawerSettings(BaseType = typeof(Image))]
        public Type ImageType;

        [TypeDrawerSettings(BaseType = typeof(RawImage))]
        public Type RawImageType;

        [Tooltip("新增图片如果不是Slice类型，只要分辨率面积大小达到该值就会自动添加RawImage，否则添加Image")]
        public float RawImageMinResolution;

        [LabelText("最大日志数量")]
        [Tooltip("不管子文件夹；0表示无穷大")]
        public int MaxLogsCount;

        public UIAssetTypeSetting GetTypeSetting(string name)
        {
            foreach (UIAssetTypeSetting typeSetting in AssetTypeSettings)
            {
                if (Regex.IsMatch(name, typeSetting.NamePattern))
                {
                    return typeSetting;
                }
            }

            return default;
        }

        public struct UIAssetTypeSetting
        {
            public string NamePattern;
            [VerticalGroup("Prefab二级目录")]
            [HideLabel]
            public string Folder;
            public GameObject Template;

            public static bool operator ==(UIAssetTypeSetting v1, UIAssetTypeSetting v2)
            {
                return v1.Equals(v2);
            }
            public static bool operator !=(UIAssetTypeSetting v1, UIAssetTypeSetting v2)
            {
                return !v1.Equals(v2);
            }
            public override bool Equals(object obj)
            {
                return base.Equals(obj);
            }
            public override int GetHashCode()
            {
                return base.GetHashCode();
            }
        }
    }
}
