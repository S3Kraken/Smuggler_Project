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
        aud = GetComponent<AudioSource>();
        if (SceneManager.GetActiveScene().buildIndex == 0) //Mainmenu
        {
            aud.clip = Resources.Load<AudioClip>("Music/Shy_Bandit's_Theme");
        }
        else if (SceneManager.GetActiveScene().buildIndex == 1) //Tutorial
        {
            aud.clip = Resources.Load<AudioClip>("Music/Ninjala_OST_Closet");
        }
        else if (SceneManager.GetActiveScene().buildIndex == 2) //RoofTops
        {
            int num = Random.Range(0, 3);
            if (num == 0)
            {
                aud.clip = Resources.Load<AudioClip>("Music/Vengeance_is_Mine");
            }
            else if (num == 1)
            {
                aud.clip = Resources.Load<AudioClip>("Music/Eggmanland");
            }
            else
            {
                aud.clip = Resources.Load<AudioClip>("Music/Flying_Battery_Zone_Act_1");
            }
        }
        else if (SceneManager.GetActiveScene().buildIndex == 3) //Alleyways
        {
            if (Random.Range(0, 2) == 0)
            {
                aud.clip = Resources.Load<AudioClip>("Music/Seaskape");
            }
            else
            {
                aud.clip = Resources.Load<AudioClip>("Music/Highway_in_the_Sky");
            }
        }
        else if (SceneManager.GetActiveScene().buildIndex == 4) //Sewer
        {
            //Get a random number that's between 0 or 1. if its 0 play the first song and if its one play the other song
            if (Random.Range(0, 2) == 0)
            {
                aud.clip = Resources.Load<AudioClip>("Music/Hydrocity_Zone_Act_1");
            }
            else
            {
                aud.clip = Resources.Load<AudioClip>("Music/Hydrocity Zone Act 2");
            }
        }
        aud.Play();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
