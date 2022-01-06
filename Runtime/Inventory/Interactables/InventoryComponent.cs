using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using m4k.Characters;

namespace m4k.Items {
public class InventoryComponent : MonoBehaviour
{
    public int inventorySlots = 16;
    public int auxSlots = 1;
    public string saveIdOverride;

    public Inventory inventory { get {
        return _inventory != null ? _inventory : Intialize();
    }}
    [System.NonSerialized]
    Inventory _inventory;

    bool _initialized = false;

    private void Start() {
        Intialize();
    }

    private Inventory Intialize() {
        if(_initialized) 
            return _inventory;

        _initialized = true;
        string id = $"{gameObject.scene.name}/{transform.position.ToString()}";

        GuidComponent guidComponent;
        CharacterControl cc;

        if(!string.IsNullOrEmpty(saveIdOverride)) {
            id = saveIdOverride;
        }
        else if(TryGetComponent<GuidComponent>(out guidComponent)) {
            id = guidComponent.GetGuid().ToString();
        }
        else if(TryGetComponent<CharacterControl>(out cc)) {
            id = cc.character.name;
        }
        
        _inventory = InventoryManager.I.GetOrRegisterSavedInventory(id, inventorySlots, auxSlots, false, gameObject);
        
        return _inventory;
    }
}
}