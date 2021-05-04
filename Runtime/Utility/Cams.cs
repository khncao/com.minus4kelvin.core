using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using m4k.Characters;

namespace m4k {
public enum CamTypes {
    PlayerCam, ManagementCam, FirstPersonCam
}
[System.Serializable]
public class CamInstance {
    public string key;
    public Transform pivot;
    public CinemachineVirtualCamera vcam;
    public float followDamp = 0.1f;
    public float offsetMult = 0.2f;
    public Transform followTarget;
    public Transform lookTarget;

    public Vector3 OffsetStep { get { return initOffset * offsetMult; }}

    [HideInInspector]
    public Vector3 initOffset;
}

public class Cams : Singleton<Cams>
{
    public static Camera MainCam;
    public static CinemachineBrain MainBrain;
    public CinemachineVirtualCamera playerCam, firstPersonCam, managementCam;
    public Transform playerCamRig, targetCamRig, manageCamTarget;
    public CinemachineVirtualCamera currentVCam;
    public float playerRigFollowDamp = 0.1f;
    public CamInstance currCamInst;
    public List<CamInstance> camInstances;

    CinemachineTransposer currBody, playerCamBody, manageCamBody;
    float currentCamInitFOV, playerCamInitFOV, manageCamInitFOV;
    Vector3 currCamInitOffset, playerCamInitOffset, manageCamInitOffset, currCamOffsetStep, playerCamOffsetStep, manageCamOffsetStep;
    Transform rigFaceTarget, targetCam;
    bool rigFaceIsChar;
    float zoomMult;
    Rigidbody manageTargetRb;
    int boundaryLayer;
    bool targetCamActive;

    protected override void Awake() {
        base.Awake();
        if(m_ShuttingDown) return;
        
        MainCam = Camera.main;
        MainBrain = MainCam.GetComponent<CinemachineBrain>();
    }

    void Start() {
        Init();
        ClearCamTarget();
        CharacterManager.I.onPlayerRegistered += SnapToPlayer;
    }
    void SnapToPlayer(CharacterControl cc) {
        playerCamRig.position = cc.charAnim.headHold.position;
    }
    
    public void Init() {
        playerCamInitFOV = playerCam.m_Lens.FieldOfView;
        manageCamInitFOV = managementCam.m_Lens.FieldOfView;
        playerCam.Follow = playerCamRig;
        if(CharacterManager.I.Player)
            playerCamRig.position = CharacterManager.I.Player.charAnim.headHold.position;
        targetCam = targetCamRig.GetChild(0);
        managementCam.Follow = manageCamTarget;
        manageTargetRb = manageCamTarget.GetComponent<Rigidbody>();
        boundaryLayer = LayerMask.NameToLayer("Barrier");

        playerCamBody = playerCam.GetCinemachineComponent<CinemachineTransposer>();
        manageCamBody = managementCam.GetCinemachineComponent<CinemachineTransposer>();

        // playerCamInitOffset = playerCamBody.m_FollowOffset;
        playerCamInitOffset = playerCam.transform.localPosition;
        manageCamInitOffset = manageCamBody.m_FollowOffset;
        
        playerCamOffsetStep = playerCamInitOffset * 0.2f;
        manageCamOffsetStep = manageCamInitOffset * 0.2f;

        SetCam(CamTypes.PlayerCam);
    }

    public void SetFollowTarget(Transform t) {
        playerCam.Follow = t;
        managementCam.Follow = t;
    }
    public void SetLookTarget(Transform t, bool isCharacter = false) {
        rigFaceTarget = t;
        rigFaceIsChar = isCharacter;
    }
    public void SetCamTarget(GameObject target) {
        targetCamActive = true;
        targetCamRig.gameObject.SetActive(true);
        targetCamRig.transform.position = target.transform.position;
    }
    public void ClearCamTarget() {
        targetCamActive = false;
        targetCamRig.gameObject.SetActive(false);
    }
    public void RotateTargetCam(float y) {
        targetCamRig.transform.Rotate(0, y * 6f, 0);
    }
    public void PanTargetCam(float x, float y) {
        // charCam.localPosition += move * 0.02f;
        var pos = targetCam.localPosition;
        pos.x = Mathf.Clamp(pos.x + x * 0.05f, -0.2f, 0.2f);
        pos.y = Mathf.Clamp(pos.y + y * 0.05f, 0, 1.8f);
        targetCam.localPosition = pos;
    }
    public void ZoomTargetCam(float zoom) {
        Vector3 pos = targetCam.localPosition;
        pos.z = Mathf.Clamp(pos.z + zoom, 0.3f, 3f);
        targetCam.localPosition = pos;
    }

