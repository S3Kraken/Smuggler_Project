using Unity.VisualScripting;
using UnityEngine;

public class BreakableWall : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.tag == "Player")
        {
            //get the player's velocity
            Rigidbody2D playerRB = collider.gameObject.GetComponent<Rigidbody2D>();
            if (playerRB.linearVelocity.x > 40)
                Destroy(gameObject);
        }
    }
}
