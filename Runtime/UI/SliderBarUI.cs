using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SliderBarUI : MonoBehaviour
{
    public Slider slider;
    public TMPro.TMP_Text label;

    public void SetSliderValue(float val, float max, float min = 0f) {
        slider.maxValue = max;
        slider.minValue = min;
        slider.value = val;
        if(label) {
            label.text = min == 0f ? string.Format("{0}/{1}", val, max) : val.ToString();
        }
    }
}
