using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PSD2UGUI
{
    [DisallowMultipleComponent]
    public class PSDGenInfos : MonoBehaviour
    {
#if UNITY_EDITOR
        public string SrcFileName;
        public List<string> AllSrcPaths = new();
        public Vector2 SrcSize;
#endif
    }
}
