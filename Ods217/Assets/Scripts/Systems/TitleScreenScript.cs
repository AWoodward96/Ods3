using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Video;


public class TitleScreenScript : MonoBehaviour {

    public Image blackOut;

    float desiredAlpha;
    float curAlpha;
    bool Selected = false;
    float recentInput = 0;

    public Canvas MenuDemo;
    public Canvas DemoCanvas;

    public GameObject JukeBox;


    public bool state;
    public VideoPlayer myPlayer;
    Coroutine inBetweenCRT;

    Button[] buttons = new Button[5];
    Vector3 cameraPos;
 

    public void LoadScene(string _Name)
    {
        if (Selected)
            return;
        Selected = true;
        StartCoroutine(LoadCRT(_Name));
        desiredAlpha = 1;
		Directory.Delete(Application.persistentDataPath + "/Saves/Save1", true);
    }


    public void LoadGame()
    {
        if (Selected)
            return;
        Selected = true;
        StartCoroutine(GameCRT());
        desiredAlpha = 1;
    }

    public void QuitGame()
    {
        if (Selected)
            return;
        Selected = true;
        StartCoroutine(QuitCRT());
        desiredAlpha = 1;
    }

    private void Start()
    {
        desiredAlpha = 0;
        curAlpha = 1;
        buttons = GetComponentsInChildren<Button>();
        
    }

    private void Update()
    {
        Color c = Color.black;
        curAlpha = Mathf.Lerp(curAlpha, desiredAlpha, Time.deltaTime);
        c.a = curAlpha;
        blackOut.color = c;

        /*
        recentInput += Time.deltaTime;
        if (Input.anyKey)
        {
            recentInput = 0;
            desiredAlpha = 0;
            State = true;
        }

        if(recentInput > 15)
        {
            State = false;
        }*/


    }


    bool State
    {
        get { return state; }
        set {
            if(state != value)
            { 
                state = value;
                if (inBetweenCRT != null)
                    StopCoroutine(inBetweenCRT);
                inBetweenCRT = StartCoroutine(TransitionBetweenStates(value));
            }
        }

    }

    IEnumerator TransitionBetweenStates(bool val)
    {
        desiredAlpha = 1;
        if (val)
            myPlayer.Play();
        else
            myPlayer.Stop();
        yield return new WaitForSeconds(3);

        AudioSource[] a = JukeBox.GetComponents<AudioSource>();
        for (int i = 0; i < a.Length; i++)
        {
            if (val)
                a[i].Play();
            else
                a[i].Stop();
        }


        desiredAlpha = 0;
        DemoCanvas.gameObject.SetActive(!val);
        MenuDemo.gameObject.SetActive(val);
    }

    IEnumerator LoadCRT(string _Name)
    {
        yield return new WaitForSeconds(3);
        SceneManager.LoadScene(_Name); 
    }

    IEnumerator GameCRT()
    {
        yield return new WaitForSeconds(3);
		GameManager.instance.LoadSaveFile(true);
    }

    IEnumerator QuitCRT()
    { 
        yield return new WaitForSeconds(3);
        Application.Quit();
    }

   
}
