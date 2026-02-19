using UnityEngine;

public class BulletScript : MonoBehaviour
{
    float lifeTime = 5f; // Time in seconds before the bullet is destroyed
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //go foward in x direction
        transform.Translate(Vector3.left * Time.deltaTime * 10);

    }

    private void Update()
    {
        //decrease life time
        lifeTime -= Time.deltaTime;

        //if life time is less than 0, destroy the bullet
        if (lifeTime < 0)
        {
            Destroy(gameObject);
        }
    }
}
