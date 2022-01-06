// Reference: https://gist.github.com/TwoTenPvP/29684118fb37d1de946aee03307d80c1

using System.Collections.Generic;
using UnityEngine;

namespace m4k {
public class Equipmentizer : MonoBehaviour
{
    public SkinnedMeshRenderer TargetMeshRenderer;
    public Transform TargetRig;
    
    Dictionary<string, Transform> boneMap = new Dictionary<string, Transform>();

    void Awake()
    {
        if(!TargetMeshRenderer) {
            Debug.LogWarning($"No target mesh renderer on {transform.parent.name}");
            return;
        }
        if(boneMap.Count < 1)
            foreach (Transform bone in TargetMeshRenderer.bones)
                boneMap[bone.gameObject.name] = bone;
    }

    Transform[] TransferBones(SkinnedMeshRenderer equip) {
        Awake();
        Transform[] newBones = new Transform[equip.bones.Length];
        for (int i = 0; i < equip.bones.Length; ++i)
        {
            if (!boneMap.TryGetValue(equip.bones[i].name, out newBones[i]))
            {
                if(equip.bones[i].name == equip.rootBone.name) 
                    break;
                Debug.Log("Unable to map bone \"" + equip.bones[i].name + "\" to target skeleton.");
                break;
            }
        }
        return newBones;
    }

    public void Equip(SkinnedMeshRenderer equip) {
        if(!TargetMeshRenderer || !equip || equip.bones == null) 
            return;

        equip.rootBone = TargetMeshRenderer.rootBone;
        equip.bones = TransferBones(equip);
        // equip.bones = TargetMeshRenderer.bones;

        // Destroy unneeded rig from merging mesh
        if(equip.rootBone.parent != TargetRig)
            Destroy(equip.rootBone.parent.gameObject);
    }
}
}