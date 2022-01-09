

using UnityEngine;
// using UnityEngine.Animations;

namespace m4k.Characters {
[CreateAssetMenu(fileName = "AnimationProfile", menuName = "Data/AnimationProfile", order = 0)]
public class AnimationProfile : ScriptableObject {
    // public AnimationClip greet, drink, eat, laugh, yes, no, cheer, jeer, sit;
    public AnimationGesture[] gestures;

    AnimatorOverrideController overrideController;
    AnimationClipOverrides clipOverrides;
    // string[] clipNames;
    AnimationClip[] clips;

    public void Initialize(Animator anim) {
        if(gestures.Length < 1) {
            Debug.LogWarning("Tried to initialize with no gestures");
            return;
        }
        overrideController = new AnimatorOverrideController(anim.runtimeAnimatorController);
        clipOverrides = new AnimationClipOverrides(overrideController.overridesCount);
        overrideController.GetOverrides(clipOverrides);

        for(int i = 0; i < gestures.Length; ++i) {
            clipOverrides[gestures[i].gestureName] = gestures[i].clip;
        }

        // clips = new AnimationClip[] { greet, drink, eat, laugh, yes, no, cheer, jeer, sit, };
        // clipNames = new string[] { "greet", "drink", "eat", "laugh", "yes", "no", "cheer", "jeer", "sit", };
        // for(int i = 0; i < clips.Length; ++i) {
        //     clipOverrides[clipNames[i]] = clips[i];
        // }
    }
    public AnimatorOverrideController GetOverrideController() {
        if(!overrideController) 
            Debug.LogError("Non initialized override controller");
        
        return overrideController;
    }
    public AnimationGesture GetGesture(string gestureName) {
        return System.Array.Find<AnimationGesture>(gestures, x=>x.gestureName == gestureName);
    }

    // public string GetRandomGestureName() {
    //     int rand = Random.Range(0, clipNames.Length);
    //     return clipNames[rand];
    // }
}

public class AnimationGesture {
    public string gestureName;
    public AnimationClip clip;
}
}