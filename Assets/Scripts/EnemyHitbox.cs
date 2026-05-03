using UnityEngine;

public class EnemyHitbox : MonoBehaviour
{
    public void OnTriggerStay2D(Collider2D collision)
    {
        print("Son you got stabbed");
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.gameObject.GetComponent<PlayerHealth>().TakeDamage(30);
        }
    }
}
