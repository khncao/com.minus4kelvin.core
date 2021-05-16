using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using m4k.InventorySystem;
using m4k.Characters;
using m4k.Utility;

namespace m4k.Characters.Customization {

[System.Serializable]
public class CharacterCustomizationData {
    public List<CharacterCustomizeOptions> customizeOptions;
}

[System.Serializable]
public class CharacterCustomizeOptions {
    public string characterName;
    public List<CustomizeItemOption> options = new List<CustomizeItemOption>();

    [System.NonSerialized]
    public CharacterControl charCtrl;
}

[System.Serializable] 
public class CustomizeItemOption {
    public ItemTag optionType;
    public string itemName;
    public Color[] colors;
    public float[] blendShapes;

    [System.NonSerialized]
    public CustomizeItemLibrary library;
}

[System.Serializable]
public class CustomizeItemLibrary {
    public ItemTag optionType;
    public List<Item> items;
    public Transform parent;
    public GameObject section;

    [System.NonSerialized]
    public int tempItemInd;
    [System.NonSerialized]
    public GameObject[] itemInstances;
    [System.NonSerialized]
    public UIInstance optionUI;
    [System.NonSerialized]
    public List<UIInstance> colorPickers = new List<UIInstance>();
    [System.NonSerialized]
    public List<UIInstance> bsSliders = new List<UIInstance>();
}

public class CharacterCustomize : Singleton<CharacterCustomize>
{
    public GameObject mannequinPrefab;
    public CharacterControl mannequin;
    public CharacterCustomizeUI UI;
    public CustomizeItemLibrary[] itemLibraries;
    public List<CharacterCustomizeOptions> characterCustomizations = new List<CharacterCustomizeOptions>();
    bool isCustomizing;
    CharacterCustomizeOptions currCharCustomize;
    List<string> givenNames = new List<string>(){ "Orange", "Red", "Bat", "Glow", "Dusk", };

    Dictionary<ItemTag, CustomizeItemLibrary> itemLibraryDict = new Dictionary<ItemTag, CustomizeItemLibrary>();

    private void Start() {
        for(int i = 0; i < itemLibraries.Length; ++i) {
            itemLibraryDict[itemLibraries[i].optionType] = itemLibraries[i];
            itemLibraries[i].items = AssetRegistry.I.GetItemListByTag(itemLibraries[i].optionType);
            itemLibraries[i].itemInstances = new GameObject[itemLibraries[i].items.Count];
        }
        UI.Init(this);
        UI.SetupItemLibraries(itemLibraries);

        // var mannequin = Instantiate(mannequinPrefab);
        // mannequin.transform.position = new Vector3(0, 1000f, 0);
        // mannequinAnim = mannequin.GetComponentInChildren<CharacterAnimation>();
        // mannequin.SetActive(false);
    }
    void CleanItemLibraries() {
        for(int i = 0; i < itemLibraries.Length; ++i) { 
            itemLibraries[i].tempItemInd = 0;

            // for(int j = 0; j < itemLibraries[i].itemInstances.Length; ++j) {
            //     Destroy(itemLibraries[i].itemInstances[j]);
            // }
        }
    }
    int GetItemIndex(Item item) {
        int index = itemLibraryDict[item.itemTags[0]].items.FindIndex(x=>x==item);
        if(index == -1)
            Debug.LogWarning("item not found");
        return index;
    }
    public CustomizeItemLibrary GetLibrary(ItemTag type) {
        CustomizeItemLibrary library = null;
        itemLibraryDict.TryGetValue(type, out library);
        return library;
    }
    CharacterCustomizeOptions GetCharacterCustomize(string cn) {
        int ind = characterCustomizations.FindIndex(x=>x.characterName == cn);
        if(ind == -1) {
            return null;
        }
        return characterCustomizations[ind];
    }
    public void ApplyCustomizationsOnCharacterSpawn(Character character, GameObject instance) {
        var cc = GetCharacterCustomize(character.name);
        if(cc == null) 
            return;
        for(int i = 0; i < cc.options.Count; ++i) {
            var library = GetLibrary(cc.options[i].optionType);
            if(library == null) continue;
            cc.options[i].library = library;
        }
        var chara = instance.GetComponent<CharacterControl>();
        cc.charCtrl = chara;
        cc.characterName = character.name;
        currCharCustomize = cc;
        LoadCharacterCustomizations(cc.charCtrl);
    }
    public void CustomizeFocused() {
        if(!CharacterManager.I.focused) return;
        SetCharacter(CharacterManager.I.focused);
    }
    public void CustomizePlayer() {
        SetCharacter(CharacterManager.I.Player);
    }
    public void SetCharacter(Character character) {
        var chara = CharacterManager.I.GetCharInstance(character);
        if(!chara) {
            Debug.LogWarning("Char instance not found");
            return;
        }
        SetCharacter(chara);
    }
    public void SetCharacter(CharacterControl character) {
        if(isCustomizing) return;
        UI.ToggleActive(true);
        // isCurrPlayer = character == Characters.I.Player.cc;
        isCustomizing = true;
        // mannequinAnim.gameObject.SetActive(true);
        var mannequin = Instantiate(mannequinPrefab);
        mannequin.transform.position = new Vector3(0, 1000f, 0);
        this.mannequin = mannequin.GetComponentInChildren<CharacterControl>();

        var cc = GetCharacterCustomize(character.character.name);
        if(cc == null) {
            // Debug.Log("Creating new customization profile");
            cc = new CharacterCustomizeOptions();
            cc.characterName = character.character.name;

            for(int i = 0; i < itemLibraries.Length; ++i) {
                var option = new CustomizeItemOption();
                option.optionType = itemLibraries[i].optionType;
                option.library = itemLibraries[i];
                cc.options.Add(option);

                var charEquip = character.charEquip.GetSlotFromTag(itemLibraries[i].optionType);
                if(charEquip == null || !charEquip.item) 
                    continue;
                option.itemName = charEquip.item.name;
                itemLibraries[i].tempItemInd = GetItemIndex(charEquip.item);
            }
        }

        cc.charCtrl = character;
        currCharCustomize = cc;
        LoadCharacterCustomizations(this.mannequin);

        UI.SetupOptions(currCharCustomize);
        this.mannequin.charAnim.TogglePoseMode(true);
            Cams.I.SetCamTarget(this.mannequin.gameObject);
    }

