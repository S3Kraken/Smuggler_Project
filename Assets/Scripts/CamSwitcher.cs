using UnityEngine;

public class CamSwitcher : MonoBehaviour
{
    GameObject mainCam;
    GameObject panCam;
    public bool panModeActive = false;
    void Awake()
    {
        mainCam = GameObject.Find("Main Camera");
        panCam = GameObject.Find("PanCam");
    }

    void Start()
    {
        panCam.SetActive(false);
    }
     void OnToggleCam()
    {
        if (mainCam.activeSelf)
        {
            mainCam.SetActive(false);
            panCam.SetActive(true);
            panModeActive = true;
            panCam.GetComponent<PanCamScript>().ResetSpeed();
        }
        else
        {
            mainCam.SetActive(true);
            panCam.SetActive(false);
            panModeActive = false;
        }
    }
}
