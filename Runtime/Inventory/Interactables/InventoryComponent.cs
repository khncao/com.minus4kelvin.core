using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using m4k.Characters;

namespace m4k.Items {
public class InventoryComponent : MonoBehaviour
{
    public int inventorySlots = 16;
    [Header("Will register as saved inventory if saveId not empty,\nGuidComponent on same gameObject,\nor CharacterControl on same gameObject in that order")]
    public string saveId;

    public string id { get; set; }

    public Inventory inventory { 
        get {
            return _inventory != null ? _inventory : Intialize();
        }
        set {
            _inventory = value;
        }
    }
    [System.NonSerialized]
    Inventory _inventory;

    // private void Start() {
    //     Intialize();
    // }

    private Inventory Intialize() {
        if(_inventory != null) 
            return _inventory;

        if(!string.IsNullOrEmpty(saveId)) {
            id = saveId;
        }
        else if(TryGetComponent<GuidComponent>(out GuidComponent guidComponent)) {
            id = guidComponent.GetGuid().ToString();
        }
        else if(TryGetComponent<CharacterControl>(out CharacterControl cc)) {
            id = cc.character.name;
        }
        
        if(!string.IsNullOrEmpty(id))
            _inventory = InventoryManager.I.GetOrRegisterSavedInventory(id, inventorySlots, gameObject);
        else
            _inventory = new Inventory(inventorySlots);
        
        return _inventory;
    }
}
}