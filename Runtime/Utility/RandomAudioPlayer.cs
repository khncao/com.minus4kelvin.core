/// <summary>
/// Adopted from Unity's 3D Game Kit
/// </summary>

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomAudioPlayer : MonoBehaviour
{
    public Dictionary<Material, List<AudioClip>> matAudioDict = new Dictionary<Material, List<AudioClip>>();
    public List<MaterialAudioPool> matAudioPoolList;

    [System.Serializable]
    public class MaterialAudioPool {
        public Material material;
        public List<AudioClip> clips;
        // public AudioClip[] landClips;
    }
    public AudioSource audioSource;
    public float volumeVariation = 0.2f;
    public float pitchVariation = 0.2f;

    float origVol, origPitch;

    private void Start() {
        for(int i = 0; i < matAudioPoolList.Count; ++i) {
            if(matAudioPoolList[i].material == null) continue;
            matAudioDict.Add(matAudioPoolList[i].material, matAudioPoolList[i].clips);
        }
        origVol = audioSource.volume;
        origPitch = audioSource.pitch;
    }

    public void PlayRandomClip() {
        PlayRandomClip(matAudioPoolList[0].material);
    }

    public void PlayRandomClip(Material material) {
        List<AudioClip> clips;
        
        if(material == null) {
            clips = matAudioPoolList[0].clips;
        }
        else {
            matAudioDict.TryGetValue(material, out clips);
        }
        if(clips != null && clips.Count > 0) {
            int rand = Random.Range(0, clips.Count - 1);
            audioSource.PlayOneShot(clips[rand]);
            RandomizeVolumeAndPitch();
        }
    }

    public void PlayRandomClip(AudioClip clip) {
        audioSource.PlayOneShot(clip);
        RandomizeVolumeAndPitch();
    }

    void RandomizeVolumeAndPitch() {
        audioSource.volume = origVol + Random.Range(-volumeVariation, volumeVariation);
        audioSource.pitch = origPitch + Random.Range(-pitchVariation, pitchVariation);
    }
}
