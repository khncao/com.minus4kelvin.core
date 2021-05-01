    using System.Collections.Generic;
    using UnityEngine;
     
    public class Equipmentizer : MonoBehaviour
    {
        public SkinnedMeshRenderer TargetMeshRenderer;
        public Transform TargetRig;
        
        Dictionary<string, Transform> boneMap = new Dictionary<string, Transform>();
        void Awake()
        {
            foreach (Transform bone in TargetMeshRenderer.bones)
                boneMap[bone.gameObject.name] = bone;
        }

        public void Equip(SkinnedMeshRenderer equip) {
            // Transform[] newBones = new Transform[equip.bones.Length];
            if(!equip || equip.bones == null) return;
            for (int i = 0; i < equip.bones.Length; ++i)
            {
                // GameObject bone = equip.bones[i].gameObject;
                Transform bone;
                // if (!boneMap.TryGetValue(bone.name, out newBones[i]))
                if(!boneMap.TryGetValue(equip.bones[i].name, out bone))
                {
                    Debug.Log("Unable to map bone \"" + bone.name + "\" to target skeleton.");
                    break;
                }
            }
            if(equip.rootBone.parent != TargetRig)
                Destroy(equip.rootBone.parent.gameObject);
            equip.rootBone = TargetMeshRenderer.rootBone;
            // equip.bones = newBones;
            equip.bones = TargetMeshRenderer.bones;
        }
    }
