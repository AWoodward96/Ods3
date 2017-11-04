using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CutsceneCharacters : MonoBehaviour {

    public string characterName;
    public Sprite defaultPortrait;
    public CutscenePortraits[] characterPortraits;
    public AudioClip talkingClip;
}

[System.Serializable]
public struct CutscenePortraits
{
    public string PortId;
    public Sprite PortImg;
}
 