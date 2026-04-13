using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    public float maxHealth { get; set; } = 100;
    public float currentHealth { get; set; }
    public float iFrames { get; set; }

    TMPro.TextMeshProUGUI healthText;
    bool shielded = false;
    bool alphaReset = false;
    GameObject shield;
    GameObject activeShield;

    private void Awake()
    {
        healthText = GameObject.Find("Health (TMP)").GetComponent<TMPro.TextMeshProUGUI>();
        shield = GameObject.Find("ShieldPickup");
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentHealth = maxHealth;
        healthText.text = "Health: " + currentHealth;
    }

    // Update is called once per frame
    void Update()
    {
        iFrames -= Time.deltaTime;
        if (iFrames > 0)
        {
            alphaReset = false;
            Flicker();
        }
        else if (!alphaReset)
        {
            alphaReset = true;
            EndFlicker();
        }
    }
    public void Heal(float amount)
    {
        currentHealth += amount;
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
        healthText.text = "Health: " + currentHealth;
    }
    public void TakeDamage(float amount)
    {
        
        if (iFrames <= 0)
        {
            //Gives the player 3 secs of Iframes
            iFrames = 3;

            //Makes the shield take damage if the player has one
            if (shielded)
            {
                shielded = false;
                Destroy(activeShield);
                return;
            }

            currentHealth -= amount;
            if (currentHealth <= 0)
            {
                currentHealth = 0;
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
            healthText.text = "Health: " + currentHealth;
        }
    }
    public void Flicker()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr.color.a == 1)
        {
            sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 0.5f);
        }
        else
        {
            sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 1f);
        }
    }
    public void EndFlicker()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 1f);
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("PowerUp"))
        {
            //get the name
            string powerUpName = collision.gameObject.name;
            switch (powerUpName)
            {
                case "ShieldPickup":
                    Destroy(collision.gameObject);
                    shielded = true;
                    activeShield = Instantiate(shield, transform.position, Quaternion.identity);
                    activeShield.transform.SetParent(transform);
                    activeShield.GetComponent<BoxCollider2D>().enabled = false;
                    break;
                default:
                    break;
            }
        }
    }
}
