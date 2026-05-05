using TMPro;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.SceneManagement;
public class GameManager : MonoBehaviour
{
    [HideInInspector] public float timeElapsed = 0f;
    [SerializeField] TextMeshProUGUI timer;
    public GameObject pauseScreen;

    public static bool paused = false;
    public static bool restarting = false;

    void Start()
    {
        restarting = false;
        paused = false;
    }

    void Update()
    {

        if (timer != null)
        {
            //update timer
            timeElapsed += Time.deltaTime;
            int minutes = Mathf.FloorToInt(timeElapsed / 60F);
            int seconds = Mathf.FloorToInt(timeElapsed - minutes * 60);
            timer.text = string.Format("Time: {0:0}:{1:00}", minutes, seconds);
        }

        //pause when q is pressed
        if (Input.GetKeyDown(KeyCode.Q) && !paused && !restarting)
        {
            Pause();
        }
        else if (Input.GetKeyDown(KeyCode.Q) && paused && !restarting)
        {
            Resume();
        }
    }

    public void Pause()
    {
        paused = true;

        //set time scale to 0 to pause the game
        Time.timeScale = 0f;

        //release mouse cursor from 3rd person controller so that it can interact with menu items
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        pauseScreen.gameObject.SetActive(true);
    }
    public void Resume()
    {
        Time.timeScale = 1f;
        paused = false;

        //hide mouse cursor and lock it to center of screen
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        pauseScreen.gameObject.SetActive(false);
    }
    public void Quit()
    {
        Time.timeScale = 1f;
        paused = false;
        SceneManager.LoadSceneAsync("MainMenu");
    }

    public void Restart()
    {
        restarting = true;
        paused = false;
        Time.timeScale = 1f;

        //reload current scene
        SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex);
    }
}
