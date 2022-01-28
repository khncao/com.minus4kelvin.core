using UnityEngine;

namespace m4k {
public interface IToolInteract {
    void StartInteract(string toolId, Transform target);
    void StopInteract();
}
}