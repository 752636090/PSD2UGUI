using NUnit.Framework.Internal;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Unity.Mathematics;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Search;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static PSD2UGUI.PSD2UISetting;
using CSImage = System.Drawing.Image;
using Image = UnityEngine.UI.Image;

namespace PSD2UGUI
{
    public class PSD2UIEditor : OdinEditorWindow
    {
        [MenuItem("Tools/WindowEditor/PSD2UGUI")]
        public static void OpenEditor()
        {
            if (PSD2UIProjectSetting.Instance.Setting == null)
            {
                DisplayErrorDialog("δ���� ProjectSettings - PSD2UGUI");
                return;
            }

            if (PSD2UIProjectSetting.Instance.Setting.UIScene == null)
            {
                DisplayErrorDialog("ProjectSettings-PSD2UGUI-UISceneΪ��");
                return;
            }

            GetWindow<PSD2UIEditor>();
        }

        [BoxGroup("����")]
        [OnValueChanged("OnPsdFileChanged")]
        public GameObject PsdFile;
        [BoxGroup("����")]
        [LabelText("Prefab����Ŀ¼")]
        public string PrefabThirdFolder;
        [BoxGroup("����")]
        [LabelText("Prefab����")]
        public string PrefabName;

        private PSD2UISetting Setting => PSD2UIProjectSetting.Instance.Setting;

        private HashSet<string> ValidTags = new() { "Ref", "Slice", "Ignore" };

        #region ����
        private string preScenePath;
        private float pixelsPerUnit = 100;
        private static StringBuilder logSb = new();
        #endregion

        [BoxGroup("����")]
        [Button("����")]
        private void Button_Export() => ExportClickHandler();

        private RectTransform lockChildrenRoot;
        [FoldoutGroup("С����")]
        [ShowInInspector]
        [Title("��������������λ�ã��Ա����������")]
        private RectTransform LockChildrenRoot { get => lockChildrenRoot; set => SetLockChildrenRoot(value); }
        private Dictionary<RectTransform, Vector3> locks = new();

