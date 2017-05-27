using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This script is because unity doesn't support one offs. You have to deleberately set up a prefab and then generate a prefab and that seems like
/// a waste of space. This isn't the most elegant script as it's generating objects during runtime, but this script won't be used heavily so
/// it should be alright if used moderately
/// </summary>
public class myAudioSystem : MonoBehaviour {


    public static void PlayAudioOneShot(AudioClip _clip, Vector3 _AtLocation)
    {
        GameObject obj = new GameObject("One Shot: " + _clip.name);
        obj.transform.position = _AtLocation;
        AudioSource source = obj.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = false;

        source.clip = _clip;
        DestroyAfterXSeconds dest = obj.AddComponent<DestroyAfterXSeconds>();
        dest.time = _clip.length+ .1f;
        source.Play();
    }

    public static void PlayAudioOneShot(AudioClip _clip, Vector3 _AtLocation,float _Volume)
    {
        GameObject obj = new GameObject("One Shot: " + _clip.name);
        obj.transform.position = _AtLocation;
        AudioSource source = obj.AddComponent<AudioSource>();
        source.volume = _Volume;
        source.playOnAwake = false;
        source.loop = false;

        source.clip = _clip;
        DestroyAfterXSeconds dest = obj.AddComponent<DestroyAfterXSeconds>();
        dest.time = _clip.length + .1f;
        source.Play();
    }


    public static void PlayAudioOneShot(AudioSource _SourceParams, Vector3 _AtLocation)
    {
        GameObject obj = new GameObject("One Shot: " + _SourceParams.clip.name);
        obj.transform.position = _AtLocation;
        AudioSource source = obj.AddComponent<AudioSource>();
        source.clip = _SourceParams.clip;
        source.priority = _SourceParams.priority;
        source.volume = _SourceParams.volume;
        source.pitch = _SourceParams.pitch;
        source.panStereo = _SourceParams.panStereo;
        source.spatialBlend = _SourceParams.spatialBlend;
        source.reverbZoneMix = _SourceParams.reverbZoneMix;
 
        DestroyAfterXSeconds dest = obj.AddComponent<DestroyAfterXSeconds>();
        dest.time = _SourceParams.clip.length + .1f;
        source.Play();
    }

}