    public void ZoomCam(float z) {
        zoomMult = z;
        // currentVCam.m_Lens.FieldOfView = Mathf.Clamp(currentVCam.m_Lens.FieldOfView - z * 50f, currentCamInitFOV - 20, currentCamInitFOV + 40);
        float yOffset = 0;
        if(currentVCam != playerCam) {
            yOffset = Mathf.Clamp(currBody.m_FollowOffset.y - (currCamInitOffset.y * zoomMult), currCamInitOffset.y - currCamInitOffset.y * 0.8f, currCamInitOffset.y + currCamInitOffset.y * 0.5f);
        }
        else {
            yOffset = Mathf.Clamp(playerCam.transform.localPosition.y - (currCamInitOffset.y * zoomMult), currCamInitOffset.y - currCamInitOffset.y * 0.8f, currCamInitOffset.y + currCamInitOffset.y * 0.5f);
        }

        float zOffset = currCamInitOffset.z * (yOffset / currCamInitOffset.y);

        if(currentVCam != playerCam) {
            currBody.m_FollowOffset = new Vector3(0, yOffset, zOffset);
        }
        else {
            playerCam.transform.localPosition = new Vector3(0, yOffset, zOffset);
        }
    }
    Vector3 rigPos, velo;
    public void RotateCam(float x) {
        playerCamRig.transform.Rotate(0, x * 4f, 0);
    }

    public void FirstPersonLook(float x, float y) {
        playerCamRig.transform.rotation *= Quaternion.Euler(-y * 2f, x * 2f, 0);
        var rot = playerCamRig.transform.eulerAngles;
        rot.z = 0f;
        playerCamRig.transform.eulerAngles = rot;
    }
    public void UpdateManageTargetPos(Vector3 input) {
        manageTargetRb.velocity = input * 25f;
    }
    private void FixedUpdate() {
        if(!CharacterManager.I.Player || targetCamActive) 
            return;

        rigPos = CharacterManager.I.Player.charAnim.headHold.position;
        // if(CharacterManager.I.Player.isFirstPerson)
        //     playerCamRig.position = rigPos;
        // else
            playerCamRig.position = Vector3.SmoothDamp(playerCamRig.position, rigPos, ref velo, playerRigFollowDamp);
        
        // if global cam focus target is set, and is aimable cam
        if(rigFaceTarget) { 
            var dir = rigFaceTarget.position - rigPos;
            dir.y = 0;
            var rot = Quaternion.Euler(dir);
            playerCamRig.rotation = Quaternion.Lerp(playerCamRig.rotation, rot, Time.deltaTime);
        }
    }

    public void SetCam(CamTypes camType) {
        ResetCamPriorities();

        switch(camType) {
            case CamTypes.PlayerCam: {
                playerCam.Priority = 10;
                currentVCam = playerCam;
                currBody = playerCamBody;
                currentCamInitFOV = playerCamInitFOV;
                currCamInitOffset = playerCamInitOffset;
                currCamOffsetStep = playerCamOffsetStep;
                playerCamRig.rotation = Quaternion.identity;
                break;
            }
            case CamTypes.FirstPersonCam: {
                firstPersonCam.Priority = 10;
                currentVCam = firstPersonCam;
                break;
            }
            case CamTypes.ManagementCam: {
                managementCam.Priority = 10;
                currentVCam = managementCam;
                currBody = manageCamBody;
                currentCamInitFOV = manageCamInitFOV;
                currCamInitOffset = manageCamInitOffset;
                currCamOffsetStep = manageCamOffsetStep;
                manageCamTarget.gameObject.SetActive(true);
                break;
            }
        }
    }

    void ResetCamPriorities() {
        playerCam.Priority = 0;
        firstPersonCam.Priority = 0;
        managementCam.Priority = 0;
        manageCamTarget.gameObject.SetActive(false);
    }
}
}