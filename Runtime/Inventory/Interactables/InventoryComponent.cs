using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using m4k.Characters;

namespace m4k.Items {
public class InventoryComponent : MonoBehaviour
{
    public int inventorySlots = 16;
    public int auxSlots = 1;
    [Header("Will register as saved inventory if saveId not empty, GuidComponent on same gameObject, or CharacterControl on same gameObject")]
    public string saveId;

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

        // string id = $"{gameObject.scene.name}/{transform.position.ToString()}";
        string id = "";

        GuidComponent guidComponent;
        CharacterControl cc;

        if(!string.IsNullOrEmpty(saveId)) {
            id = saveId;
        }
        else if(TryGetComponent<GuidComponent>(out guidComponent)) {
            id = guidComponent.GetGuid().ToString();
        }
        else if(TryGetComponent<CharacterControl>(out cc)) {
            id = cc.character.name;
        }
        
        if(!string.IsNullOrEmpty(id))
            _inventory = InventoryManager.I.GetOrRegisterSavedInventory(id, inventorySlots, auxSlots, false, gameObject);
        else
            _inventory = new Inventory(inventorySlots, auxSlots);
        
        return _inventory;
    }
}
}