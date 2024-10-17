using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace PSD2UGUI
{
    public class NewBehaviourScript : MonoBehaviour
    {
        [Button]
        private void LogRect()
        {
            Debug.Log($"{transform.localPosition}");
            Debug.Log(GetComponent<RectTransform>().rect);
            Vector2 center = new Vector2(transform.position.x, transform.position.y)
                + GetComponent<RectTransform>().rect.center * transform.lossyScale.x;
            Debug.Log(center / transform.lossyScale.x);
        }

        public GameObject PsdFile;
        [Button]
        private void LogPsdSize()
        {
            float xMin = 0;
            float yMin = 0;
            float xMax = 0;
            float yMax = 0;
            foreach (SpriteRenderer spriteRenderer in PsdFile.GetComponentsInChildren<SpriteRenderer>())
            {
                Debug.Log($"{spriteRenderer.bounds}-{spriteRenderer.localBounds}");
                xMin = math.min(xMin, spriteRenderer.bounds.min.x * spriteRenderer.sprite.pixelsPerUnit);
                yMin = math.min(yMin, spriteRenderer.bounds.min.y * spriteRenderer.sprite.pixelsPerUnit);
                xMax = math.max(xMax, spriteRenderer.bounds.max.x * spriteRenderer.sprite.pixelsPerUnit);
                yMax = math.max(yMax, spriteRenderer.bounds.max.y * spriteRenderer.sprite.pixelsPerUnit);
            }
            Debug.Log(new Vector2(xMax - xMin, yMax - yMin));
        }

        public float TestWidth;
        [Button]
        private void TestChangeWidth()
        {
            RectTransform rectTransform = GetComponent<RectTransform>();
            //Vector2 pivot = rectTransform.pivot;
            //Vector2 tempPos = rectTransform.localPosition;
            Vector2 centerPos = GetCenterPosition();
            //rectTransform.pivot = Vector2.one / 2;
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, TestWidth);
            //rectTransform.pivot = pivot;
            //rectTransform.localPosition = tempPos;
            rectTransform.position = centerPos - rectTransform.rect.center * rectTransform.lossyScale.x;
        }

        public Vector2 TestCenterPos;
        public Vector2 TestSize;
        [Button]
        public void TestChangeRect()
        {
            RectTransform rectTransform = GetComponent<RectTransform>();
            //Rect oldRect = rectTransform.rect;
            Rect realRect = new(TestCenterPos - TestSize / 2, TestSize);
            //Vector2 oldCenterPos = GetCenterPosition();
            //Vector2 centerPos = oldCenterPos + (realRect.center - oldRect.center) * rectTransform.lossyScale.x;
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, realRect.width);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, realRect.height);
            rectTransform.position = (realRect.position + rectTransform.pivot * realRect.size) * rectTransform.lossyScale.x;
        }

        private Vector2 GetCenterPosition()
        {
            RectTransform rectTransform = GetComponent<RectTransform>();
            return new Vector2(rectTransform.position.x, rectTransform.position.y)
                + rectTransform.rect.center * rectTransform.lossyScale.x;
        }
    }

}