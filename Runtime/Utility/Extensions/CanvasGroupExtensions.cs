using UnityEngine;

public static class CanvasGroupExtensions 
{
    public static void Fade(this CanvasGroup canvasGroup, bool b) {
        if(b) FadeIn(canvasGroup);
        else FadeOut(canvasGroup);
    }
    public static void FadeIn(this CanvasGroup canvasGroup) {
        canvasGroup.alpha = 1;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }
    public static void FadeOut(this CanvasGroup canvasGroup) {
        canvasGroup.alpha = 0;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }
}
