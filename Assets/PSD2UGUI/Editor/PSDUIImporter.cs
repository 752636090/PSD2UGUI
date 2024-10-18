using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEditor.U2D.PSD;
using UnityEngine;

namespace PSD2UGUI
{
    [ScriptedImporter(22200003, new string[] { }, new[] { "psb", "psd" }, AllowCaching = true)]
    public class PSDUIImporter : PSDImporter
    {
        private PSDUIImporter() : base()
        {
            FieldInfo m_TextureImporterSettingsFieldInfo = typeof(PSDImporter).GetField("m_TextureImporterSettings", BindingFlags.Instance | BindingFlags.NonPublic);
            TextureImporterSettings textureImporterSettings = (TextureImporterSettings)m_TextureImporterSettingsFieldInfo.GetValue(this);
            textureImporterSettings.readable = true;
            textureImporterSettings.filterMode = FilterMode.Point;
            textureImporterSettings.mipmapEnabled = false;
            textureImporterSettings.npotScale = TextureImporterNPOTScale.None;
            textureImporterSettings.aniso = 0;

            FieldInfo m_PlatformSettingsFieldInfo = typeof(PSDImporter).GetField("m_PlatformSettings", BindingFlags.Instance | BindingFlags.NonPublic);
            List<TextureImporterPlatformSettings> m_PlatformSettings = (List<TextureImporterPlatformSettings>)m_PlatformSettingsFieldInfo.GetValue(this);
            if (m_PlatformSettings.Count == 0)
            {
                TextureImporterPlatformSettings platformSettings = new()
                {
                    maxTextureSize = 16384,
                    textureCompression = TextureImporterCompression.Uncompressed,
                };
                m_PlatformSettings.Add(platformSettings);
            }

            // UseLayerGroup
            FieldInfo m_GenerateGOHierarchyFieldInfo = typeof(PSDImporter).GetField("m_GenerateGOHierarchy", BindingFlags.Instance | BindingFlags.NonPublic);
            m_GenerateGOHierarchyFieldInfo.SetValue(this, true);

            FieldInfo m_DocumentAlignmentFieldInfo = typeof(PSDImporter).GetField("m_DocumentAlignment", BindingFlags.Instance | BindingFlags.NonPublic);
            m_DocumentAlignmentFieldInfo.SetValue(this, SpriteAlignment.Center);

            FieldInfo m_ResliceFromLayerFieldInfo = typeof(PSDImporter).GetField("m_ResliceFromLayer", BindingFlags.Instance | BindingFlags.NonPublic);
            m_ResliceFromLayerFieldInfo.SetValue(this, true);
        }
    } 
}
