using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialsReplacer : MonoBehaviour
{
    Renderer[] renderers;
    Material[][] origMats;
    Dictionary<Material, Material[][]> replacementMatsDict = new Dictionary<Material, Material[][]>();
    bool initialized;
    int layerMask;

    void Start()
    {
        Initialize();
        layerMask = LayerMask.NameToLayer("TransparentFX");
    }
    void Initialize() {
        if(initialized) return;
        renderers = GetComponentsInChildren<Renderer>();
        origMats = new Material[renderers.Length][];
        for(int i = 0; i < renderers.Length; ++i) {
            origMats[i] = renderers[i].materials;
        }
        initialized = true;
    }
    public void RegisterReplacementMat(Material[] mats) {
        Initialize();
        for(int i = 0; i < mats.Length; ++i) {
            if(replacementMatsDict.ContainsKey(mats[i]))
                continue;
            replacementMatsDict[mats[i]] = new Material[renderers.Length][];
            
            for(int j = 0; j < renderers.Length; ++j) {
                Material[] m = renderers[j].materials;
                replacementMatsDict[mats[i]][j] = new Material[m.Length];

                for(int k = 0; k < m.Length; ++k) {
                    replacementMatsDict[mats[i]][j][k] = mats[i];
                }
            }
        }
    }

    public void Replace(Material material) {
        if(!replacementMatsDict.ContainsKey(material)) {
            Debug.LogError("replacement material doesnt exist");
            return;
        }
        for(int i = 0; i < renderers.Length; ++i) {
            if(renderers[i].gameObject.layer == layerMask) 
                continue;
            else
                renderers[i].materials = replacementMatsDict[material][i];
        }
    }
    public void ChangeColor(Color color) {
        for(int i = 0; i < renderers.Length; ++i) {
            for(int j = 0; j < renderers[i].materials.Length; ++j) {
                renderers[i].materials[j].color = color;
            }
        }
    }

    public void Restore() {
        for(int i = 0; i < renderers.Length; ++i) {
            renderers[i].materials = origMats[i];
        }
    }
}
