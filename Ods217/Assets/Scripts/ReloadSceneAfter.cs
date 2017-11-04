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
        yield return new WaitForSeconds(4);
        // GameManager.instance.LoadLastSaveFile();
        Debug.Log("loading scene");
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
