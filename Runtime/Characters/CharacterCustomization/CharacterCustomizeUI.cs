using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using m4k.Items;
using m4k.UI;
using m4k.Characters;

namespace m4k.Characters.Customization {
public class UIInstance {
    public GameObject go;
    public Toggle toggle1;
    public Slider slider1;
    public TMPro.TMP_Text label1;
    public Image image1;

    public void ToggleShowHide(bool active) {
        go.SetActive(active);
    }
}

public class CharacterCustomizeUI : MonoBehaviour
{
    public GameObject sliderPrefab, colorPrefab;
    public ColorPicker colorPicker;
    public Transform mainParent;
    public Button confirmButton, cancelButton, randomizeButton;
    public bool hoveringViewport;
    public bool isActive { get { return gameObject.activeInHierarchy; }}
    string blendshapePrefix = "def";
    CharacterCustomizeOptions currCc;
    // int presetInd;
    // Slider presetSlider;
    // TMPro.TMP_Text presetTxt;
    CharacterCustomize customizeCharacter;

    public void Init(CharacterCustomize cc) {
        customizeCharacter = cc;
        
        colorPicker.onColorChange += OnColorChange;
        // var obj = Instantiate(sliderPrefab, mainParent, false);
        // presetSlider = obj.GetComponentInChildren<Slider>();
        // presetTxt = obj.GetComponentInChildren<TMPro.TMP_Text>();
        // presetSlider.minValue = 0;
        // presetSlider.maxValue = customizeCharacter.customizationPresets.Count - 1;
        // presetSlider.onValueChanged.AddListener(delegate { OnPresetSliderChange(presetSlider); });
        // OnPresetSliderChange(presetSlider);

        confirmButton.onClick.AddListener(FinalizeCustomization);
        cancelButton.onClick.AddListener(CancelCustomization);
        randomizeButton.onClick.AddListener(RandomizeCustomization);
    }
    private void OnDisable() {
        customizeCharacter.CancelCustomize();
    }

    public void ToggleActive(bool active) {
        gameObject.SetActive(active);
        colorPicker.ClearAndDisable();
    }

    public void SetHoveringViewport() => hoveringViewport = true;
    public void SetNotHoveringViewport() => hoveringViewport = false;

    public void SetupItemLibraries(CustomizeItemLibrary[] libraries) {
        for(int i = 0; i < libraries.Length; ++i) {
            var lib = libraries[i];

            lib.optionUI = new UIInstance();
            var optionInst = Instantiate(sliderPrefab, lib.parent);
            lib.optionUI.go = optionInst;
            lib.optionUI.slider1 = optionInst.GetComponentInChildren<Slider>();
            lib.optionUI.slider1.minValue = 0;
            lib.optionUI.slider1.maxValue = lib.items.Count - 1;
            lib.optionUI.label1 = optionInst.GetComponentInChildren<TMPro.TMP_Text>();
            lib.optionUI.slider1.onValueChanged.AddListener(delegate { OnSliderChange(lib); });

            for(int j = 0; j < 4; ++j) {
                var bsUI = new UIInstance();
                int v = j;
                var bsInst = Instantiate(sliderPrefab, lib.parent);
                bsUI.go = bsInst;
                bsUI.slider1 = bsInst.GetComponentInChildren<Slider>();
                bsUI.slider1.minValue = 0;
                bsUI.slider1.maxValue = 100;
                bsUI.slider1.onValueChanged.AddListener(delegate { OnBlendshapeSlider(lib, v); });
                bsUI.label1 = bsInst.GetComponentInChildren<TMPro.TMP_Text>();
                bsUI.ToggleShowHide(false);
                lib.bsSliders.Add(bsUI);
            }
            for(int j = 0; j < 4; ++j) {
                var colUI = new UIInstance();
                int v = j;
                var colInst = Instantiate(colorPrefab, lib.parent);
                colUI.go = colInst;
                colUI.toggle1 = colInst.GetComponentInChildren<Toggle>();
                colUI.toggle1.group = colorPicker.toggleGroup;
                colUI.toggle1.onValueChanged.AddListener(delegate { OnSwatchToggle(lib, v); });
                colUI.image1 = colInst.GetComponentInChildren<Image>();
                colUI.label1 = colInst.GetComponentInChildren<TMPro.TMP_Text>();
                colUI.ToggleShowHide(false);
                lib.colorPickers.Add(colUI);
            }
        }
    }

    public void Reset(CustomizeItemLibrary[] libraries) {
        colorPicker.ClearAndDisable();
        for(int i = 0; i < libraries.Length; ++i) {
            for(int j = 0; j < libraries[i].colorPickers.Count; ++j) {
                libraries[i].colorPickers[j].image1.color = Color.white;
            }
        }
    }