        protected override void OnEnable()
        {
            base.OnEnable();
            preScenePath = SceneManager.GetActiveScene().path;
            if (Setting.InitScene != null)
            {
                preScenePath = AssetDatabase.GetAssetPath(Setting.InitScene);
            }
            if (preScenePath != AssetDatabase.GetAssetPath(Setting.UIScene))
            {
                EditorSceneManager.OpenScene(AssetDatabase.GetAssetPath(Setting.UIScene)); 
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            if (SceneManager.GetActiveScene().path != preScenePath)
            {
                EditorSceneManager.OpenScene(preScenePath); 
            }
        }

        private void Update()
        {
            UpdateLock();
        }

        #region ������
        private void ExportClickHandler()
        {
            try
            {
                if (Directory.Exists("Temp/PSD2UGUI/ExportLogs") && Setting != null && Setting.MaxLogsCount > 0)
                {
                    string[] logPaths = Directory.GetFiles("Temp/PSD2UGUI/ExportLogs");
                    if (logPaths.Length >= Setting.MaxLogsCount)
                    {
                        string deletePath = null;
                        foreach (string logPath in logPaths)
                        {
                            if (deletePath == null || File.GetLastWriteTime(logPath) < File.GetLastWriteTime(deletePath))
                            {
                                deletePath = logPath;
                            }
                        }
                        FileHelper.DeleteFile(deletePath);
                    }
                }
                logSb.Clear();
                SetLockChildrenRoot(null);
                ExportInner();
            }
            catch (Exception e)
            {
                DisplayErrorDialog(e.ToString());
            }
            finally
            {
                string logPath = $"Temp/PSD2UGUI/ExportLogs/{DateTime.Now:yyyy-MM-dd-hh-mm-ss-ff}.txt";
                if (!Directory.Exists("Temp/PSD2UGUI/ExportLogs"))
                {
                    Directory.CreateDirectory("Temp/PSD2UGUI/ExportLogs");
                }
                FileHelper.CreateTextFile(logPath, logSb.ToString());
                Debug.Log($"��־��������{Path.GetFullPath(logPath)}");
                logSb.Clear();
                AssetDatabase.Refresh();
            }
        }

        private bool ExportInner()
        {
            #region Ԥ��顢׼������
            if (Setting == null)
            {
                DisplayErrorDialog("δ���� ProjectSettings-PSD2UGUI");
                return false;
            }

            if (PsdFile == null)
            {
                DisplayErrorDialog("δָ��psd�ļ�");
                return false;
            }

            FileHelper.CleanDirectory("Temp/PSD2UGUI/TempImages");

            UIAssetTypeSetting typeSetting = Setting.GetTypeSetting(PrefabName);
            if (typeSetting == default)
            {
                DisplayErrorDialog("Prefab���ƴ�������ProjectSettings-PSD2UGUI-AssetTypeSettings-NamePattern");
                return false;
            }

            if (SceneManager.GetActiveScene().name != Setting.UIScene.name)
            {
                EditorSceneManager.OpenScene(AssetDatabase.GetAssetPath(Setting.UIScene));
            }

            Transform uiRoot = GameObject.Find(Setting.UIRoot).transform;
            if (uiRoot == null)
            {
                DisplayErrorDialog("�Ҳ���UIRoot������ProjectSettings-PSD2UGUI-UIRoot");
                return false;
            }

            CanvasScaler canvasScaler = uiRoot.GetComponentInParent<CanvasScaler>();
            if (canvasScaler == null)
            {
                DisplayErrorDialog("UIRoot���丸�����Ҳ���CanvasScaler");
                return false;
            }
            Vector2 resolution = canvasScaler.referenceResolution;

            DoubleMap<string, string> imageHashMap = CollectProjectUIImageHashMap();
            if (imageHashMap == null)
            {
                return false; // �����Ѿ�������
            }

            IEnumerable<Transform> psdLayers = PsdFile.transform.GetAllTransformsDFS();
            if (psdLayers.Select(a => a.name.Split('[')[0]).Distinct().Count() < psdLayers.Count())
            {
                DisplayErrorDialog("�����������壨��ǩ���������֣�");
                return false;
            }
            #endregion

            string prefabPath = GetPrefabPath(PrefabName, 4);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            bool isNew = prefab == null;
            PSDGenInfos genInfos = null;
            #region ����δӦ���޸ĵ�Ԥ�赽�����У���Ҫʱ�޸Ĵ�С
            if (uiRoot.Find(PrefabName) != null)
            {
                Undo.DestroyObjectImmediate(uiRoot.Find(PrefabName).gameObject);
            }

            Vector2 initSizeDelta;
            if (isNew)
            {
                if (typeSetting == default || typeSetting.Template == null)
                {
                    prefab = new GameObject(PrefabName, typeof(RectTransform));
                    prefab.AddComponent<CanvasRenderer>();
                    prefab.layer = 5;
                    initSizeDelta = new(100, 100);
                }
                else
                {
                    prefab = Instantiate(typeSetting.Template);
                    prefab.name = PrefabName;
                    initSizeDelta = (typeSetting.Template.transform as RectTransform).sizeDelta;
                }
                Undo.RegisterCreatedObjectUndo(prefab, "Create New UI Prefab");
            }
            else
            {
                Undo.RecordObject(prefab, "Edit Old UI Prefab In Asset");
                initSizeDelta = (prefab.transform as RectTransform).sizeDelta;
                prefab = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            }
            Undo.RecordObject(prefab, "Edit UI Prefab In Scene");
            if (!prefab.TryGetComponent(out genInfos))
            {
                genInfos = prefab.AddComponent<PSDGenInfos>();
                genInfos.enabled = false;
            }
            genInfos.SrcFileName = PsdFile.name;
            prefab.transform.SetParent(uiRoot);
            prefab.transform.localScale = Vector3.one;
            prefab.transform.localPosition = Vector3.zero;
            RectTransform prefabRectTrans = prefab.transform as RectTransform;
            prefabRectTrans.sizeDelta = initSizeDelta;
            if (prefabRectTrans.anchorMin == prefabRectTrans.anchorMax)
            {
                prefabRectTrans.sizeDelta = GetPsdSize(PsdFile.transform);
                if (prefabRectTrans.TryGetComponent(out LayoutElement layoutElement))
                {
                    if (layoutElement.preferredWidth == genInfos.SrcSize.x)
                    {
                        layoutElement.preferredWidth = prefabRectTrans.sizeDelta.x;
                    }
                    if (layoutElement.preferredHeight == genInfos.SrcSize.y)
                    {
                        layoutElement.preferredHeight = prefabRectTrans.sizeDelta.y;
                    }
                }
            }
            genInfos.SrcSize = GetPsdSize(PsdFile.transform);
            #endregion

            //HashSet<string> oldSrcPaths = new(genInfos.AllSrcPaths);
            HashSet<string> oldSrcNames = new(genInfos.AllSrcPaths.Select(a => Path.GetFileName(a)));
            oldSrcNames.Remove("");
            if (!isNew)
            {
                oldSrcNames.Add(genInfos.SrcFileName);
            }
            HashSet<string> unCheckedOldSrcNames = new(oldSrcNames);
            Dictionary<string, PSDLayerGenInfo> validLayerDict = new(); // Key: SrcName
            HashSet<string> manualDeletedNames = new();
            #region �ռ�����Ϣ
            foreach (Transform oldTrans in prefab.transform.GetAllTransformsDFS())
            {
                if (oldTrans.TryGetComponent(out PSDLayerGenInfo info))
                //&& (!PrefabUtility.IsAnyPrefabInstanceRoot(oldTrans.parent.gameObject)
                //    || oldTrans.parent == prefab.transform))
                {
                    validLayerDict.Add(info.SrcName, info);
                }
            }
            foreach (string srcName in oldSrcNames)
            {
                if (!validLayerDict.ContainsKey(srcName))
                {
                    manualDeletedNames.Add(srcName);
                }
            }
            //Dictionary<string, GameObject> oldSrcName2ObjDict = new();
            //foreach (PSDLayerGenInfo info in oldLayerInfos)
            //{
            //    oldSrcName2ObjDict.Add(Path.GetFileName(info.SrcPath), info.gameObject);
            //} 
            #endregion

            HashSet<string> removedTextures = new();
            List<(string, int4)> savedTextures = new();
            #region ����
            foreach (Transform layer in psdLayers)
            {
                if (!GetLayerTags(layer.name, out Dictionary<string, string> tags))
                {
                    return false;
                }
                if (tags.ContainsKey("Ignore"))
                {
                    continue;
                }

                string layerName = layer.name.Split('[')[0];
                unCheckedOldSrcNames.Remove(layerName);
                if (manualDeletedNames.Contains(layerName))
                {
                    LogColor($"�ֶ�ɾ����{layerName}����������", "2222BB");
                    continue;
                }

                bool isNewLayer;
                bool needCheckImage = false;
                RectTransform rectTransform;
                SpriteRenderer spriteRenderer = null;
                #region ����ͼ��
                if (validLayerDict.TryGetValue(layerName, out PSDLayerGenInfo layerInfo))
                {
                    isNewLayer = false;
                    rectTransform = layerInfo.transform as RectTransform;
                    layerInfo.SrcPath = layer.GetRelativePath(PsdFile.transform, name => name.Split('[')[0]);
                    if (layer.TryGetComponent(out spriteRenderer)
                        && rectTransform.TryGetComponent(out MaskableGraphic graphic)
                        && (graphic is Image || graphic is RawImage))
                    {
                        needCheckImage = true;
                    }
                }
                #endregion
                #region ��ͼ��
                else
                {
                    isNewLayer = true;
                    LogColor($"����ͼ�� {layerName}", "22BB22");
                    GameObject gameObject = null;

                    GameObject refObj = null;
                    if (tags.TryGetValue("Ref", out string refName))
                    {
                        if (string.IsNullOrEmpty(refName))
                        {
                            DisplayErrorDialog($"{layer.name}��Ref�Ĳ���Ϊ��");
                            return false;
                        }
                        refObj = FindUIPrefabInProject(refName);
                        if (refObj == null)
                        {
                            DisplayErrorDialog($"��Ҫ������{refObj}");
                            return false;
                        }
                    }

                    if (layer == PsdFile.transform)
                    {
                        gameObject = prefab;
                    }
                    else
                    {
                        gameObject = refObj != null
                            ? (GameObject)PrefabUtility.InstantiatePrefab(refObj)
                            : new(layerName, typeof(RectTransform));
                        gameObject.name = layerName;
                        gameObject.layer = 5;
                    }
                    rectTransform = gameObject.transform as RectTransform;
                    if (refObj == null)
                    {
                        layerInfo = gameObject.AddComponent<PSDLayerGenInfo>();
                    }
                    else
                    {
                        layerInfo = gameObject.GetComponent<PSDLayerGenInfo>();
                    }
                    layerInfo.enabled = false;
                    layerInfo.SrcPath = layer.GetRelativePath(PsdFile.transform, name => name.Split('[')[0]);
                    #region ���ø�����
                    string realPath = layerInfo.SrcPath;
                    validLayerDict.Add(layerName, layerInfo);
                    if (layer != PsdFile.transform)
                    {
                        Transform parent = null;
                        while (true)
                        {
                            realPath = string.IsNullOrEmpty(realPath) ? "" : Path.GetDirectoryName(realPath);
                            if (string.IsNullOrEmpty(realPath))
                            {
                                parent = prefab.transform;
                                break;
                            }
                            string parentName = Path.GetFileName(realPath);
                            if (validLayerDict.TryGetValue(parentName, out PSDLayerGenInfo parentInfo))
                            {
                                parent = parentInfo.transform;
                                break;
                            }
                        }
                        rectTransform.SetParent(parent);
                        rectTransform.localScale = Vector3.one;
                    }
                    #endregion
                    if (refObj == null && layer.TryGetComponent(out spriteRenderer))
                    {
                        if (!tags.ContainsKey("Slice") && spriteRenderer.sprite.textureRect.width * spriteRenderer.sprite.textureRect.height >= Setting.RawImageMinResolution)
                        {
                            RawImage image = (RawImage)gameObject.AddComponent(Setting.RawImageType ?? typeof(RawImage));
                            image.raycastTarget = false;
                        }
                        else
                        {
                            Image image = (Image)gameObject.AddComponent(Setting.ImageType ?? typeof(Image));
                            image.raycastTarget = false;
                            if (tags.ContainsKey("Slice"))
                            {
                                image.type = Image.Type.Sliced;
                            }
                        }
                        needCheckImage = true;
                    }
                    genInfos.AllSrcPaths.Add(layerInfo.SrcPath);
                }
                #endregion
                #region ��顢����������ͼƬ
                if (needCheckImage)
                {
                    #region ��ͼ�������¾�MD5
                    if (spriteRenderer.sprite.textureRect.width == 0)
                    {
                        DisplayErrorDialog($"{layer.name}ͼƬ�����⣬�ǲ��������ɰ棿");
                        return false;
                    }
                    Texture2D texture = spriteRenderer.sprite.ToTexture2D();
                    bool isSlice = tags.TryGetValue("Slice", out string sliceParamS);
                    int4 border = int4.zero;
                    if (isSlice)
                    {
                        int sliceParam;
                        if (string.IsNullOrEmpty(sliceParamS))
                        {
                            sliceParam = 0;
                        }
                        else if (!int.TryParse(sliceParamS, out sliceParam))
                        {
                            DisplayErrorDialog($"{layer.name} ��Slice��������");
                            return false;
                        }
                        (Texture2D tex, int4 border_) = texture.Slice(sliceParam != 1, sliceParam != 2);
                        border = border_;
                        texture = tex;
                    }
                    string newTexHash = texture.GetTextureMD5();

                    string oldTexHash = null;
                    Texture2D oldTexture = null;
                    if (rectTransform.TryGetComponent(out Image image) && image.sprite != null)
                    {
                        oldTexture = image.sprite.texture;
                        oldTexHash = oldTexture.GetFileMD5();
                        if (isSlice != (image.type == Image.Type.Sliced))
                        {
                            LogImportant($"Slice���ó�ͻ��{rectTransform.name} �� {layer.name}");
                        }
                    }
                    if (rectTransform.TryGetComponent(out RawImage rawImage) && rawImage.texture != null)
                    {
                        oldTexture = (Texture2D)rawImage.texture;
                        oldTexHash = oldTexture.GetFileMD5();
                        if (isSlice)
                        {
                            LogImportant($"Slice���ó�ͻ��{rectTransform.name} �� {layer.name}");
                        }
                    }
                    //oldTexHash = oldTexture.GetTextureMD5();
                    #endregion

                    #region ����ͼƬ������
                    //if (newTexHash != oldTexHash && oldTexture != null)
                    //{
                    //    string oldTexPath = AssetDatabase.GetAssetPath(oldTexture);
                    //    if (!string.IsNullOrEmpty(oldTexPath))
                    //    {
                    //        oldTexHash = MD5Helper.FileMD5(oldTexPath);
                    //        if (newTexHash == oldTexHash)
                    //        {
                    //            LogColor($"�����෢�1�����¶�һ��MD5 {oldTexPath}", "000000");
                    //        }
                    //    }
                    //}
                    if (newTexHash != oldTexHash)
                    {
                        //Debug.Log($"{oldTexHash} -> {newTexHash}");
                        //Debug.Log(oldTexture.GetTextureMD5());
                        //Debug.Log(image.sprite.ToTexture2D().GetTextureMD5());
                        //Debug.Log(image.sprite.texture.GetTextureMD5());
                        //Debug.Log(MD5Helper.FileMD5(imageHashMap.GetKeyByValue(newTexHash)));
                        if (oldTexture != null && oldTexture.name != layerName)
                        {
                            removedTextures.Add(AssetDatabase.GetAssetPath(oldTexture));
                        }

                        string texturePath;
                        if (imageHashMap.ContainsValue(newTexHash))
                        {
                            texturePath = imageHashMap.GetKeyByValue(newTexHash);
                            Log($"{rectTransform.name}��������ͼƬ{texturePath}");
                        }
                        else
                        {
                            texturePath = GetDefaultTexturePath(layerName);
                            LogColor($"{rectTransform.name}����/�޸�ͼƬ{texturePath}", "22BB22");
                            texture.SaveAsSprite(texturePath, border);
                            savedTextures.Add((texturePath, border));
                            imageHashMap.Add(texturePath, newTexHash);
                        }
                        texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);

                        if (image != null)
                        {
                            image.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(texturePath);
                        }
                        else if (rawImage != null)
                        {
                            rawImage.texture = texture;
                        }
                    }
                    #endregion
                }
                #endregion

                if (tags.ContainsKey("Ref"))
                {
                    UpdateRefRect(rectTransform, layer, layerInfo, isNewLayer);
                }
                else
                {
                    UpdateRect(rectTransform, layer, layerInfo, isNewLayer);
                }
            }

            foreach (string unCheckedName in unCheckedOldSrcNames)
            {
                LogImportant($"psdɾ����{unCheckedName}");
                PSDLayerGenInfo layerInfo = validLayerDict[unCheckedName];
                if (layerInfo.gameObject.TryGetComponent(out Graphic graphic) && graphic.mainTexture is Texture2D texture)
                {
                    string texPath = AssetDatabase.GetAssetPath(texture);
                    removedTextures.Add(texPath);
                }
                genInfos.AllSrcPaths.Remove(layerInfo.SrcPath);
                DestroyImmediate(layerInfo.gameObject);
            }

            prefab.transform.localPosition = Vector3.zero;
            if (PrefabUtility.IsAnyPrefabInstanceRoot(prefab))
            {
                PrefabUtility.ApplyPrefabInstance(prefab, InteractionMode.AutomatedAction);
            }
            else
            {
                PrefabUtility.SaveAsPrefabAssetAndConnect(prefab, prefabPath, InteractionMode.AutomatedAction);
            }
            AssetDatabase.Refresh();
            #endregion

            CheckDeleteTextures(prefab.transform, removedTextures);

            Log("�����ɹ�");
            EditorUtility.DisplayDialog("Success", "�����ɹ�", "ȷ��");
            return true;
        }

