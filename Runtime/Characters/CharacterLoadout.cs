using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using m4k.Items;

// public enum EquipType { Body, Head, Hat, Hairstyle, Outfit, RHand, LHand, }

namespace m4k.Characters {
[System.Serializable]
public class CharacterEquipInstance {
    public string label;
    public ItemTag tag;
    public Item item;
    public GameObject instance;

    public Renderer rend;
    public Transform equipParent;

    public CharacterEquipInstance(ItemTag t) {
        this.tag = t;
        label = t.ToString();
    }
}

public class CharacterLoadout : MonoBehaviour
{
	public CharacterControl charControl;
    public Equipmentizer equipmentizer;
    public Transform loadoutParent;
    public Item rHandItem;
    public List<CharacterEquipInstance> charEquips;

    public CharacterEquipInstance rHand;

    bool initialized = false;
	

#if UNITY_EDITOR
    [ContextMenu("Setup equip slots")]
    void SetupSlots() {
		// find active equip gameobjects under loadout
		List<GameObject> prequippedInst = new List<GameObject>();
        if(loadoutParent && prequippedInst.Count < 1) {
            for(int i = 0; i < loadoutParent.childCount; ++i) {
                var go = loadoutParent.GetChild(i).gameObject;
                if(!go.activeInHierarchy) continue;
                prequippedInst.Add(go);
            }
        }

		// create CharacterEquipInstance for each equipTag type; populate with item, instance, and equipParent if applicable
		foreach(var t in ItemEquip.equipTags) {
			if(charEquips.Find(x=>x.tag == t) == null) {
				var e = new CharacterEquipInstance(t);
                
				// if(t == ItemTag.Holdable) {
				// 	e.equipParent = charAnim.rHandHold;
				// 	e.tag = ItemTag.Holdable;
				// }
				if(t == ItemTag.Hat) 
					e.equipParent = charControl.Head;

                charEquips.Add(e);
				if(!loadoutParent) continue;

                // var items = AssetRegistry.I.GetItemListByType(typeof(ItemEquip))
				var items = AssetRegistry.Database.items;
                for(int i = 0; i < items.Count; ++i) {
                    if(!items[i].HasTag(t) || !items[i].prefab) 
                        continue;
                    GameObject inst = prequippedInst.Find(x=>x.name == items[i].prefab.name);
                    if(inst) {
                        e.instance = inst;
                        if(e.item == null)
                            e.item = items[i];
                    }
                }
			}
		}
    }
#endif

    public void Start()
    {
        if(initialized) return;
		charControl = GetComponent<CharacterControl>();
		for(int i = 0; i < charEquips.Count; ++i) {
			if(charEquips[i].instance) {
				charEquips[i].rend = charEquips[i].instance.GetComponentInChildren<SkinnedMeshRenderer>();

				if(equipmentizer)
					equipmentizer.Equip(charEquips[i].rend as SkinnedMeshRenderer);
			}
		}
        rHand = charEquips.Find(x=>x.tag == ItemTag.Holdable);
        if(rHandItem) 
			EquipItem(rHandItem);
        initialized = true;
    }

	public CharacterEquipInstance GetSlotFromTag(ItemTag t) {
		return charEquips.Find(x=>x.tag == t);
	}
	public CharacterEquipInstance GetSlotFromItem(Item item) {
		var slot = charEquips.Find(x=>x.item == item);
		if(slot == null) {
			slot = charEquips.Find(x=>item.HasTag(x.tag));
		}
			
		if(slot == null)
			Debug.Log("slot not found: " + item);
		return slot;
	}
	public GameObject EquipItem(Item item) {
		var slot = GetSlotFromItem(item);
		return EquipItem(item, slot);
	}
	public GameObject EquipItem(Item newItem, CharacterEquipInstance slot) {
		if(slot.item && slot.item.displayName == newItem.displayName) {
			return slot.instance ? slot.instance : null;
		}
		if(slot.instance) 
			Destroy(slot.instance);

		slot.item = newItem;
		if(!newItem || !newItem.prefab) {
			Debug.LogWarning("Equip item null or no prefab");
			return null;
		}
		slot.instance = Instantiate(slot.item.prefab);
		if(slot.equipParent)
			slot.instance.transform.SetParent(slot.equipParent, false);
		else
			slot.instance.transform.SetParent(loadoutParent, false);

		if(slot == rHand) slot.instance.SetActive(false);
		slot.rend = slot.instance.GetComponentInChildren<Renderer>();
		SkinnedMeshRenderer skinned = slot.rend as SkinnedMeshRenderer;
		if(skinned) equipmentizer.Equip(skinned);
		
		return slot.instance;
	}
	public void ChangeEquipColor(Item item, Color color, int matInd) {
		var slot = GetSlotFromItem(item);
		if(!slot.rend) return;
		if(matInd < slot.rend.materials.Length)
			slot.rend.materials[matInd].color = color;
	}
	public void SetBlendshapeWeight(Item item, int bsInd, float weight) {
		var slot = GetSlotFromItem(item);
		if(!slot.rend) return;
		var skin = slot.rend as SkinnedMeshRenderer;
		if(bsInd < skin.sharedMesh.blendShapeCount)
			skin.SetBlendShapeWeight(bsInd, weight);
	}
}
}