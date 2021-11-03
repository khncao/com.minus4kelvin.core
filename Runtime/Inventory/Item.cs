using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
// using UnityEngine.AddressableAssets;
// using UnityEngine.ResourceManagement;

namespace m4k.InventorySystem {

public enum ItemType {
    Item = 0, Buildable = 10, Character = 40, Recipe = 50, Equip = 60, Achievement = 100,
}
public enum ItemTag {
    Consumable = 0, Drink = 1, Food = 2,
    Floor = 10, Light = 11, Table = 12, Seat = 13, Prop = 14, Zone = 15, Wall = 16, 
    Bar = 50, Kitchen = 51, Brew = 52, Foundry = 53, Stonemason = 54, Sawmill = 55,
    Hat = 60, Hairstyle = 61, Head = 62, Outfit = 63, Body = 64, Holdable = 65, RightHand = 66, LeftHand = 67,
    
}

[System.Serializable]
public class ItemData {
    public string itemName;
    public int amount;
    public ItemData(string n, int a) {
        itemName = n;
        amount = a;
    }
}

[System.Serializable]
public class ItemInstance {
    public string ItemName { get { 
        if(item) {
            if(string.IsNullOrEmpty(item.itemName))
                return item.name;
            else 
                return item.itemName;
        }
        return "";
    }}
    public Item item;
    public int amount = 1;

    [System.NonSerialized]
    public System.Action onChange;

    public ItemInstance(Item i, int a) {
        item = i;
        amount = a;
    }
}

[CreateAssetMenu(menuName="ScriptableObjects/Items/Item")]
[System.Serializable]
public class Item : ScriptableObject
{
    public string itemName;
    public string description;
    public Sprite itemIcon;
    public ItemType itemType;
    public List<ItemTag> itemTags;
    public GameObject prefab;
    // public AssetReference prefabRef;
    public Conditions conditions;
    public int maxAmount = 1;
    public float value;
    public string guid;

    // public GameObject prefab { get { return prefabRef.Asset as GameObject; }}
    
    HashSet<ItemTag> tagHash;

    // public void LoadAssets() {
    //     prefabRef.LoadAssetAsync<GameObject>();
    // }
    // public void ReleaseAssets() {
    //     prefabRef.ReleaseAsset();
    // }

    public bool HasTag(ItemTag tag) {
        if(tagHash == null) {
            tagHash = new HashSet<ItemTag>();
            foreach(var t in itemTags)
                tagHash.Add(t);
        }

        return tagHash.Contains(tag);
    }

    public virtual void SingleClick(ItemSlot slot) {
        
    }
    public virtual void DoubleClick(ItemSlot slot) {

    }

    public virtual void AddToInventory(int amount, bool notify) {
        InventoryManager.I.mainInventory.AddItemAmount(this, amount, notify);
    }

    public virtual void Copy(Item item) {
        name = item.name;
        itemName = item.itemName;
        description = item.description;
        prefab = item.prefab;
        // prefabRef = item.prefabRef;
        conditions = item.conditions;
        maxAmount = item.maxAmount;
        value = item.value;
        guid = item.guid;
        itemIcon = item.itemIcon;
        itemTags = item.itemTags;
        itemType = item.itemType;
    }
}

// public interface ISlottable {
//     public string slotName { get; }
//     public string slotDescription { get; }
//     public Sprite slotSprite { get; }
// }
}