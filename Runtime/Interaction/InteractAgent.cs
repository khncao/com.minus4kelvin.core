using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using m4k.Characters;

namespace m4k.Interaction {
public class InteractAgent : MonoBehaviour
{
    public CharacterControl cc;
    public LayerMask interactLayers;

    Collider[] hits = new Collider[20];

    private void Start() {
        if(!cc) cc = GetComponent<CharacterControl>();
        // cc.navChar.onArrive += OnArrive;
    }

    // void OnArrive(Transform target) {
    //     Physics.OverlapSphereNonAlloc(transform.position, 1f, hits, interactLayers, QueryTriggerInteraction.Collide);

    //     for(int i = 0; i < hits.Length; ++i) {
    //         if(!target || hits[i] == null || hits[i].transform != target) 
    //             continue;

    //         // Interactable interactable;
    //         // hits[i].TryGetComponent<Interactable>(out interactable);
    //         DestroyZone destroyZone;
    //         hits[i].TryGetComponent<DestroyZone>(out destroyZone);

    //         // if(interactable) {
    //         //     interactable.Interact();
    //         // }
    //         // else 
    //         if(destroyZone) {
    //             GetComponent<IDestroyable>().Destroy();
    //         }
    //     }
    // }

    private void OnTriggerStay(Collider other) {
        if(!cc.navChar.target || other.transform != cc.navChar.target)
            return;

        Interactable interactable;
        other.TryGetComponent<Interactable>(out interactable);
        DestroyZone destroyZone;
        other.TryGetComponent<DestroyZone>(out destroyZone);
    //     SeatController seat;
    //     target.TryGetComponent<SeatController>(out seat);
        if(interactable) {
            interactable.Interact();
            cc.navChar.StopAgent();
        }
        else if(destroyZone) {
            GetComponent<IDestroyable>().Destroy();
        }
    //     else if(seat && !seat.occupied) {
    //         cc.charAnim.Sit(seat);
    //         agent.nextPosition = transform.position;
    //         agent.updatePosition = false;
    //         StopAgent();
    //     }
        cc.navChar.target = null;
    }
}
}