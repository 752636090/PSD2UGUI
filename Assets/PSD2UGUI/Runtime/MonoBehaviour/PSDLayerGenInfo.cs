using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace PSD2UGUI
{
    [DisallowMultipleComponent]
    public class PSDLayerGenInfo : MonoBehaviour
    {
        #region UNITY_EDITOR
        public string SrcPath;
        public Vector2 SrcPos;
        public Vector2 SrcSize;

        public Rect SrcRect => new(SrcPos - SrcSize / 2, SrcSize);
        public string SrcName => string.IsNullOrEmpty(SrcPath) ? GetComponent<PSDGenInfos>().SrcFileName : Path.GetFileName(SrcPath).Split('[')[0];
        #endregion
    }
}
