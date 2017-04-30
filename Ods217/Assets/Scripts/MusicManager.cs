using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour {


    public List<AudioTrack> AudioTracks;

    public void Awake()
    {
        for(int i = 0; i < AudioTracks.Count; i++)
        {
            AudioTrack a = AudioTracks[i];
            AudioSource src = gameObject.AddComponent<AudioSource>();
            a.Source = src;
            src.volume = a.Volume;
            src.playOnAwake = a.PlayOnAwake;
            src.loop = a.Loop;
            src.clip = a.Audio;

            if(a.PlayOnAwake)
            {
                src.Play();
            }

            AudioTracks[i] = a;
        }
    }
 	 
}


[System.Serializable]
public class AudioTrack
{
    public AudioClip Audio;
    public AudioSource Source;
    public bool PlayOnAwake = false;
    public bool Loop = true;
    [Range(0,1)]
    public float Volume = 1;
}