    public void FinalizeCharacter() {
        if(!characterCustomizations.Contains(currCharCustomize)) {
            characterCustomizations.Add(currCharCustomize);
            Debug.Log("Customization profile listed, total: " + characterCustomizations.Count);
        }
        for(int i = 0; i < currCharCustomize.options.Count; ++i){
            var o = currCharCustomize.options[i];
            o.itemName = o.library.items[o.library.tempItemInd].name;

            o.colors = new Color[o.library.colorPickers.Count];
            o.blendShapes = new float[o.library.bsSliders.Count];

            for(int j = 0; j < o.colors.Length; ++j) {
                o.colors[j] = o.library.colorPickers[j].image1.color;
            }
            for(int k = 0; k < o.blendShapes.Length; ++k) {
                o.blendShapes[k] = o.library.bsSliders[k].slider1.value;
            }
        }
        LoadCharacterCustomizations(currCharCustomize.charCtrl);

        CancelCustomize();
    }
    public void CancelCustomize() {
        CleanItemLibraries();
        Cams.I?.ClearCamTarget();
        isCustomizing = false;
        // mannequinAnim.gameObject.SetActive(false);
        if(mannequin)
            Destroy(mannequin.gameObject);
        UI.ToggleActive(false);
        UI.Reset(itemLibraries);
    }

    public void MannequinEquipItem(CustomizeItemLibrary library, Item item) {
        library.itemInstances[library.tempItemInd] = mannequin.charEquip.EquipItem(item);
    }
    public void MannequinChangeEquipColor(Item item, Color color, int matInd) {
        mannequin.charEquip.ChangeEquipColor(item, color, matInd);
    }
    public void MannequinSetBlendshapeWeight(Item item, int bsInd, float weight) {
        mannequin.charEquip.SetBlendshapeWeight(item, bsInd, weight);
    }

    void LoadCharacterCustomizations(CharacterControl chara) {
        for(int i = 0; i < currCharCustomize.options.Count; ++i) {
            var o = currCharCustomize.options[i];
            if(string.IsNullOrEmpty(o.itemName)) {
                Debug.Log($"{o.optionType.ToString()} no itemname");
                continue;
            }
            var item = AssetRegistry.I.GetItemFromName(o.itemName);
            o.library.tempItemInd = GetItemIndex(item);

            var equipInst = chara.charEquip.EquipItem(item);

            if(chara == mannequin) {
                if(!equipInst)
                    Debug.LogWarning("no equip instance");
                o.library.itemInstances[o.library.tempItemInd] = equipInst;
            }
            if(o.colors != null)
                for(int j = 0; j < o.colors.Length; ++j) {
                    chara.charEquip.ChangeEquipColor(item, o.colors[j], j);
                }
            if(o.blendShapes != null)
                for(int k = 0; k < o.blendShapes.Length; ++k) {
                    chara.charEquip.SetBlendshapeWeight(item, k, o.blendShapes[k]);
                }
        }
    }

    public string GetRandomName() {
        if(givenNames.Count < 1) {
            return null;
        }
        int rand = Random.Range(0, givenNames.Count);
        string n = givenNames[rand];
        givenNames.RemoveAt(rand);
        return n;
    }

    public void RandomizeCustomer(GameObject go) {
        // RandomizeChar(go, customerColors, customerHats);
    }

    public void RandomizeCurrCustomize() {
        for(int i = 0; i < currCharCustomize.options.Count; ++i) {
            var o = currCharCustomize.options[i];
            o.library.tempItemInd = Random.Range(0, o.library.items.Count);

            for(int j = 0; j < o.blendShapes.Length; ++j) {
                o.blendShapes[j] = Random.Range(0, 101);
            }
        }
        LoadCharacterCustomizations(mannequin);
        UI.UpdateOptions(currCharCustomize);
    }

    public void Serialize(ref CharacterCustomizationData data) {
        data.customizeOptions = characterCustomizations;
    }
    public void Deserialize(ref CharacterCustomizationData data) {
        characterCustomizations = data.customizeOptions;
    }
}
}