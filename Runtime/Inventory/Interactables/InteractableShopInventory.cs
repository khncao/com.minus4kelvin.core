using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace m4k.Items {
public class InteractableShopInventory : MonoBehaviour
{
    public bool itemShop, characterShop;
    public ItemTierTable itemSpawnTable;
    public int shopItemsTier = 0;

    [System.NonSerialized]
    Inventory inventory;

    private void Start() {
        if(!itemShop && !characterShop)
            Debug.LogError("InventoryType not as expected");
    }

    public void Interact() {
        if(itemSpawnTable && inventory == null) {
            inventory = itemSpawnTable.GetItemsUpToTier(new Inventory(16), shopItemsTier);
        }

        if(inventory == null) {
            Debug.LogError("Inventory get error");
            return;
        }

        if(itemShop)
            InventoryManager.I.ToggleShop(inventory);
        else if(characterShop)
            InventoryManager.I.ToggleCharShop(inventory);
    }
}
}