using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using m4k.Progression;

namespace m4k {
/// <summary>
/// Handle PlayableDirector callbacks. Will try to find and set timeline binds from PlayableManager.
/// </summary>
[RequireComponent(typeof(PlayableDirector))]
public class PlayableController : MonoBehaviour
{
    [Tooltip("If enabled, will register the name of finished playable assets as key states in ProgressionManager")]
    public bool isKeyState;
    public bool isCinematic;
    PlayableDirector director;

    IEnumerator Start()
    {
        director = GetComponent<PlayableDirector>();
        director.played += OnStart;
        director.stopped += OnStop;

        // if(bindMainCamera) {
        //     // BindAllSimilar<Cinemachine.CinemachineBrain>(director, "_cam", Cams.MainBrain);
        //     BindAllType<Cinemachine.CinemachineBrain>(director, Cams.MainBrain);
        // }
        
        yield return new WaitForEndOfFrame();
        BindTimelineGlobal(director);
        if(director.playOnAwake) OnStart(director);
    }

    void OnStart(PlayableDirector director) {
        if(isCinematic)
            PlayableManager.I.ToggleCinematic(true);
        // Debug.Log("OnStart playable");
    }

    void OnStop(PlayableDirector director) {
        if(isKeyState && director.playableAsset)
            ProgressionManager.I?.RegisterKeyState(director.playableAsset.name);
        if(isCinematic)
            PlayableManager.I?.ToggleCinematic(false);
        // Debug.Log("OnStop Playable");
    }

    /// <summary>
    /// Bind search criteria: no generic binding or bound object is disabled
    /// Bind search key: output stream name or object name of existing bind
    /// </summary>
    /// <param name="director"></param>
    public void BindTimelineGlobal(PlayableDirector director)
    {
        foreach(var output in director.playableAsset.outputs) 
        {
            if(!output.sourceObject || output.outputTargetType == null) 
                continue;

            Object genBind = director.GetGenericBinding(output.sourceObject);
            Component monoObj = null;
            if(genBind && genBind is Component) 
                monoObj = genBind as Component;
            if(genBind)
                if(!monoObj || monoObj && monoObj.gameObject.activeInHierarchy)
                    continue;

            // string s = $"Stream: {output.streamName}\nType: {output.outputTargetType}";
            // if(output.sourceObject) s += $"\nObject: {output.sourceObject.name}";
            // Debug.Log(s);
            
            GameObject go = null;
            go = monoObj ? PlayableManager.I.GetBindTarget(monoObj.gameObject.name) : PlayableManager.I.GetBindTarget(output.streamName);
            if(!go)
                continue;
            if(output.outputTargetType.Equals(typeof(GameObject))) {
                director.SetGenericBinding(output.sourceObject, go);
            }
            else {
                var bind = go.GetComponent(output.outputTargetType);
                director.SetGenericBinding(output.sourceObject, bind);
            }
            
            // handle cinemachine binds
            if(!(output.sourceObject is CinemachineTrack))
                continue;
            var cinemachineTrack = output.sourceObject as CinemachineTrack;
            bool b;
            foreach( var clip in cinemachineTrack.GetClips() ) {
                var cinemachineShot = clip.asset as CinemachineShot;
                if(director.GetReferenceValue(cinemachineShot.VirtualCamera.exposedName, out b))
                    continue;
                GameObject go2 = PlayableManager.I.GetBindTarget(clip.displayName);
                if(!go2)
                    continue;
                var vc = go2.GetComponent<Cinemachine.CinemachineVirtualCameraBase>();
                director.SetReferenceValue(cinemachineShot.VirtualCamera.exposedName, vc);
            }
        }
    }

    public void BindAllType<T>(PlayableDirector director, T obj) where T : Object
    {
        foreach(var output in director.playableAsset.outputs) {
            if(output.outputTargetType.Equals(typeof(T))) {
                director.SetGenericBinding(output.sourceObject, obj as T);
            }
        }
    }

    public void BindSimilarQuery<T>(PlayableDirector director, string query, T obj) where T : Object
    {
        // var timeline = director.playableAsset as TimelineAsset;
        // foreach (var track in timeline.GetOutputTracks())
        // {
        //     if (track.name.Contains(query))
        //     {
        //         director.SetGenericBinding(track, obj as T);
        //     }
        // }
        foreach(var output in director.playableAsset.outputs) {
            if(output.streamName.Contains(query)) {
                director.SetGenericBinding(output.sourceObject, obj as T);
            }
        }
    }

    public void BindExactQuery<T>(PlayableDirector director, string query, T obj) where T : Object
    {
        // var timeline = director.playableAsset as TimelineAsset;
        // foreach (var track in timeline.GetOutputTracks())
        // {
        //     if (track.name == trackName)
        //     {
        //         director.SetGenericBinding(track, obj as T);
        //         break;
        //     }
        // }
        foreach(var output in director.playableAsset.outputs) {
            if(output.streamName == query) {
                director.SetGenericBinding(output.sourceObject, obj as T);
                break;
            }
        }
    }

}
}