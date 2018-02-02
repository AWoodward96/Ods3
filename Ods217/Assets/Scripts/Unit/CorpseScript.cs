using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// For when an enemy dies and needs to replace the existing enemy
/// </summary>
public class CorpseScript : MonoBehaviour
{

    Animator myAnim;
    UsableIndicator usableIndc;


    public int ScrapValue = 10;
    public bool Looted = false;
    public int xDisplacement;
    bool init = false;

    private void Init()
    {
        myAnim = GetComponent<Animator>();
        usableIndc = GetComponentInChildren<UsableIndicator>();
        usableIndc.Preset = UsableIndicator.usableIndcPreset.Loot;
        usableIndc.Output = ClaimLoot;
        init = true;
    }

    public void DropCorpse(Vector3 _atPos, bool _flipX)
    {
        if (!init)
            Init();

        xDisplacement *= ((_flipX) ? -1 : 1);
        GetComponent<SpriteRenderer>().flipX = _flipX;
        DropCorpse(_atPos);
    }

    public void DropCorpse(Vector3 _atPos)
    {
        if (!init)
            Init();

        usableIndc.GetComponent<RectTransform>().anchoredPosition = new Vector3(xDisplacement, 1, 0);
        gameObject.SetActive(true);
        transform.SetParent(null);
        transform.position = _atPos;
        myAnim.SetTrigger("Fire");
    }
    void ClaimLoot()
    {
        if (!Looted)
        {
            Looted = true;
            GameManager.ScrapCount += ScrapValue;
            usableIndc.Disabled = true;
        }
    }
}
