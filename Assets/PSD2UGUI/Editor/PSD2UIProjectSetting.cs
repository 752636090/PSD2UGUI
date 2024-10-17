using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor.Internal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace PSD2UGUI
{
    public class PSD2UIProjectSetting : SerializedScriptableObject
    {
        private static PSD2UIProjectSetting instance;
        public static PSD2UIProjectSetting Instance
        {
            get
            {
                if (instance == null)
                {
                    Object[] objects = InternalEditorUtility.LoadSerializedFileAndForget("ProjectSettings/PSD2UIProjectSetting.asset");
                    if (objects.Length > 0)
                    {
                        instance = (PSD2UIProjectSetting)objects[0];
                    }
                    else
                    {
                        instance = CreateInstance<PSD2UIProjectSetting>();
                        string[] assets = Directory.GetFiles(Environment.CurrentDirectory, "PSD2UISetting.asset", SearchOption.AllDirectories);
                        if (assets.Length > 0)
                        {
                            instance.Setting = AssetDatabase.LoadAssetAtPath<PSD2UISetting>(Path.GetRelativePath(Environment.CurrentDirectory, assets[0]));
                        }
                        InternalEditorUtility.SaveToSerializedFileAndForget(new Object[] { Instance }, "ProjectSettings/PSD2UIProjectSetting.asset", true);
                    }
                }
                return instance;
            }
        }

        public PSD2UISetting Setting;

        private static Editor settingEditor;

        [SettingsProvider]
        private static SettingsProvider CreateProjectSettingProvider()
        {
            return new SettingsProvider("Project/PSD2UGUI", SettingsScope.Project)
            {
                label = "PSD2UGUI",
                activateHandler = ProjectSettingActiveHandler,
                guiHandler = ProjectSettingGUIHandler,
            };
        }

        private static void ProjectSettingActiveHandler(string _, VisualElement __)
        {
            if (settingEditor != null && settingEditor.target != Instance.Setting)
            {
                DestroyImmediate(settingEditor);
            }
            if (settingEditor == null && Instance.Setting != null)
            {
                settingEditor = Editor.CreateEditor(Instance.Setting);
            }
        }

        private static void ProjectSettingGUIHandler(string _)
        {
            PSD2UISetting newSetting = (PSD2UISetting)EditorGUILayout.ObjectField(Instance.Setting, typeof(PSD2UISetting), false);
            if (newSetting != Instance.Setting)
            {
                Instance.Setting = newSetting;
                if (settingEditor != null)
                {
                    DestroyImmediate(settingEditor); 
                }
                if (newSetting != null)
                {
                    settingEditor = Editor.CreateEditor(Instance.Setting); 
                }
                InternalEditorUtility.SaveToSerializedFileAndForget(new Object[] { Instance }, "ProjectSettings/PSD2UIProjectSetting.asset", true);
            }

            if (settingEditor != null)
            {
                EditorGUILayout.HelpBox("以下区域为上方文件的引用", MessageType.Info);
                settingEditor.OnInspectorGUI(); 
            }
        }
    }
}
