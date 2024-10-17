using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace PSD2UGUI
{
    public class PSDPostProcessor : AssetPostprocessor
    {
        private void OnPreprocessAsset()
        {
            if (!assetPath.EndsWith(".psd") && !assetPath.EndsWith(".psb"))
            {
                return;
            }

            if (AssetImporter.GetAtPath(assetPath) is PSDUIImporter)
            {
                return;
            }

            AssetDatabase.SetImporterOverride<PSDUIImporter>(assetPath);
        }
    }
}
