using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace m4k.Interaction {
[RequireComponent(typeof(Collider))]
public class TriggerInteraction : MonoBehaviour
{
    [System.Serializable]
    public class UnityEvents {
        public UnityEvent onTriggerEnter, onTriggerExit;
    }
    public UnityEvents events;
    public Conditions onEnterConds, onExitConds;
    Collider col;

    private void Start() {
        col = GetComponent<Collider>(); 
        col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other) {
        if(!onEnterConds.CheckCompleteReqs())
            return;
        events.onTriggerEnter?.Invoke();
    }

    private void OnTriggerExit(Collider other) {
        if(!onExitConds.CheckCompleteReqs())
            return;
        events.onTriggerExit?.Invoke();
    }
}
}