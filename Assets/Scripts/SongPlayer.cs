using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SongPlayer : MonoBehaviour
{
    public AudioSource aud;
    // Start is called before the first frame update
    void Start()
    {
        SetMusic();
    }

    public void SetMusic()
    {
        Debug.Log("Running set music");
        aud = GetComponent<AudioSource>();
        if (SceneManager.GetActiveScene().buildIndex == 0) //Mainmenu
        {
            aud.clip = Resources.Load<AudioClip>("Music/");
        }
        else if (SceneManager.GetActiveScene().buildIndex == 1) //Alleyways
        {
            aud.clip = Resources.Load<AudioClip>("Music/");
        }
        else if (SceneManager.GetActiveScene().buildIndex == 2) //RoofTops
        {
            aud.clip = Resources.Load<AudioClip>("Music/");
        }
        else if (SceneManager.GetActiveScene().buildIndex == 3) //Sewer
        {
            //Get a random number that's between 0 or 1. if its 0 play the first song and if its one play the other song
            if (Random.Range(0,2) == 0)
            {
                aud.clip = Resources.Load<AudioClip>("Music/Hydrocity_Zone_Act_1");
            }
            else
            {
                aud.clip = Resources.Load<AudioClip>("Music/Hydrocity Zone Act 2");
            }
            print("music should be on");
        }
        aud.Play();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
