using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// using UnityEngine.Animations.Rigging;

namespace m4k.Characters {
public class CharacterIK : MonoBehaviour
{
    public bool enableIk;//, enableHandIk;
    public Transform headTarget, hipFront;
    public float ikTime = 0.3f;
    [Range(0,1f)]
    public float bodyWeight = 0.2f, headWeight = 0.7f, eyeWeight = 0.2f, clampWeight = 0.5f;
    public bool debug;
    // public TwoBoneIKConstraint rHandIk;
    
    Transform lookTarget, rHandTarget, lHandTarget;
    Transform faceTarget;
    float ikTimer, ikWeight;
    [SerializeField]
    float lookTimer, faceTimer, rHandTimer;
    Animator anim;
    bool facing, looking, rHandIkBool;
    Transform prevLookTarget, prevRHandTarget;

    private void Awake() {
        anim = GetComponentInChildren<Animator>();
    }

    public void EnableIk() {
        enableIk = true;
    }
    public void DisableIk() {
        enableIk = false;
    }

    public void SetLook(Transform target) {
        // if(target) {
            prevLookTarget = lookTarget;
            lookTarget = target;
        // }
        looking = target;
    }
    // public void SetFacing(Transform target) {
    //     // if(faceTarget)  {
    //         faceTarget = target;
    //     // }
    //     facing = target;
    // }
    public void SetRightHandTarget(Transform target) {
        // if(target)
            prevRHandTarget = rHandTarget;
            rHandTarget = target;
            // if(!prevRHandTarget) prevRHandTarget = rHandTarget;
        rHandIkBool = target;
    }
    // private void LateUpdate() {
    // //     // lookik
    // //     if(looking) {
    // //         anim.SetLookAtPosition(lookTarget.position);
    // //         if(lookTimer < ikTime) 
    // //             lookTimer += Time.deltaTime;
    // //     }
    // //     else if(lookTimer > 0 && prevLookTarget) {
    // //         anim.SetLookAtPosition(prevLookTarget.position);
    // //         lookTimer -= Time.deltaTime;
    // //     }

    //     // facing
    //     if(nav && !nav.isPathing && facing) {
    //         Vector3 dir = faceTarget.position - transform.position;
    //         // dir.x = 0;
    //         if(faceTimer < ikTime) 
    //             faceTimer += Time.deltaTime;
                
    //         var rot = Quaternion.LookRotation(dir);
    //         transform.rotation = Quaternion.Lerp(transform.rotation, rot, faceTimer / ikTime);
    //     }
        // if(!rHandIk) return;

        // var rHandData = rHandIk.data;
        // if(rHandIkBool) {
        //     rHandData.target.position = rHandTarget.position;
        //     rHandData.target.rotation = rHandTarget.rotation;
        //     if(rHandTimer < ikTime) {
        //         rHandTimer += Time.deltaTime;
        //     }
        // }
        // else if(rHandTimer > 0 && prevRHandTarget) {
        //     rHandData.target.position = prevRHandTarget.position;
        //     rHandData.target.rotation = prevRHandTarget.rotation;
        //     rHandTimer -= Time.deltaTime;
        // }

        // rHandData.targetPositionWeight = rHandTimer / ikTime;
        // rHandData.targetPositionWeight = rHandTimer / ikTime;
        
        // rHandIk.data = rHandData;
    // }

    private void OnAnimatorIK(int layerIndex) {
        if(!enableIk) return;

        // lookik
        if(looking) {
            anim.SetLookAtPosition(lookTarget.position);
            if(lookTimer < ikTime) 
                lookTimer += Time.deltaTime;
        }
        else if(lookTimer > 0 && prevLookTarget) {
            anim.SetLookAtPosition(prevLookTarget.position);
            lookTimer -= Time.deltaTime;
        }
        
        anim.SetLookAtWeight(lookTimer / ikTime, bodyWeight, headWeight, eyeWeight, clampWeight);

        // facing
        // if(nav && !nav.isPathing && facing) {
        //     Vector3 dir = faceTarget.position - transform.position;
        //     // dir.x = 0;
        //     if(faceTimer < ikTime) 
        //         faceTimer += Time.deltaTime;
                
        //     var rot = Quaternion.LookRotation(dir);
        //     transform.rotation = Quaternion.Lerp(transform.rotation, rot, faceTimer / ikTime);
        // }

        // if(!rHandIk) return;
        // if(rHandIkBool) {
        //     anim.SetIKPosition(AvatarIKGoal.RightHand, rHandTarget.position);
        //     anim.SetIKRotation(AvatarIKGoal.RightHand, rHandTarget.rotation);
        //     if(rHandTimer < ikTime) {
        //         rHandTimer += Time.deltaTime;
        //     }
        // }
        // else if(rHandTimer > 0 && prevRHandTarget) {
        //     anim.SetIKPosition(AvatarIKGoal.RightHand, prevRHandTarget.position);
        //     anim.SetIKRotation(AvatarIKGoal.RightHand, prevRHandTarget.rotation);
        //     rHandTimer -= Time.deltaTime;
        // }
        // anim.SetIKPositionWeight(AvatarIKGoal.RightHand, rHandTimer / ikTime);
        // anim.SetIKRotationWeight(AvatarIKGoal.RightHand, rHandTimer / ikTime);

        // if(!rHandIk) return;
        // var rHandData = rHandIk.data;
        // if(rHandIkBool) {
        //     rHandData.target.position = rHandTarget.position;
        //     rHandData.target.rotation = rHandTarget.rotation;
        //     if(rHandTimer < ikTime) {
        //         rHandTimer += Time.deltaTime;
        //     }
        // }
        // else if(rHandTimer > 0) {
        //     rHandData.target.position = prevRHandTarget.position;
        //     rHandData.target.rotation = prevRHandTarget.rotation;
        //     rHandTimer -= Time.deltaTime;
        // }
        // rHandData.targetPositionWeight = rHandTimer / ikTime;
        // rHandData.targetPositionWeight = rHandTimer / ikTime;
        // rHandIk.data = rHandData;
        
        // feetik
    }
}
}