        private DoubleMap<string, string> CollectProjectUIImageHashMap()
        {
            if (!Directory.Exists(Setting.UIImageFolder))
            {
                DisplayErrorDialog("�ļ��в����ڣ�����ProjectSettings-PSD2UGUI-UIImageFolder");
                return null;
            }

            DoubleMap<string, string> imageHashMap = new();
            foreach (string absPath in Directory.GetFiles(Setting.UIImageFolder, "*.png", SearchOption.AllDirectories))
            {
                string path = absPath[absPath.IndexOf(Setting.UIImageFolder)..].Replace('\\', '/');
                //using CSImage image = CSImage.FromFile(path);
                //using Bitmap bitmap = new(image);
                imageHashMap.Add(path, MD5Helper.FileMD5(path));
                //DateTime time = DateTime.Now;
                //for (int i = 0; i < 10; i++)
                //{
                //    MD5Helper.FileMD5(path);
                //}
                //Debug.Log((DateTime.Now - time).Ticks);
                //time = DateTime.Now;
                //for (int i = 0; i < 10; i++)
                //{
                //    AssetDatabase.LoadAssetAtPath<Texture2D>(path).DeCompress().EncodeToPNG().GetMD5();
                //}
                //Debug.Log((DateTime.Now - time).Ticks);
            }

            return imageHashMap;
        }

