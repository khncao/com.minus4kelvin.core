
using UnityEngine;
using System.Collections.Generic;

namespace m4k.Progression {
[System.Serializable]
public class Story {
    public StoryData data;
}

[System.Serializable]
public class StorySegment {
    public Conditions startReqs, completeReqs;
    // public Objective objective;
    // public GuidReference objectiveObj;
    // public Objective objective { 
    //     get {
    //         return objectiveObj.gameObject != null ? objectiveObj.gameObject.GetComponent<Objective>() : null;
    //     }
    // }
    // public StoryEntity[] storyObjects;
}

[CreateAssetMenu(menuName="ScriptableObjects/Progress/StoryData")]
[System.Serializable]
public class StoryData : ScriptableObject {
    public string storyName;
    public string storyDescription;
    public List<StorySegment> storySegments;
}}