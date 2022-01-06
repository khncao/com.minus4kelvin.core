using UnityEngine;

namespace m4k {
public static class BoundsExtensions {
    public static Vector3 GetRandomPoint(this Bounds bounds) {
        return new Vector3(
            Random.Range(bounds.min.x, bounds.max.x),
            Random.Range(bounds.min.y, bounds.max.y),
            Random.Range(bounds.min.z, bounds.max.z));
    }
    
    public static Vector3 GetRandomPointXZ(this Bounds bounds, float y) {
        return new Vector3(
            Random.Range(bounds.min.x, bounds.max.x),
            y,
            Random.Range(bounds.min.z, bounds.max.z));
    }
}
}