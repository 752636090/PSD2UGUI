using PSD2UGUI;
using UnityEngine;

public class MyPSD2UIHandler : PSD2UICustomHandler
{
    public override RectTransform GetItemPrefab(Transform group)
    {
        // if (group.TryGetComponent(out LoopScrollRect loopScrollRect)) return loopScrollRect.ItemPrefab;
        return base.GetItemPrefab(group);
    }
}
