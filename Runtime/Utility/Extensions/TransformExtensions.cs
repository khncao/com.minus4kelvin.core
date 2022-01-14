using System.Collections.Generic;
using UnityEngine;

namespace m4k {
public static class TransformExtensions {

    /// <summary>
    /// Closest of list of Component castable types to a transform
    /// </summary>
    /// <param name="refTransform">Reference point transform</param>
    /// <param name="list">List Component for comparison</param>
    /// <param name="closest">Result closest</param>
    /// <typeparam name="T"></typeparam>
    public static void GetClosest<T>(this Transform refTransform, IList<T> list, out T closest)
    {
        closest = default(T);

        if(list == null || list.Count == 0) {
            Debug.LogError("Invalid list");
            return;
        }
        // casting Component instead of limiting where T : Component for interface compatibility
        if(!(list[0] is Component)) {
            Debug.LogError("T is not Component");
            return;
        }
        float closestDist = Mathf.Infinity;
        foreach(var i in list) {
            var m = i as Component;
            var sqrDist = (m.transform.position - refTransform.position).sqrMagnitude;
            if(sqrDist < closestDist) {
                closestDist = sqrDist;
                closest = i;
            }
        }
    }
}
}