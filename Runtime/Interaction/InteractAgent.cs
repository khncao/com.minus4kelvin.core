using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using m4k.Characters;

namespace m4k.Interaction {
public class InteractAgent : MonoBehaviour
{
    public NavCharacterControl navChar;
    public LayerMask interactLayers;

    [System.NonSerialized]
    Collider[] hits = new Collider[10];

    private void Start() {
        if(!navChar) navChar = GetComponent<NavCharacterControl>();
        navChar.onArrive += OnArrive;
        navChar.onNewTarget += OnNewTarget;
    }

    private void OnDisable() {
        if(!navChar) return;
        navChar.onArrive -= OnArrive;
        navChar.onNewTarget -= OnNewTarget;
    }

    void OnNewTarget(Transform target) {
        OnArrive(target);
    }

    bool ProcessInteractions(Collider other) {
        bool interacted = false;
        var interactables = other.GetComponents<IInteractable>();
        for(int i = 0; i < interactables.Length; ++i) {
            if(interactables[i].Interact(gameObject)) 
                interacted = true;
        }
        return interacted;
    }

    void OnArrive(Transform target) {
        hits.Clear<Collider>();
        Physics.OverlapSphereNonAlloc(transform.position, 0.5f, hits, interactLayers, QueryTriggerInteraction.Collide);
        // bool interacted = false;

        for(int i = 0; i < hits.Length; ++i) {
            if(!target || hits[i] == null || hits[i].transform != target) 
                continue;

            ProcessInteractions(hits[i]);
            // if(ProcessInteractions(hits[i]))
            //     interacted = true;

            // process only first hit
            navChar.target = null;
            break;
        }
        // if(interacted) {
        //     navChar.StopAgent();
        // }
    }

    private void OnTriggerEnter(Collider other) {
        if(!navChar || !navChar.target || other.transform != navChar.target)
            return;

        if(ProcessInteractions(other)) {
            navChar.StopAgent();
        }
    }
}
}