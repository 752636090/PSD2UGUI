using System;
using System.Collections.Generic;
using System.Text;
using Unity.Mathematics;
using UnityEngine;

namespace PSD2UGUI
{
    internal static class TransformHelper
    {
        public static IEnumerable<Transform> GetAllTransformsDFS(this Transform root)
        {
            Stack<Transform> stack = new();
            stack.Push(root);
            while (stack.Count > 0)
            {
                Transform transform = stack.Pop();
                yield return transform;
                foreach (Transform child in transform)
                {
                    stack.Push(child);
                }
            }
        }

        public static string GetAbsolutePathInHierarchy(this Transform transform)
        {
            StringBuilder pathSb = new();
            while (transform != null)
            {
                pathSb.Insert(0, "/");
                pathSb.Insert(0, transform.name);
                transform = transform.parent;
            }
            pathSb.Remove(pathSb.Length - 1, 1);
            return pathSb.ToString();
        }

        public static string GetRelativePath(this Transform self, Transform root, Func<string, string> nameModifier = null)
        {
            if (self == root)
            {
                return "";
            }

            StringBuilder pathSb = new();
            Transform transform = self;
            while (transform != root)
            {
                if (transform == null)
                {
                    Debug.LogError($"{transform.GetAbsolutePathInHierarchy()} 不是 {root.GetAbsolutePathInHierarchy()} 的子物体");
                }
                pathSb.Insert(0, "/");
                pathSb.Insert(0, nameModifier == null ? transform.name : nameModifier.Invoke(transform.name));
                transform = transform.parent;
            }
            pathSb.Remove(pathSb.Length - 1, 1);
            return pathSb.ToString();
        }

        public static string GetRelativePathWithRoot(this Transform self, Transform root)
        {
            StringBuilder pathSb = new();
            Transform transform = self;
            while (transform != null && transform != root.parent)
            {
                if (transform == null)
                {
                    Debug.LogError($"{transform.GetAbsolutePathInHierarchy()} 不是 {root.GetAbsolutePathInHierarchy()} 的子物体");
                }
                pathSb.Insert(0, "/");
                pathSb.Insert(0, transform.name);
                transform = transform.parent;
            }
            pathSb.Remove(pathSb.Length - 1, 1);
            return pathSb.ToString();
        }

        public static Vector2 GetCenterLocalPosition(this RectTransform self, RectTransform root)
        {
            return (self.GetCenterPosition() - root.GetCenterPosition()) / root.localScale;
        }

        public static Vector2 GetCenterPosition(this RectTransform self)
        {
            return new Vector2(self.position.x, self.position.y)
                + self.rect.center * self.lossyScale.x;
        }

        public static Vector2 Round(this Vector2 self, int decimalPlace = 0)
        {
            int factor = (int)math.pow(10, decimalPlace);
            return new(math.round(self.x * factor) / factor, math.round(self.y * factor) / factor);
        }
    }
}
