using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A system of audio zones that set up the verticle re-orchestration
/// A bunch of tracks play at once that are on loop but you can only hear certain ones when you're in a zone
/// </summary>
[ExecuteInEditMode]
public class NewMusicManager : MonoBehaviour {

        public static NewMusicManager instance;
        [Header("Audio Information")]
        public List<AudioTrack> AudioTracks;
        [Space(25)]
        [Header("Zone Information")]
        public List<MusicZone> MusicZones;

        [Space(25)]
        [Header("Script Information")]
        public bool Override;
        public float YPosition;
        public enum AudioZoneType { Additive, Inverse, Enable };

        // Use this for initialization
        void Awake()
        {
            if (Application.isPlaying)
            {
                instance = this;
                for (int i = 0; i < AudioTracks.Count; i++)
                {
                    AudioTrack a = AudioTracks[i];
                    AudioSource src = gameObject.AddComponent<AudioSource>();
                    a.Source = src;
                    src.volume = a.Volume;
                    src.playOnAwake = a.PlayOnAwake;
                    src.loop = a.Loop;
                    src.clip = a.Audio;

                    a.BaseVolume = a.Volume;
                    if (a.PlayOnAwake)
                    {
                        src.Play();
                    }

                    AudioTracks[i] = a;
                }
            }


        }

        // Update is called once per frame
        void Update()
        {
            if (Application.isPlaying)
            {
                checkZones();

                foreach (AudioTrack track in AudioTracks)
                {
                    if (Override)
                    {
                        track.Volume = Mathf.Lerp(track.Volume, track.TargetVolume, .5f * Time.deltaTime);
                        track.Source.volume = Mathf.Clamp(track.Volume, 0, track.Cap);
                    }
                    else
                    {
                        track.Volume = Mathf.Lerp(track.Volume, track.BaseVolume, .5f * Time.deltaTime);
                        track.Source.volume = Mathf.Clamp(track.Volume, 0, track.Cap);
                    }
                    track.Source.loop = track.Loop;
                }

            }
            else
            {
                for (int i = 0; i < MusicZones.Count; i++)
                {
                    if (MusicZones[i].POPULATETRACKS)
                    {
                        MusicZones[i].POPULATETRACKS = false;
                        MusicZones[i].TrackVolumes = new float[AudioTracks.Count];
                    }
                }
            }
        }


        void checkZones()
        {
            // Check for the player
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player) // Can't do anything if he doesn't exist
            {
                // Calculate the zones extents
                Vector3 playerPos = GlobalConstants.ZeroYComponent(player.transform.position);

                foreach (MusicZone zone in MusicZones)
                {
                    Vector3 zoneTopLeft = new Vector3(zone.Position.x, 0, zone.Position.y) - new Vector3(zone.Size.x / 2, 0, -zone.Size.y / 2);
                    Vector3 zoneBottomRight = new Vector3(zone.Position.x, 0, zone.Position.y) + new Vector3(zone.Size.x / 2, 0, -zone.Size.y / 2);


                    // If the player is in the audio zone
                    if (playerPos.x > zoneTopLeft.x && playerPos.x < zoneBottomRight.x && playerPos.z < zoneTopLeft.z && playerPos.z > zoneBottomRight.z)
                    {
                        Override = true;
                        StopCoroutine(OverrideCoroutine());
                        StartCoroutine(OverrideCoroutine());
                        for (int i = 0; i < AudioTracks.Count; i++)
                        {
                            // loop through and dump all the values into the target volume
                            AudioTracks[i].TargetVolume = zone.TrackVolumes[i];
                        }
                    }
                }

            }
        }

        IEnumerator OverrideCoroutine()
        {
            yield return new WaitForSeconds(1);
            Override = false;
        }

        private void OnDrawGizmosSelected()
        {
            for (int i = 0; i < MusicZones.Count; i++)
            {
                MusicZone t = MusicZones[i]; // Get the zone
                                             // Set up the positions and associated positions
                Vector3 tpos = new Vector3(t.Position.x, YPosition, t.Position.y);
                Gizmos.color = t.ZoneColor;

                Vector3 transSize = new Vector3(t.Size.x, 1, t.Size.y);

                Gizmos.DrawCube(tpos, transSize);
            }
        }

        public void SetTrack(int _index, float _value)
        {
            AudioTracks[_index].BaseVolume = _value;
        }
    }

    [System.Serializable]
    public class MusicZone
    {
        public Vector2 Size;
        public Vector2 Position;

        public bool POPULATETRACKS;
        [Range(0, 1)]
        public float[] TrackVolumes;

        public Color ZoneColor;
        public int AudioTrackIndex;
    }


    [System.Serializable]
    public class AudioTrack
{
    [Tooltip("The actual sound clip that we'll be playing")]
    public AudioClip Audio;
    [Tooltip("The Audio Source generated on runtime that we'll be using for this Track.")]
    public AudioSource Source;

        public bool PlayOnAwake = false;
        public bool Loop = true;

    [Tooltip("Current volume of this track.")]
    [Range(0, 1)]
        public float Volume = 1;


    [Tooltip("The desired volume of this track.")] 
        public float TargetVolume;

        [HideInInspector]
        public float BaseVolume;

        [Range(0, 1)]
        public float Cap = 1;
    }

 






