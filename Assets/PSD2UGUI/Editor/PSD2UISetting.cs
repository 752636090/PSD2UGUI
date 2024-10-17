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
        [Tooltip("�رձ༭����ʱ�Զ���ת�����Ϊ�������ת��֮ǰ�ĳ���")]
        public SceneAsset InitScene;

        [Tooltip("UI�༭������UI������·��")]
        public string UIRoot;

        [FolderPath(RequireExistingPath = true)]
        [LabelText("Prefabһ��Ŀ¼")]
        public string UIPrefabFolder;

        [TableList]
        public List<UIAssetTypeSetting> AssetTypeSettings = new();

        [FolderPath(RequireExistingPath = true)]
        [LabelText("ͼƬ����Ŀ¼")]
        public string UIImageFolder;

        [LabelText("�ֶ������ͼƬĿ¼")]
        public List<string> ImageManualFolders = new();

        [TypeDrawerSettings(BaseType = typeof(Image))]
        public Type ImageType;

        [TypeDrawerSettings(BaseType = typeof(RawImage))]
        public Type RawImageType;

        [Tooltip("����ͼƬ�������Slice���ͣ�ֻҪ�ֱ��������С�ﵽ��ֵ�ͻ��Զ����RawImage���������Image")]
        public float RawImageMinResolution;

        [LabelText("�����־����")]
        [Tooltip("�������ļ��У�0��ʾ�����")]
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
            [VerticalGroup("Prefab����Ŀ¼")]
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
