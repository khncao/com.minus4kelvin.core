using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace m4k.Interaction {
public class InteractAgent : MonoBehaviour
{
    public INavMovable movable;
    public LayerMask interactLayers;

    [System.NonSerialized]
    Collider[] hits = new Collider[10];

    private void Start() {
        if(movable == null) movable = GetComponent<INavMovable>();
        movable.OnArrive += OnArrive;
        movable.OnNewTarget += OnNewTarget;
    }

    private void OnDisable() {
        if(movable == null) return;
        movable.OnArrive -= OnArrive;
        movable.OnNewTarget -= OnNewTarget;
    }

    void OnNewTarget() {
        OnArrive();
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

    void OnArrive() {
        hits.Clear<Collider>();
        Physics.OverlapSphereNonAlloc(transform.position, 1f, hits, interactLayers, QueryTriggerInteraction.Collide);

        for(int i = 0; i < hits.Length; ++i) {
            if(!movable.Target || hits[i] == null || hits[i].transform != movable.Target) 
                continue;

            ProcessInteractions(hits[i]);

            movable.Stop();
            break;
        }
    }

    private void OnTriggerEnter(Collider other) {
        if(movable == null || !movable.Target || other.transform != movable.Target)
            return;

        if(ProcessInteractions(other)) {
            movable.Stop();
        }
    }
}
}