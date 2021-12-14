using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

namespace m4k {
public class Cams : Singleton<Cams>
{
    public static Camera MainCam;
    public static CinemachineBrain MainBrain;
    public CamBase currCamBase, mainCamBase;
    public List<CamBase> camBases;

    public bool MainCamActive { get { return currCamBase == mainCamBase; }}

    Transform _mainCamTarget;

    protected override void Awake() {
        base.Awake();
        if(m_ShuttingDown) return;
        
        MainCam = Camera.main;
        MainBrain = MainCam.GetComponent<CinemachineBrain>();
    }

    void Start() {
        Init();
        ClearCamTarget();
    }
    
    public void Init() {
        SetCam(mainCamBase);
    }

    // public void SetLookTarget(Transform t, bool isCharacter = false) {
    //     rigFaceTarget = t;
    // }
    public void SetMainCamTarget(Transform t) {
        mainCamBase.transform.position = t.position;
        _mainCamTarget = t;
    }
    public void SetCamTarget(Transform t) {
        currCamBase.transform.position = t.position;
    }
    public void SetCamTarget(GameObject target) {
        SetCam("target");
        currCamBase.transform.position = target.transform.position;
    }
    public void ClearCamTarget() {
        SetCam("main");
    }

    public void PanCam(float x, float y) {
        currCamBase?.PanCamera(x, y);
    }
    public void SimpleZoom(float zoom) {
        currCamBase?.SimpleZoomCam(zoom);
    }

    public void ZoomCam(float z) {
        currCamBase?.ZoomCam(z);
    }
    
    public void RotateRig(float xIn, float yIn) {
        currCamBase?.RotateRig(xIn, yIn);
    }
    public void MoveRigRb(Vector3 input) {
        currCamBase?.MoveRigRb(input);
    }
    
    private void FixedUpdate() {
        if(!_mainCamTarget || !currCamBase || currCamBase != mainCamBase)
            return;
        
        mainCamBase.MoveRig(_mainCamTarget.position);
        
        // if global cam focus target is set, and is aimable cam
        // if(rigFaceTarget) { 
        //     var dir = rigFaceTarget.position - rigPos;
        //     dir.y = 0;
        //     var rot = Quaternion.Euler(dir);
        //     playerCamRig.rotation = Quaternion.Lerp(playerCamRig.rotation, rot, Time.deltaTime);
        // }
    }
    public void SetFarClip(float farClip = -1f) {
        if(!currCamBase) return;
        currCamBase.SetFarClip(farClip);
    }

    public void SetCamFollowLook(string key, Transform follow = null, Transform look = null, bool teleport = false) {
        CamBase cam = camBases.Find(x=>x.key == key);
        if(!cam) return;
        cam.AssignTargets(follow, look, teleport);
    }
    public void SetCamFollowLook(Transform follow = null, Transform look = null, bool teleport = false) {
        if(!currCamBase) return;
        currCamBase.AssignTargets(follow, look, teleport);
    }

    void ResetCams() {
        for(int i = 0; i < camBases.Count; ++i) {
            camBases[i].Disable();
        }
    }

    public void SetCam(string key) {
        CamBase cam = camBases.Find(x=>x.key == key);
        if(cam) {
            SetCam(cam);
        }
    }
    public void SetCam(CamBase cam) {
        ResetCams();
        cam.Enable();
        currCamBase = cam;
        Debug.Log($"Set cam to {currCamBase.key}");
    }
}
}