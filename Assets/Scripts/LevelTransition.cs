using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelTransition : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision != null && collision.gameObject.CompareTag("Player"))
        {
            switch (SceneManager.GetActiveScene().buildIndex)
            {
                case 2: //roof to alley
                    SceneManager.LoadSceneAsync("AlleywayStage");
                    break;
                case 3: //alley to sewer
                    SceneManager.LoadSceneAsync("SewerStage");
                    break;
                case 4: //sewer to roof
                    SceneManager.LoadSceneAsync("RoofTopStage");
                    break;
            }
        }
    }
}
