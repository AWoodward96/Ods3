using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour {

    public static MusicManager instance;
    public List<AudioTrack> AudioTracks;
    public enum AudioZoneType { Additive, Inverse, Enable };
    public bool Override;

    List<AudioTrack> DuplicateTrack;

    public void Awake()
    {
        instance = this;
        for(int i = 0; i < AudioTracks.Count; i++)
        {
            AudioTrack a = AudioTracks[i];
            AudioSource src = gameObject.AddComponent<AudioSource>();
            a.Source = src;
            src.volume = a.Volume;
            src.playOnAwake = a.PlayOnAwake;
            src.loop = a.Loop;
            src.clip = a.Audio;

            a.BaseVolume = a.Volume;
            if(a.PlayOnAwake)
            {
                src.Play();
            }

            AudioTracks[i] = a;
        }
        
 
    }

    private void Update()
    {
        foreach(AudioTrack track in AudioTracks)
        {
            if(Override)
            { 
                track.Volume = Mathf.Lerp(track.Volume, track.TargetVolume, .5f * Time.deltaTime);
                track.Source.volume = Mathf.Clamp(track.Volume, 0, track.Cap);
            }else
            {
                track.Volume = Mathf.Lerp(track.Volume, track.BaseVolume, .5f * Time.deltaTime);
                track.Source.volume = Mathf.Clamp(track.Volume, 0, track.Cap);
            }
            track.Source.loop = track.Loop;
        }
    }

    IEnumerator OverrideRoutine()
    {
        yield return new WaitForSeconds(1);
        Override = false;
    }

    public void modifyTrack(int _index, AudioZoneType _type, float _PassedValue)
    {
        float val = 0;
        switch(_type)
        {
            case AudioZoneType.Additive:
                val = Mathf.Lerp(AudioTracks[_index].Source.volume, _PassedValue, .5f * Time.deltaTime);
                val =  Mathf.Clamp(val, 0, AudioTracks[_index].Cap);
                AudioTracks[_index].Source.volume = val;
                AudioTracks[_index].Volume = val;

                break;
            case AudioZoneType.Inverse:
                float reveredVal = 1 - _PassedValue;
                val = Mathf.Lerp(AudioTracks[_index].Source.volume, reveredVal, .5f * Time.deltaTime);
                val = Mathf.Clamp(val, 0, AudioTracks[_index].Cap);
                AudioTracks[_index].Source.volume = val;
                AudioTracks[_index].Volume = val;
                break;
        } 
    }

    public void ResetTrack(int _index)
    {
        AudioTracks[_index].Source.volume = DuplicateTrack[_index].Volume;
    }


}
