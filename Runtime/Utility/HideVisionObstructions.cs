using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HideVisionObstructions : MonoBehaviour
{
    // public Material replacementMaterial;
    public int maxHits = 32;
    public float castDistance = 10f; 
    public float castRadius = 1f;
    public LayerMask hitLayers;
    // public float alphaTransitionTime = 0.2f;

    RaycastHit[] hits;
    // Dictionary<Renderer, List<Material>> rendererCache;
    HashSet<Renderer> currentHits, prevHits;
    int _hitCount, _prevHitCount;

    private void Start() {
        hits = new RaycastHit[maxHits];
        // rendererCache = new Dictionary<Renderer, List<Material>>();
        currentHits = new HashSet<Renderer>();
        prevHits = new HashSet<Renderer>();
    }

    void Update()
    {
        prevHits.Clear();
        foreach(var hit in currentHits) {
            prevHits.Add(hit);
        }
        currentHits.Clear();

        _hitCount = Physics.SphereCastNonAlloc(transform.position, castRadius, transform.forward, hits, castDistance, hitLayers, QueryTriggerInteraction.Ignore);

        for (int i = 0; i < _hitCount; ++i) {
            Renderer rend = hits[i].transform.GetComponent<Renderer>();

            if(!rend || currentHits.Contains(rend))
                continue;

            currentHits.Add(rend);
            rend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;

            // for(int j = 0; j < rend.materials.Length; ++j) {
            //     rend.materials[j] = Instantiate(replacementMaterial);
            // }
        }

        foreach(var hit in prevHits) {
            if(!currentHits.Contains(hit)) {
                hit.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            }
        }

        // to transparent
        // foreach(var e in rendererCache) {
        //     for(int i = 0; i < e.Value.Count; ++i) {
        //         var mat = e.Value[i];
        //         float amount = Time.deltaTime / alphaTransitionTime;
        //         var col = mat.color;
        //         col.a -= amount;
        //         mat.color = col;
        //     }
        //     if(e.Key.material.color.a >= 1f) {
        //         rendererCache.Remove(e.Key);
        //     }
        // }
        // rendererCache.TrimExcess();

        // from transparent
        // foreach(var e in rendererCache) {
        //     foreach(var mat in e.Value) {
        //         float amount = Time.deltaTime / alphaTransitionTime;
        //         var col = mat.color;
        //         col.a += amount;
        //         mat.color = col;
        //     }
        // }
    }
}
