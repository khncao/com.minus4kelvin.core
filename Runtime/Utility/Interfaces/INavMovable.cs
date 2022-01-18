using UnityEngine;
using System;

namespace m4k {
/// <summary>
/// Common interface for directable navigation agents. Used by AI, tasks, etc.
/// </summary>
public interface INavMovable {
    Transform Target { get; }
    bool IsMoving { get; }
    void SetTarget(Transform target);
    void SetTarget(Vector3 position);
    void SetFaceTarget(Transform target);
    void Move();
    void Stop();
    void Pause();
    void Resume();
    event Action OnArrive;
    event Action OnNewTarget;
}
}