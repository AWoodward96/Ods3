using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Chest : MonoBehaviour {

    public GameObject Lid;
    public GameObject PlusHealthInd;
    bool pickedUp = false;
    float dTime = 0; 
    UsableIndicator myIndicator;
    AudioSource mySource;

    // Use this for initialization
    void Start()
    { 
        myIndicator = GetComponentInChildren<UsableIndicator>();

        myIndicator.Output = OnInteract;

        mySource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        if (pickedUp)
        {
            if(PlusHealthInd != null)
            {
                dTime += Time.deltaTime;
                PlusHealthInd.transform.localPosition = Vector3.Lerp(PlusHealthInd.transform.localPosition, new Vector3(0, 2.5f, 0), Time.deltaTime);
                if (dTime > 3)
                {
                    Destroy(PlusHealthInd);
                }
            }
        }

    }

    void OnInteract()
    {
        GameManager.HealthKits++;
        MenuManager.instance.ShowHealthkit();
        pickedUp = true;


        PlusHealthInd.SetActive(true);
        PlusHealthInd.transform.parent = GameObject.FindGameObjectWithTag("Player").transform;
        PlusHealthInd.transform.localPosition = new Vector3(0, 2, 0);


        Lid.SetActive(false);
        myIndicator.Disabled = true;

        if (mySource != null)
            mySource.Play();
    }
}
