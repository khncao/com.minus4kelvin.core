
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

// public enum UIType {
//     HoriButton, VertButton, HoriPanel, VertPanel, TitleText, PrimaryText, SecondaryText, 
// }
public class UIObject : MonoBehaviour {
    [Header("Text")]
    public List<TMPro.TMP_Text> titleTxts;
    public List<TMPro.TMP_Text> txts;

    [Header("Sprites")]
    public List<Button> buttons;
    public List<Image> panels;
    public List<Slider> sliders;
    public List<ScrollRect> scrollRects;

    [Header("Colors")]
    public List<Graphic> tint1;

    public void ApplyTheme(UIThemeSO theme) {
        foreach(var s in sliders) {
            s.fillRect.GetComponent<Image>().sprite = theme.slider1;
            s.handleRect.GetComponent<Image>().sprite = theme.sliderKnob1;
        }
        
        foreach(var s in scrollRects) {
            if(s.verticalScrollbar)
                s.verticalScrollbar.image.sprite = theme.scrollbar1;
            if(s.horizontalScrollbar)
                s.horizontalScrollbar.image.sprite = theme.scrollbar1;
        }
        
        foreach(var b in buttons) {
            b.image.sprite = theme.button1;
            if(theme.button1) b.image.color = Color.white;
            
            // var cols = b.colors;
            // if(theme.tint1 != default(Color)) cols.normalColor = theme.tint1;
            // b.colors = cols;
        }

        foreach(var p in panels) {
            p.sprite = theme.panel1;
            if(theme.panel1) p.color = Color.white;
        }

        foreach(var t in txts) {
            if(theme.font1) t.font = theme.font1;
            if(theme.fontSize1 > 0f) t.fontSize = theme.fontSize1;
            if(theme.fontCol1 != default(Color)) t.color = theme.fontCol1;
        }

        foreach(var g in tint1) {
            if(theme.tint1 != default(Color)) g.color = theme.tint1;
        }
    }
}