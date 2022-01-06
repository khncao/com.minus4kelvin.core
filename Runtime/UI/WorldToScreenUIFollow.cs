using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace m4k.UI {
/// <summary>
/// Have normal canvas UI objects follow a point in world space. Change script execution order near tail end to eliminate visible follow delays.
/// </summary>
public class WorldToScreenUIFollow : MonoBehaviour
{
	class UITarget {
		public Renderer followTargetRend;
		public RectTransform followerTrans;
		public Transform followTargetTrans;
		public GameObject followerObj;
		public float vertOffset;
	}
	public RectTransform canvasRectT;
    public GameObject defaultUITxtPrefab;
    public GameObject defaultUIImgPrefab;

	List<UITarget> uiTargets = new List<UITarget>();
	// GameUI gameUI;
	Camera mainCam;

	public void Start() {
		if(!canvasRectT) {
			canvasRectT = GameObject.Find("WorldFollowCanvas").transform as RectTransform;
		}
		SceneHandler.I.onSceneLoaded += OnSceneLoaded;
		mainCam = Cams.MainCam;
	}

	void OnSceneLoaded() {
		ClearAll();
	}

	void LateUpdate () 
	{
		for(int i = 0; i < uiTargets.Count; i++)
		{
			Vector3 viewportPos = mainCam.WorldToViewportPoint(uiTargets[i].followTargetTrans.position + (Vector3.up * uiTargets[i].vertOffset));

			if(viewportPos.z < 0) {
				uiTargets[i].followerObj.SetActive(false);
			}
			else {
				uiTargets[i].followerObj.SetActive(true);
			}

			viewportPos -= 0.5f * Vector3.one; 
			viewportPos.z = 0;
			
			viewportPos.x *= canvasRectT.rect.width;
			viewportPos.y *= canvasRectT.rect.height;

			uiTargets[i].followerTrans.localPosition = viewportPos;// + screenOffset;



            // // Offset position above object bbox (in world space)
            // float offsetPosY = uiTargets[i].followTargetTrans.position.y + 1.5f;
            
            // // Final position of marker above GO in world space
            // Vector3 offsetPos = new Vector3(uiTargets[i].followTargetTrans.position.x, offsetPosY, uiTargets[i].followTargetTrans.position.z);
            
            // // Calculate *screen* position (note, not a canvas/recttransform position)
            // Vector2 canvasPos;
            // Vector2 screenPoint = Cams.MainCam.WorldToScreenPoint(offsetPos);
            
            // // Convert screen position to Canvas / RectTransform space <- leave camera null if Screen Space Overlay
            // RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRectT, screenPoint, null, out canvasPos);
            
            // // Set
            // uiTargets[i].followerTrans.localPosition = canvasPos;
		}
	}

    public GameObject RegisterDefaultTxtUI(Renderer targetRend, Transform targetTrans, float vertOffset = 0f) {
        return RegisterFollowUI(targetRend, defaultUITxtPrefab, targetTrans, vertOffset);
    }
    public GameObject RegisterDefaultImgUI(Renderer targetRend, Transform targetTrans, float vertOffset = 0f) {
        return RegisterFollowUI(targetRend, defaultUIImgPrefab, targetTrans, vertOffset);
    }

	public GameObject RegisterFollowUI(Renderer targetRend, GameObject followerUIObj, Transform targetTrans, float vOffset = 0f)
	{
		var go = Instantiate(followerUIObj) as GameObject;
		var t = go.GetComponent<RectTransform>();

		var obj = new UITarget() {
			followerObj = go,
			followerTrans = t,
			followTargetRend = targetRend,
			followTargetTrans = targetTrans,
			vertOffset = vOffset,
		};
		t.SetParent(canvasRectT);
		t.localScale = Vector3.one;

		uiTargets.Add(obj);

		return go;
	}

	public void UnregisterFollowUI(Transform t)
	{
		int i = uiTargets.FindIndex(x => x.followTargetTrans == t);

		if(i != -1){
			Destroy(uiTargets[i].followerObj);
			uiTargets.RemoveAt(i);
		}

		// Debug.Log("Removed follower ui");
	}

	public void ClearAll() {
		for(int i = 0; i < uiTargets.Count; ++i) {
			Destroy(uiTargets[i].followerObj);
		}
		uiTargets.Clear();
	}
}
}