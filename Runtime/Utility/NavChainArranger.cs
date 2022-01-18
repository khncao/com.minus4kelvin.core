using System.Collections.Generic;
using UnityEngine;

namespace m4k {

public class NavChainArranger : MonoBehaviour {
    public Transform pivot;
    public int maxLength = 20;

    Dictionary<Transform, System.Action> targetLeaveReactMap = new Dictionary<Transform, System.Action>();
    LinkedList<Transform> movables = new LinkedList<Transform>();

    Transform GetSpot() {
        return movables.Last != null ? movables.Last.Value : pivot;
    }

    public bool HasRoom(int amount = 1) {
        return movables.Count + amount <= maxLength;
    }

    public Transform Join(Transform movable, System.Action onTargetLeave) {
        if(!HasRoom() || movables.Contains(movable)) {
            return null;
        }
        var spot = GetSpot();

        targetLeaveReactMap.Add(spot, onTargetLeave);

        movables.AddLast(movable);
        return null;
    }

    public void Leave(Transform movable) {
        if(!movables.Contains(movable))
            return;
        // invoke for onLeave for child
        if(targetLeaveReactMap.TryGetValue(movable, out var action)) {
            action?.Invoke();
        }
        // remove onLeave on parent if applicable
        var node = movables.Find(movable);
        if(node.Previous != null && targetLeaveReactMap.ContainsKey(node.Previous.Value)) {
            targetLeaveReactMap.Remove(node.Previous.Value);
        }

        movables.Remove(node);
    }
}
}