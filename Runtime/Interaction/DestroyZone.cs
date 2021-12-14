using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace m4k.Interaction {
/// <summary>
/// Process IDestroyables
/// </summary>
public class DestroyZone : MonoBehaviour, IInteractable
{
    public bool Interact(GameObject go) {
        IDestroyable destroyable;
        go.TryGetComponent<IDestroyable>(out destroyable);
        if(destroyable != null) {
            destroyable.Destroy();
            return true;
        }
        else
            return false;
    }
    // public string collisionTag;
    // private void OnTriggerStay(Collider other) {
    //     var destroyable = other.GetComponent<IDestroyable>();
    //     if(destroyable == null) return;
    //     destroyable.Destroy();
            
    //     // Destroy(other.transform.root.gameObject);
    // }
}
}