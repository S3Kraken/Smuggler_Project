using System;
using System.Collections;
using System.Linq;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class BombGoblin : MonoBehaviour
{
    Transform player;
    [SerializeField] GameObject bomb;
    Animator anim;
    [SerializeField] bool justIdle;

    float lockState = 0;
    float attackCooldown = 5f;

    bool facingRight;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        player = GameObject.Find("Player").GetComponent<Transform>();
        anim = GetComponent<Animator>();
    }
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        lockState -= Time.deltaTime;
        attackCooldown -= Time.deltaTime;
        CheckFacingDirection();

        if (justIdle)
        {
            Idle();
            return;
        }

        if (lockState > 0)
        {
            return;
        }
        else if (math.distance(transform.position.x, player.position.x) <= 15 && attackCooldown < 0)
        {
            StartCoroutine(nameof(Attack), true);
        }
        else if (math.distance(transform.position.x, player.position.x) <= 25 && attackCooldown < 0)
        {
            StartCoroutine(nameof(Attack), false);
        }
        else
        {
            Idle();
        }
    }

    private void CheckFacingDirection()
    {
        if (transform.position.x > player.position.x)
        {
            transform.localScale = new Vector3(-1, 1, 1);
            facingRight = false;
        }
        else if (transform.position.x < player.position.x)
        {
            transform.localScale = new Vector3(1, 1, 1);
            facingRight = true;
        }
    }
    public IEnumerator Attack(bool inRange)
    {
        anim.CrossFade("Attack", 0, 0);
        lockState = anim.GetCurrentAnimatorStateInfo(0).length;
        attackCooldown = 3 + anim.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(.19f/.60f);

        Bomb spawnedBomb = Instantiate(bomb, transform.position + new Vector3(-0.669f, 0.82f, 0), quaternion.identity).GetComponent<Bomb>();
        if (facingRight)
        {
            spawnedBomb.Launch(Vector2.right, inRange);
        }
        else
        {
            spawnedBomb.Launch(Vector2.left, inRange);
        }
    }


    public void Idle()
    {
        anim.CrossFade(nameof(Idle), 0, 0);
    }
    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.gameObject.GetComponent<PlayerHealth>().TakeDamage(10);
        }
    }
}
