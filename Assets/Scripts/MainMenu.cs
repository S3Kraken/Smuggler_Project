using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Play()
    {
        SceneManager.LoadSceneAsync("RoofTopStage");
    }

    public void loadLevel(int levelNum)
    {
        switch (levelNum)
        {
            case 0:
                SceneManager.LoadSceneAsync("TutorialStage");
                break;
            case 1:
                SceneManager.LoadSceneAsync("AlleywayStage");
                break;
            case 2:
                SceneManager.LoadSceneAsync("RoofTopStage");
                break;
            case 3:
                SceneManager.LoadSceneAsync("SewerStage");
                break;
        }
    }
}
