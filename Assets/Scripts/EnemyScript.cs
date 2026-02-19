using System.Collections;
using UnityEngine;

public class EnemyScript : MonoBehaviour
{
    GameObject player;
    public GameObject bullet;
    bool canShoot = true;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GameObject.Find("Player");
        //get bullet prefab

    }

    // Update is called once per frame
    void Update()
    {
        //if the player is within 20 units of the enemy, shoot at the player
        if (Vector2.Distance(transform.position, player.transform.position) < 20 && canShoot)
        {
            //shoot at the player
            Vector2 direction = (player.transform.position - transform.position).normalized;

            Instantiate(bullet, transform.position, Quaternion.FromToRotation(-Vector2.right, direction));

            StartCoroutine(ShootCooldown());
        }
    }

    IEnumerator ShootCooldown()
    {
        canShoot = false;
        yield return new WaitForSeconds(2f);
        canShoot = true;
    }
}
