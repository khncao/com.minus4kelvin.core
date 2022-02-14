using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace m4k {
[RequireComponent(typeof(Collider))]
public class CollisionEvents : MonoBehaviour
{
    public System.Action<Collision> onCollisionEnter, onCollisionStay, onCollisionExit;

    private void OnCollisionEnter(Collision other) {
        onCollisionEnter?.Invoke(other);
    }

    private void OnCollisionStay(Collision other) {
        onCollisionStay?.Invoke(other);
    }

    private void OnCollisionExit(Collision other) {
        onCollisionExit?.Invoke(other);
    }
}
}