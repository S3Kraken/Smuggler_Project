using UnityEngine;

public class CamSwitcher : MonoBehaviour
{
    GameObject mainCam;
    GameObject panCam;
     void Awake()
    {
        mainCam = GameObject.Find("Main Camera");
        panCam = GameObject.Find("PanCam");
    }
     void OnToggleCam()
    {
        if (mainCam.activeSelf)
        {
            mainCam.SetActive(false);
            panCam.SetActive(true);
        }
        else
        {
            mainCam.SetActive(true);
            panCam.SetActive(false);
        }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
