using UnityEngine;

public class EnemyHitbox : MonoBehaviour
{
    public void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.gameObject.GetComponent<PlayerHealth>().TakeDamage(30);
        }
    }
}
