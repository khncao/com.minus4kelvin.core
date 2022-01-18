using System.Collections.Generic;
using UnityEngine;

namespace m4k {
// TODO: mesh vertex input with variable density
public class ObjectArranger : MonoBehaviour {
    public List<Transform> points;
    public Transform pivot;
    public int rows = 8;
    public int cols = 8;
    public int height = 1;
    public Vector3 spacing = Vector3.one;

    [Header("Leave none to not use physics hit placement")]
    public LayerMask hitLayers;
    [Tooltip("Cast down from; max ray distance x2 of")]
    public float heightError;

    [Header("Optional looping prefab spawn on points")]
    public List<GameObject> prefabs;

    List<GameObject> _prefabInstances = new List<GameObject>();
    
    int allottedPoints;
    int row, col, hei;
    Vector3 cursor;

    Stack<int> spots = new Stack<int>();
    Dictionary<Transform, int> transformIndexMap = new Dictionary<Transform, int>();

    private void Start() {
        if(!pivot) Debug.LogWarning("No pivot on transform arranger");
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected() {
        for(int i = 0; i < points.Count; ++i) {
            Gizmos.DrawSphere(points[i].position, 0.1f);
        }
    }
#endif

    void Process() {
        allottedPoints = rows * cols * height;
        cursor = pivot.position;

        if(allottedPoints == 0) {
            Debug.LogError("Rows, cols, height cannot be 0");
        }

        if(points.Count > allottedPoints) {
            Debug.Log("More points than 3D matrix, excess will be truncated");
        }

        if(points.Count < allottedPoints) {
            int amount = allottedPoints - points.Count;
            for(int i = 0; i < amount; ++i) {
                AddPoint(i);
            }
        }
        transformIndexMap.Clear();
        spots.Clear();
        for(int i = points.Count - 1; i >= 0; --i) {
            spots.Push(i);
        }
    }

    void AddPoint(int i) {
        var go = new GameObject(i.ToString());
        go.SetActive(false);
        go.transform.SetParent(pivot);
        points.Add(go.transform);
        
        var prefab = GetPrefab();
        if(prefab) {
            var instance = Instantiate(prefab, go.transform, false);
            _prefabInstances.Add(instance);
        }
    }

    int prefabIdx = 0;
    GameObject GetPrefab() {
        if(prefabs.Count < 1) return null;
        GameObject prefab = prefabs[prefabIdx];
        prefabIdx = (prefabIdx + 1) % prefabs.Count;
        return prefab;
    }

    [ContextMenu("Reset All")]
    public void Reset() {
        DestroyAllPoints();
        // for(int i = 0; i < pivot.childCount; ++i) {
        //     DestroyImmediate(pivot.GetChild(i).gameObject);
        // }
        points = new List<Transform>();
        _prefabInstances = new List<GameObject>();
        ArrangeAll();
    }

    [ContextMenu("Arrange All")]
    public void ArrangeAll() {
        Process();
        row = col = hei = 0;
        cursor = pivot.position;

        for(int i = 0; i < points.Count; ++i) {
            if(i < allottedPoints) {
                points[i].gameObject.SetActive(true);
                Arrange(points[i]);
            }
            else {
                points[i].gameObject.SetActive(false);
            }
        }
    }

    [ContextMenu("Destroy All Points")]
    public void DestroyAllPoints() {
        foreach(var p in points) {
            if(!p) continue;
            DestroyImmediate(p.gameObject);
        }
    }

    public void Arrange(Transform pt) {
        pt.localPosition = cursor;

        RaycastHit hit;
        if(Physics.Raycast(pt.position + pt.up * heightError, -transform.up, out hit, heightError * 2f, hitLayers, QueryTriggerInteraction.Ignore)) {
            pt.position = hit.point;
        }

        float z = spacing.z, x = 0f, y = 0f;

        row++;
        if(row == rows) {
            z = 0f;
            cursor.z = 0f;
            x = spacing.x;
            row = 0;

            col++;
            if(col == cols) {
                x = 0f;
                cursor.x = 0f;
                y = spacing.y;
                col = 0;

                hei++;
            }
        }
        cursor += new Vector3(x, y, z);
    }

    public bool HasRoom(int amount = 1) {
        return transformIndexMap.Count + amount <= points.Count;
    }

    public Transform GetSpot() {
        int spotIndex = spots.Pop();
        transformIndexMap.Add(points[spotIndex], spotIndex);
        return points[spotIndex];
    }

    public void FreeSpot(Transform spot) {
        if(!transformIndexMap.TryGetValue(spot, out int spotIndex)) {
            Debug.LogWarning("Spot not found registered");
        }
        spots.Push(spotIndex);
        transformIndexMap.Remove(spot);
    }
}
}