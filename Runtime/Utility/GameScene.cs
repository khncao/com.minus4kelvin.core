using UnityEngine;
// using UnityEngine.Rendering.PostProcessing;
using System.Collections.Generic;

[CreateAssetMenu(menuName="ScriptableObjects/GameScene")]
public class GameScene : ScriptableObject
{
    public string sceneName;
    public string description;
    public List<GameScene> requiredScenes;

    public AudioClip music;
    [Range(0.0f, 1.0f)]
    public float musicVolume;

    // public PostProcessProfile postprocess;
}