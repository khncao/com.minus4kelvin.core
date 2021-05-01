

using UnityEngine;
using System.Collections.Generic;

// name, pic, customizations
// equipment

[CreateAssetMenu(fileName = "CharacterLoadoutPreset", menuName = "ScriptableObjects/CharacterLoadoutPreset", order = 0)]
public class CharacterLoadoutPreset : ScriptableObject {
    public CharacterLoadout loadout;
}

[System.Serializable]
public class CharacterLoadout {
    // public Item head, hair, body, outfit, hat, rhand, lhand;
    // [HideInInspector]
    // public Dictionary<ItemTag, Item> tagItemDict = new Dictionary<ItemTag, Item>();

    public CharacterLoadout() {
        // equips = new List<CharacterEquip>() { head, hair, body, outfit, hat, rhand, lhand };
    }

    // public CharacterEquip GetSlot(Item item) {
    //     var slot = equips.Find(x=>item.HasTag(x.tag));
    //     if(slot == null)
    //         Debug.LogWarning("no slot");
    //     return slot;
    // }

    // public CharacterEquip GetSlot(ItemTag tag) {
    //     var slot = equips.Find(x=>x.tag == tag);
    //     if(slot == null) 
    //         Debug.LogWarning("no slot");
    //     return slot;
    // }
}