        private void UpdateRect(RectTransform rectTransform, Transform layer, PSDLayerGenInfo layerInfo, bool isNew)
        {
            string layerName = layer.name.Split('[')[0];
            Vector2 srcPos = GetPsdLayerPosition(layer);
            Vector2 srcSize = GetPsdSize(layer);
            Rect psdLayerRect = new(srcPos - srcSize / 2, srcSize);
            if (isNew || psdLayerRect != layerInfo.SrcRect)
            {
                if (rectTransform.TryGetComponent(out ContentSizeFitter _))
                {
                    LogColor($"{layerName}��Rect�仯����{rectTransform.name}����ContentSizeFitter�����Ժ��Ա仯", "F6F63F");
                }
                else
                {
                    if (isNew)
                    {
                        Rect realRect = psdLayerRect;
                        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, realRect.width);
                        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, realRect.height);
                        rectTransform.position = (realRect.position + rectTransform.pivot * realRect.size) * rectTransform.lossyScale.x;
                        if (rectTransform.TryGetComponent(out LayoutElement layoutElement))
                        {
                            if (layoutElement.preferredWidth > 0)
                            {
                                layoutElement.preferredWidth = realRect.width;
                            }
                            if (layoutElement.preferredHeight > 0)
                            {
                                layoutElement.preferredHeight = realRect.height;
                            }
                        }
                    }
                    else
                    {
                        LogColor($"{layerName}��Rect�仯���Զ��ı�{rectTransform.name}", "F6F63F");
                        Vector2 oldSize = rectTransform.rect.size;
                        Rect rectManualDiff = new((rectTransform.GetCenterPosition() / rectTransform.lossyScale.x) - layerInfo.SrcRect.center,
                            oldSize - layerInfo.SrcRect.size);
                        Rect realRect = new(psdLayerRect.position + rectManualDiff.position,
                            psdLayerRect.size + rectManualDiff.size);
                        Dictionary<Transform, Vector3> childPosDict = new();
                        foreach (Transform child in rectTransform)
                        {
                            childPosDict.Add(child, child.position);
                        }
                        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, realRect.width);
                        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, realRect.height);
                        rectTransform.position = (realRect.position + rectTransform.pivot * realRect.size) * rectTransform.lossyScale.x;
                        rectTransform.localPosition = rectTransform.localPosition - Vector3.forward * rectTransform.localPosition.z;
                        foreach (Transform child in rectTransform)
                        {
                            child.position = childPosDict[child];
                        }
                        if (rectTransform.TryGetComponent(out LayoutElement layoutElement))
                        {
                            if (layoutElement.preferredWidth > 0)
                            {
                                layoutElement.preferredWidth += realRect.width - oldSize.x;
                            }
                            if (layoutElement.preferredHeight > 0)
                            {
                                layoutElement.preferredHeight += realRect.height - oldSize.y;
                            }
                        }
                    }
                }
            }
            layerInfo.SrcPos = srcPos;
            layerInfo.SrcSize = srcSize;
        }

        private void UpdateRefRect(RectTransform rectTransform, Transform layer, PSDLayerGenInfo layerInfo, bool isNew)
        {
            Vector2 srcPos = GetPsdLayerPosition(layer);
            Vector2 srcSize = GetPsdSize(layer);
            PSDGenInfos refInfo = rectTransform.GetComponent<PSDGenInfos>();
            Vector2 manualDiff = isNew ? Vector2.zero
                : rectTransform.GetCenterPosition() / rectTransform.lossyScale.x - layerInfo.SrcPos;
            rectTransform.localScale = new(math.round(srcSize.x / refInfo.SrcSize.x * 100) / 100
                , math.round(srcSize.y / refInfo.SrcSize.y * 100) / 100, 1);
            rectTransform.position = (srcPos - srcSize / 2 + rectTransform.pivot * srcSize + manualDiff) * rectTransform.lossyScale.x;
            rectTransform.localPosition = rectTransform.localPosition - Vector3.forward * rectTransform.localPosition.z;
            layerInfo.SrcPos = srcPos;
            layerInfo.SrcSize = srcSize;
        }

        private string GetPrefabPath(string name, int level = 4)
        {
            if (level == 1)
            {
                return Setting.UIPrefabFolder;
            }

            name = Path.GetFileNameWithoutExtension(name);
            string secondFolder = "Other";
            foreach (UIAssetTypeSetting typeSetting in Setting.AssetTypeSettings)
            {
                if (Regex.IsMatch(name, typeSetting.NamePattern))
                {
                    secondFolder = typeSetting.Folder;
                    break;
                }
            }

            if (level == 2)
            {
                return Path.Combine(Setting.UIPrefabFolder, secondFolder);
            }
            else if (level == 3)
            {
                if (string.IsNullOrEmpty(PrefabThirdFolder) || PrefabThirdFolder == ".")
                {
                    return Path.Combine(Setting.UIPrefabFolder, secondFolder);
                }
                else
                {
                    return Path.Combine(Setting.UIPrefabFolder, secondFolder, PrefabThirdFolder);
                }
            }
            else // if (level == 4)
            {
                if (string.IsNullOrEmpty(PrefabThirdFolder) || PrefabThirdFolder == ".")
                {
                    return Path.Combine(Setting.UIPrefabFolder, secondFolder, $"{name}.prefab");
                }
                else
                {
                    return Path.Combine(Setting.UIPrefabFolder, secondFolder, PrefabThirdFolder, $"{name}.prefab");
                }
            }
        }

        private bool GetLayerTags(string nameWithTags, out Dictionary<string, string> tags)
        {
            tags = new();
            Stack<(char, int)> bracketStack = new();
            for (int i = 0; i < nameWithTags.Length; i++)
            {
                bool addTag = false;
                if (nameWithTags[i] == '[')
                {
                    if (bracketStack.Count > 0)
                    {
                        addTag = true;
                    }
                    else
                    {
                        bracketStack.Push(('[', i));
                    }
                }
                else if (nameWithTags[i] == ']')
                {
                    if (bracketStack.Count == 0 || bracketStack.Peek().Item1 != '[')
                    {
                        DisplayErrorDialog($"�����������Ų�ƥ�䣺{nameWithTags}");
                        return false;
                    }
                    addTag = true;
                }
                if (addTag)
                {
                    int startIndex = bracketStack.Pop().Item2;
                    string tag = nameWithTags.Substring(startIndex + 1, i - startIndex - 1);
                    string[] map = tag.Split(':', '��');
                    if (map.Length == 2)
                    {
                        tags[map[0]] = map[1];
                    }
                    else if (map.Length == 1)
                    {
                        tags[map[0]] = null;
                    }
                    else
                    {
                        DisplayErrorDialog($"�����������Ų�ƥ�䣺{nameWithTags}");
                        return false;
                    }
                    if (!ValidTags.Contains(map[0]))
                    {
                        DisplayErrorDialog($"������֧�ֵı�ǩ{map[0]}������ƴд��{nameWithTags}");
                        return false;
                    }
                }
            }

            return true;
        }

        private GameObject FindUIPrefabInProject(string psdName)
        {
            string[] prefabSearchResults = Directory.GetFiles(Setting.UIPrefabFolder, "*.prefab", SearchOption.AllDirectories);
            foreach (string prefabFilePath in prefabSearchResults)
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabFilePath);
                if (prefab.TryGetComponent(out PSDGenInfos genInfos) && genInfos.SrcFileName == psdName)
                {
                    return prefab;
                }
            }
            return null;
        }

        private Vector2 GetPsdLayerPosition(Transform transform)
        {
            Vector2 pos = (transform.position - PsdFile.transform.position) * pixelsPerUnit;
            pos.x = math.round(pos.x * 1000) / 1000;
            pos.y = math.round(pos.y * 1000) / 1000;
            return pos;
        }

        private Vector2 GetPsdSize(Transform transform)
        {
            float xMin = int.MaxValue;
            float yMin = int.MaxValue;
            float xMax = int.MinValue;
            float yMax = int.MinValue;
            SpriteRenderer[] renderers = transform.GetComponentsInChildren<SpriteRenderer>();
            if (renderers.Length > 0)
            {
                foreach (SpriteRenderer spriteRenderer in renderers)
                {
                    xMin = math.min(xMin, spriteRenderer.bounds.min.x);
                    yMin = math.min(yMin, spriteRenderer.bounds.min.y);
                    xMax = math.max(xMax, spriteRenderer.bounds.max.x);
                    yMax = math.max(yMax, spriteRenderer.bounds.max.y);
                }
                xMin = math.round(xMin * 1000) / 1000;
                yMin = math.round(yMin * 1000) / 1000;
                xMax = math.round(xMax * 1000) / 1000;
                yMax = math.round(yMax * 1000) / 1000;
            }
            else
            {
                xMin = xMax = yMin = yMax = 0;
            }
            return new Vector2(xMax - xMin, yMax - yMin) * pixelsPerUnit;
        }

        private string GetDefaultTexturePath(string imageName)
        {
            return Path.Combine(Setting.UIImageFolder, PrefabName, $"{imageName}.png").Replace('\\', '/');
        }

        private bool IsTextureInDefaultFolder(Texture2D texture)
        {
            string path = AssetDatabase.GetAssetPath(texture);
            return IsTextureInDefaultFolder(path);
        }

        private bool IsTextureInDefaultFolder(string path)
        {
            string defaultFolder = Path.Combine(Setting.UIImageFolder, PrefabName).Replace('\\', '/');
            return Path.GetDirectoryName(path).Replace('\\', '/') == defaultFolder;
        }

        private void CheckDeleteTextures(Transform prefab, HashSet<string> removedTextures)
        {
            HashSet<Texture2D> referencedTextures = new();
            foreach (MaskableGraphic graphic in prefab.GetComponentsInChildren<MaskableGraphic>())
            {
                if (graphic.mainTexture is Texture2D texture)
                {
                    referencedTextures.Add(texture); 
                }
            }
            foreach (string path in removedTextures)
            {
                Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                if (!referencedTextures.Contains(texture))
                {
                    if (IsTextureInDefaultFolder(path)
                        /*&& Setting.ImageManualFolders.FindIndex(a => path.StartsWith(a)) < 0*/)
                    {
                        AssetDatabase.MoveAssetToTrash(path);
                        LogImportant($"�������վ��{path}");
                    }
                    else
                    {
                        LogImportant($"�ֶ�������ļ���UI��û�������ã�{path}");
                    }
                }
            }
        }
        #endregion

        #region С����
        private void SetLockChildrenRoot(RectTransform value)
        {
            if (value == lockChildrenRoot)
            {
                return;
            }

            locks.Clear();
            foreach (RectTransform child in value)
            {
                locks.Add(child, child.position);
            }
            lockChildrenRoot = value;
        }

        private void UpdateLock()
        {
            foreach ((RectTransform transform, Vector2 pos) in locks)
            {
                if (transform != null)
                {
                    transform.position = pos; 
                }
            }
        }
        #endregion

        #region Odin����
        private void OnPsdFileChanged()
        {
            if (PSD2UIProjectSetting.Instance.Setting == null)
            {
                DisplayErrorDialog("δ���� ProjectSettings - PSD2UGUI");
                PsdFile = null; 
                return;
            }

            if (PsdFile != null)
            {
                bool isNotPsd = false;
                string psdPath = AssetDatabase.GetAssetPath(PsdFile);
                if (string.IsNullOrEmpty(psdPath))
                {
                    isNotPsd = true;
                }
                else
                {
                    GUID psdGUID = AssetDatabase.GUIDFromAssetPath(psdPath);
                    if (!typeof(PSDUIImporter).IsAssignableFrom(AssetDatabase.GetImporterOverride(psdPath))
                        && !typeof(PSDUIImporter).IsAssignableFrom(AssetDatabase.GetImporterType(psdGUID)))
                    {
                        isNotPsd = true;
                    }
                }
                if (isNotPsd)
                {
                    DisplayErrorDialog("����psd�ļ�");
                    PsdFile = null; 
                }
            }

            if (PsdFile == null)
            {
                PrefabThirdFolder = null;
                PrefabName = null;
                return;
            }

            GameObject prefab = FindUIPrefabInProject(PsdFile.name);
            if (prefab != null)
            {
                PrefabName = prefab.name;
                string assetPath = AssetDatabase.GetAssetPath(prefab);
                PrefabThirdFolder = Path.GetRelativePath(GetPrefabPath(prefab.name, 2), Path.GetDirectoryName(assetPath));
                if (PrefabThirdFolder == ".")
                {
                    PrefabThirdFolder = null;
                }
                if (PsdFile.transform.GetComponentInChildren<SpriteRenderer>() != null)
                {
                    pixelsPerUnit = PsdFile.transform.GetComponentInChildren<SpriteRenderer>().sprite.pixelsPerUnit;
                }
                else
                {
                    DisplayErrorDialog("psd�ļ���ûͼ");
                    PsdFile = null;
                    OnPsdFileChanged();
                }
            }
            else
            {
                PrefabThirdFolder = null;
                PrefabName = PsdFile.name;
            }
        }
        #endregion

        private static void DisplayErrorDialog(string message)
        {
            LogImportant(message);
            EditorUtility.DisplayDialog("Error", message, "ȷ��");
        }

        private static void LogImportant(string message)
        {
            LogColor(message, "BB2222");
        }

        private static void LogColor(string message, string color)
        {
            Debug.Log($"<color=#{color}>{message}</color>");
            logSb.Append("[");
            logSb.Append(color);
            logSb.Append("]");
            logSb.AppendLine(message);
        }

        private static void Log(string message)
        {
            Debug.Log(message);
            logSb.AppendLine(message);
        }
    }
}
