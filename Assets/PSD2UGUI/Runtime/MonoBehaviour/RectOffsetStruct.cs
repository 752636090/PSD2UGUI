using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace PSD2UGUI
{
    public struct RectOffsetStruct
    {
        public int left;
        public int right;
        public int top;
        public int bottom;

        public RectOffsetStruct(int left, int right, int top, int bottom)
        {
            this.left = left;
            this.right = right;
            this.top = top;
            this.bottom = bottom;
        }

        public static bool operator ==(RectOffsetStruct p1, RectOffsetStruct p2)
        {
            return p1.Equals(p2);
        }

        public static bool operator !=(RectOffsetStruct p1, RectOffsetStruct p2)
        {
            return !p1.Equals(p2);
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