    public void UpdateOptions(CharacterCustomizeOptions cc) {
        for(int i = 0; i < cc.options.Count; ++i) {
            var o = cc.options[i];
            o.library.optionUI.slider1.value = o.library.tempItemInd;
            UpdateOptionLabel(o.library, o.library.items[o.library.tempItemInd]);
            for(int j = 0; j < o.blendShapes.Length; ++j) {
                o.library.bsSliders[j].slider1.value = o.blendShapes[j];
            }
            for(int j = 0; j < o.colors.Length; ++j) {
                o.library.colorPickers[j].image1.color = o.colors[j];
            }
        }
    }
    public void SetupOptions(CharacterCustomizeOptions cc) {
        currCc = cc;
        colorPicker.ClearAndDisable();

        for(int i = 0; i < cc.options.Count; ++i) {
            var o = cc.options[i];
            o.library.optionUI.ToggleShowHide(true);
            o.library.optionUI.slider1.value = o.library.tempItemInd;
            UpdateOptionLabel(o.library, o.library.items[o.library.tempItemInd]);

            SetupColorOptions(o.library);
            SetupBlendshapes(o.library);
        }
    }

    void SetupColorOptions(CustomizeItemLibrary library) {
        colorPicker.ClearAndDisable();
        if(!library.itemInstances[library.tempItemInd]) {
            return;
        }
        var mats = library.itemInstances[library.tempItemInd].GetComponentInChildren<Renderer>()?.materials;
        if(mats == null) return;

        for(int i = 0; i < mats.Length; ++i) {
            library.colorPickers[i].ToggleShowHide(true);
            library.colorPickers[i].image1.color = mats[i].color;
            library.colorPickers[i].label1.text = mats[i].name;
        }
        for(int i = mats.Length; i < library.colorPickers.Count; ++i) {
            library.colorPickers[i].ToggleShowHide(false);
        }
    }

    void SetupBlendshapes(CustomizeItemLibrary library) {
        if(!library.itemInstances[library.tempItemInd]) {
            return;
        }
        var skin = library.itemInstances[library.tempItemInd].GetComponentInChildren<SkinnedMeshRenderer>();
        if(!skin) 
            return;
        
        for(int i = 0; i < skin.sharedMesh.blendShapeCount; ++i) {
            var split = skin.sharedMesh.GetBlendShapeName(i).Split('_');
            if(split[0] == blendshapePrefix) {
                library.bsSliders[i].ToggleShowHide(true);
                library.bsSliders[i].label1.text = split[split.Length - 1];
                // library.bsSliders[i].slider1.value = skin.sharedMesh.GetBlendShapeFrameWeight(i, 0);
            }
        }
        for(int i = skin.sharedMesh.blendShapeCount; i < library.bsSliders.Count; ++i) {
            library.bsSliders[i].ToggleShowHide(false);
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(library.parent as RectTransform);
    }

    void OnSliderChange(CustomizeItemLibrary library) {
        int val = (int)library.optionUI.slider1.value;
        var item = library.items[val];
        library.tempItemInd = val;
        customizeCharacter.MannequinEquipItem(library, item);
        
        UpdateOptionLabel(library, item);
        SetupColorOptions(library);
        SetupBlendshapes(library);
        colorPicker.ClearAndDisable();
    }
    void OnSwatchToggle(CustomizeItemLibrary library, int matInd) {
        colorPicker.SetColorOption(library.optionType, matInd, library.colorPickers[matInd].toggle1);
    }
    void OnBlendshapeSlider(CustomizeItemLibrary library, int bsInd) {
        float val = library.bsSliders[bsInd].slider1.value;
        customizeCharacter.MannequinSetBlendshapeWeight(library.items[library.tempItemInd], bsInd, val);
    }
    void OnColorChange(ItemTag libraryTag, int matInd) {
        var library = customizeCharacter.GetLibrary(libraryTag);
        Color c = library.colorPickers[matInd].image1.color;
        customizeCharacter.MannequinChangeEquipColor(library.items[library.tempItemInd], c, matInd);
    }

    void UpdateOptionLabel(CustomizeItemLibrary library, Item item) {
        library.optionUI.label1.text = item.prefab ? 
        $"{library.optionType.ToString()} {library.optionUI.slider1.value + 1}" : 
        $"No {library.optionType.ToString()}";
    }
    // void OnPresetSliderChange(Slider slider) {
    //     presetTxt.text = string.Format("Preset {0}", slider.value);
    //     presetInd = (int)slider.value;
    // }

    public void FinalizeCustomization() {
        customizeCharacter.FinalizeCharacter();
    }
    public void CancelCustomization() {
        customizeCharacter.CancelCustomize();
    }
    public void RandomizeCustomization() {
        customizeCharacter.RandomizeCurrCustomize();
    }
}
}