using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class TitleScreenScript : MonoBehaviour {

    public Image blackOut;

    float desiredAlpha;
    float curAlpha;
    bool Selected = false;

    public void LoadScene(string _Name)
    {
        if (Selected)
            return;
        Selected = true;
        StartCoroutine(LoadCRT(_Name));
        desiredAlpha = 1;
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
    }

    private void Update()
    {
        Color c = Color.black;
        curAlpha = Mathf.Lerp(curAlpha, desiredAlpha, Time.deltaTime);
        c.a = curAlpha;
        blackOut.color = c; 
    }

    IEnumerator LoadCRT(string _Name)
    {
        yield return new WaitForSeconds(3);
        SceneManager.LoadScene(_Name); 
    }

    IEnumerator GameCRT()
    {
        yield return new WaitForSeconds(3);
        GameManager.instance.LoadLastSaveFile();
    }

    IEnumerator QuitCRT()
    { 
        yield return new WaitForSeconds(3);
        Application.Quit();
    }

   
}
