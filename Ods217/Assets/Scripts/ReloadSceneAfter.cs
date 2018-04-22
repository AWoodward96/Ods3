using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ReloadSceneAfter : MonoBehaviour
{

    // Use this for initialization
    void Start()
    {
        StartCoroutine(reload());
    }

    IEnumerator reload()
    {
		yield return new WaitForSeconds(2);

		Camera.main.GetComponent<CamScript>().AddEffect(CamScript.CamEffect.FadeOut, 1.0f);

        yield return new WaitForSeconds(2);
        GameManager.instance.LoadLastSaveFile();
        Debug.Log("loading scene");
        //SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
