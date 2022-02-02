
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace m4k {
public static class ScrollRectExtensions
{
    // ref: https://stackoverflow.com/a/30769550
    public static void SnapTo(this ScrollRect scrollRect, RectTransform target)
    {
        Canvas.ForceUpdateCanvases();

        // scrollRect.content.anchoredPosition =
        //     (Vector2)scrollRect.transform.InverseTransformPoint(scrollRect.content.position)
        //     - (Vector2)scrollRect.transform.InverseTransformPoint(target.position);
        scrollRect.content.position =
            (scrollRect.content.position
            - target.position) * 2f;
    }
}
}