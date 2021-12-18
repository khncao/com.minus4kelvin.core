using UnityEngine;
using System;

namespace m4k {
/// <summary>
/// Common interface for directable navigation agents. Used by AI, tasks, etc.
/// </summary>
public interface IMoveTargetable {
    Transform Target { get; }
    bool IsMoving { get; }
    void SetTarget(Transform target, bool move);
    void SetTarget(Vector3 position, bool move);
    void Move();
    void Stop();
    void Pause();
    void Resume();
    event Action OnArrive;
    event Action OnNewTarget;
}
}