

using UnityEngine;
using Cinemachine;

namespace m4k {
public class CamBase : MonoBehaviour {
    public string key;
    // public Transform pivot;
    public CinemachineVirtualCamera vcam;
    public Transform panZoomOverride;
    public bool disableOnUnuse = false;
    public bool dampenRigFollow = true;
    public float followDamp = 0.1f;
    public float offsetMult = 0.2f;
    public Transform followTarget;
    public Transform lookTarget;
    public bool allowRigYRot;
    public float rigXRotMult = 2f;
    public float rigYRotMult = 2f;
    public float rbVelMult = 25f;

    public Vector3 OffsetStep { get { return initOffset * offsetMult; }}

    [HideInInspector]
    public Vector3 initOffset;

    float initFov, initFarClip;
    // CinemachineTransposer transposer;
    // CinemachineComposer composer;
    Rigidbody rb;
    Vector3 velo;
    Transform panZoom;

    protected virtual void Start() {
        rb = GetComponent<Rigidbody>();
        panZoom = panZoomOverride || !vcam ? panZoomOverride : vcam.transform;
        initOffset = vcam && !panZoomOverride ? vcam.transform.localPosition : panZoomOverride.localPosition;
        if(disableOnUnuse)
            gameObject.SetActive(false);

        if(!vcam) return;
        initFov = vcam.m_Lens.FieldOfView;
        initFarClip = vcam.m_Lens.FarClipPlane;

        if(followTarget)
            vcam.Follow = followTarget;
        if(lookTarget)
            vcam.LookAt = lookTarget;
        // transposer = vcam.GetCinemachineComponent<CinemachineTransposer>();
        // composer = vcam.GetCinemachineComponent<CinemachineComposer>();
    }

    public virtual void MoveRig(Vector3 targetPos) {
        if(dampenRigFollow) 
            transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref velo, followDamp);
        else
            transform.position = targetPos;
    }
    public virtual void MoveRigRb(Vector3 input) {
        if(!rb) return;
        rb.velocity = input * rbVelMult;
    }

    public virtual void RotateRig(float xIn, float yIn) {
        if(!allowRigYRot) return;
        transform.Rotate(yIn * rigXRotMult, xIn * rigYRotMult, 0);
    }

    public virtual void SimpleZoomCam(float z) {
        Vector3 pos = panZoom.localPosition;
        pos.z = Mathf.Clamp(pos.z + z, 0.3f, 3f);
        panZoom.localPosition = pos;
    }

    public virtual void ZoomCam(float z) {
        float yOffset = Mathf.Clamp(
            panZoom.localPosition.y - (initOffset.y * z), 
            initOffset.y - initOffset.y * 0.8f, 
            initOffset.y + initOffset.y * 0.5f
        );

        float zOffset = initOffset.z * (yOffset / initOffset.y);

        panZoom.localPosition = new Vector3(0f, yOffset, zOffset);
    }
    public virtual void PanCamera(float xIn, float yIn) {
        var pos = panZoom.localPosition;
        pos.x = Mathf.Clamp(pos.x + xIn * 0.05f, -0.2f, 0.2f);
        pos.y = Mathf.Clamp(pos.y + yIn * 0.05f, 0, 1.8f);
        panZoom.localPosition = pos;
    }

    public void SetFarClip(float f) {
        if(!vcam) return;

        vcam.m_Lens.FarClipPlane = f == -1f ? initFarClip : f;
    }

    public void AssignTargets(Transform follow, Transform look, bool teleport = false) {
        if(vcam) {
            vcam.Follow = follow;
            vcam.LookAt = look;
        }
        if(teleport)
            transform.position = follow.transform.position;
    }

    public virtual void Enable() {
        if(vcam)
            vcam.Priority = 10;
        if(rb)
            rb.gameObject.SetActive(true);
        if(!gameObject.activeInHierarchy)
            gameObject.SetActive(true);
    }
    public virtual void Disable() {
        if(vcam)
            vcam.Priority = 0;
        if(rb)
            rb.gameObject.SetActive(false);
        if(disableOnUnuse)
            gameObject.SetActive(false);
    }
}
}