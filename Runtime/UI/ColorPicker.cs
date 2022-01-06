using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using m4k.Items;

namespace m4k.UI {
public class ColorPicker : MonoBehaviour, IBeginDragHandler, IDragHandler
{
    public Image colorPicker, cursor, clickCloseBg;
    public System.Action<ItemTag, int> onColorChange;
    public ToggleGroup toggleGroup;
    ItemTag currFocusTag;
    int currMatIndex;
    Image currFocusImageUI;
    Texture2D colorPickerImg;
    Vector3[] imageCorners;
    Color onBeginColor;
    void Awake() {
        colorPickerImg = colorPicker.mainTexture as Texture2D;
        imageCorners = new Vector3[4];
        gameObject.GetComponent<RectTransform>().GetWorldCorners(imageCorners);
    }

    public void SetColorOption(ItemTag libraryTag, int matInd, Toggle toggle = null) {
        gameObject.SetActive(toggle.isOn);
        if(!toggle) return;
        currFocusImageUI = toggle.GetComponentInChildren<Image>();
        currFocusTag = libraryTag;
        currMatIndex = matInd;
        clickCloseBg.gameObject.SetActive(true);
    }

    public void ClearAndDisable() {
        currFocusImageUI = null;
        gameObject.SetActive(false);
        clickCloseBg.gameObject.SetActive(false);
        toggleGroup.SetAllTogglesOff();
    }
    public void OnBeginDrag(PointerEventData data) {
        
    }

    public void OnDrag(PointerEventData data) {
        float texWidth = imageCorners[2].x - imageCorners[0].x;
        float texHeight = imageCorners[2].y - imageCorners[0].y;
        float uvX = (data.position.x - imageCorners[0].x) / texWidth;
        float uvY = (data.position.y - imageCorners[0].y) / texHeight;

        if(gameObject.activeInHierarchy && currFocusImageUI) {
            currFocusImageUI.color = colorPickerImg.GetPixelBilinear(uvX, uvY);
            onColorChange?.Invoke(currFocusTag, currMatIndex);
        }
    }

}
}