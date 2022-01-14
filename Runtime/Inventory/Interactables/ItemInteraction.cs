using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using m4k.Interaction;
// using Uween;

namespace m4k.Items {
[RequireComponent(typeof(Interactable))]
public class ItemInteraction : MonoBehaviour
{
    public ItemItem item;
    public ItemArranger arranger;
    public ParticleSystem particles;

    GameObject instance;
    // public AnimationClip animationClip;
    Interactable interactable;

    private void Start() {
        interactable = GetComponent<Interactable>();
        interactable?.events.onInteract.AddListener(Interact);

        if(!item || !item.prefab) {
            Debug.Log("ItemInteraction has no item or no item prefab");
            return;
        }
        if(arranger)
            arranger.UpdateItems(item);

        instance = Instantiate(item.prefab, transform);
        gameObject.name = item && !string.IsNullOrEmpty(item.displayName) ? item.displayName : item.prefab.name;
        if(interactable)
            interactable.description = gameObject.name;
    }
    public void Interact() {
        // if(instance) {
            // Feedback.I.AssignText("It's a " + itemName);
            // Feedback.I.SendLine("It appears to be " + itemName);
        // }
        if(item) {
            item.AddToInventory(1, true);
            
            // if(spawnItem)
            //     TweenSXYZ.Add(gameObject, 0.5f, Vector3.one * 0.2f).Then(()=>Destroy(transform.parent.gameObject));
        }
        if(particles) {
            particles.Stop();
            particles.Clear();
        }
        // if(animationClip && interactable) {
        //     var anim = interactable.otherCol.GetComponentInChildren<Animator>();
        // }
    }
}
}