using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// using UnityEngine.UI;

public class AnimatedUIQueue : MonoBehaviour
{
    [System.Serializable]
    public class AnimatedUIObj {
        public GameObject instance;
        public TMPro.TMP_Text txt;
        public CanvasGroup canvasGroup;
        public float progress;
    }

    public GameObject prefab;
    public Transform parent;
    public CanvasGroup eventLogGroup;
    public TMPro.TMP_Text eventLogTxt;

    Queue<string> stringQueue = new Queue<string>();
    Stack<AnimatedUIObj> animatedUIObjs = new Stack<AnimatedUIObj>();
    List<AnimatedUIObj> animating = new List<AnimatedUIObj>();
    Queue<string> eventLogLines = new Queue<string>();

    public void Start() {
        for(int i = 0; i < 10; ++i) {
            SpawnUiObj();
        }
    }

    void SpawnUiObj() {
        var uiObj = new AnimatedUIObj();
        uiObj.instance = Instantiate(prefab, parent);
        uiObj.txt = uiObj.instance.GetComponentInChildren<TMPro.TMP_Text>();
        uiObj.canvasGroup = uiObj.instance.GetComponentInChildren<CanvasGroup>();
        // uiObj.instance.SetActive(false);
        uiObj.canvasGroup.alpha = 0;

        animatedUIObjs.Push(uiObj);
    }

    public void QueueLineWithCooldown(string line, bool popup = false) {
        if(popup)
            stringQueue.Enqueue(line);
        UpdateEventLog(line);
    }
    public void SendLineInstant(string line, bool popup = false) {
        if(popup)
            PopObj(line);
        UpdateEventLog(line);
    }

    void PopObj(string line) {
        if(animatedUIObjs.Count < 1) {
            SpawnUiObj();
        }
        var obj = animatedUIObjs.Pop();

        obj.txt.text = line;
        obj.instance.transform.localPosition = Vector3.zero;
        obj.canvasGroup.alpha = 1f;
        obj.progress = 0;

        animating.Add(obj);
    }

    void RequeueObj(AnimatedUIObj obj) {
        animatedUIObjs.Push(obj);
    }
    float lastEventLogTime, lastLogDiff;
    void UpdateEventLog(string line) {
        if(eventLogLines.Count >= 50) {
            eventLogLines.Dequeue();
        }
        eventLogLines.Enqueue(line);
        var evArr = eventLogLines.ToArray();
        eventLogTxt.text = "";
        for(int i = 0; i < evArr.Length; ++i) {
            eventLogTxt.text += evArr[i] + '\n';
        }
        lastEventLogTime = Time.time;
        lastLogDiff = 0;
        eventLogGroup.alpha = 1;
        eventLogGroup.blocksRaycasts = true;
    }

    float cdTimer;
    private void Update() {
        if(lastLogDiff < 4) {
            lastLogDiff = Time.time - lastEventLogTime;
            if(lastLogDiff > 3)
                eventLogGroup.alpha = 1 - (lastLogDiff - 3);
            if(eventLogGroup.alpha < 1)
                eventLogGroup.blocksRaycasts = false;
        }
        if(cdTimer < 0.4f) {
            cdTimer += Time.deltaTime;
        }
        else if(stringQueue.Count > 0) {//
            PopObj(stringQueue.Dequeue());
            cdTimer = 0;
        }

        if(animating.Count < 1)
            return;
        for(int i = 0; i < animating.Count; ++i) {
            if(animating[i].progress < 2f) {
                animating[i].progress += Time.deltaTime;
                float ratio = animating[i].progress / 2f;
                animating[i].instance.transform.localPosition = Vector3.up * (ratio * 100);
                animating[i].canvasGroup.alpha = 1 - ratio;
            }
            else {
                animating[i].canvasGroup.alpha = 0;
                RequeueObj(animating[i]);
                animating.Remove(animating[i]);
            }
        }
    }
}
