using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using m4k.InventorySystem;
// using Uween;

namespace m4k.Interaction {
[RequireComponent(typeof(Interactable))]
public class ItemInteraction : MonoBehaviour
{
    public Item item;
    public GameObject prefab;
    public ItemArranger arranger;
    public bool spawnItem;
    GameObject instance;
    // public AnimationClip animationClip;
    Interactable interactable;
    string itemName;

    private void Start() {
        interactable = GetComponent<Interactable>();
        interactable?.events.onInteract.AddListener(Interact);

        if(item)
            prefab = item.prefab;
        if(arranger && spawnItem)
            arranger.UpdateItems(item);
        else if(prefab && spawnItem)
            instance = Instantiate(prefab, transform);
        itemName = item ? item.itemName : prefab.name;

    }
    public void Interact() {
        // if(instance) {
            // Feedback.I.AssignText("It's a " + itemName);
            // Feedback.I.SendLine("It appears to be " + itemName);
        // }
        if(item) {
            // InventoryManager.I.mainInventory.AddItemAmount(item, 1, true);
            item.AddToInventory(1, true);
            // if(interactable.destroyOnInteract)
            //     Destroy(gameObject);
            
            // if(spawnItem)
            //     TweenSXYZ.Add(gameObject, 0.5f, Vector3.one * 0.2f).Then(()=>Destroy(transform.parent.gameObject));
        }
        // if(animationClip && interactable) {
        //     var anim = interactable.otherCol.GetComponentInChildren<Animator>();
        // }
    }
}
}