using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace m4k {
[RequireComponent(typeof(Collider))]
public class TriggerEvents : MonoBehaviour
{
    public System.Action<Collider> onTriggerEnter, onTriggerStay, onTriggerExit;

    private void Start() {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other) {
        onTriggerEnter?.Invoke(other);
    }

    private void OnTriggerStay(Collider other) {
        onTriggerStay?.Invoke(other);
    }

    private void OnTriggerExit(Collider other) {
        onTriggerExit?.Invoke(other);
    }
}
}