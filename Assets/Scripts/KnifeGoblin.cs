using System;
using System.Collections;
using System.Linq;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class KnifeGoblin : MonoBehaviour
{
    Transform player;
    [SerializeField] BoxCollider2D attackHitbox;
    Animator anim;

    Vector2 spawnLoc;
    Vector2 leftEdge;
    Vector2 rightEdge;

    float lastXPos;
    float speed = 3;
    float chaseSpeed = 7;


    bool patrolingToSpawn;
    bool patrolingLeft;
    bool patrolingRight;
    bool idleDone = true;
    bool attackIsDone = true;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        player = GameObject.Find("Player").GetComponent<Transform>();
        anim = GetComponent<Animator>();
    }
    void Start()
    {
        spawnLoc = transform.position;
        leftEdge = new Vector2(transform.position.x - 10, transform.position.y);
        rightEdge = new Vector2(transform.position.x + 10, transform.position.y);
    }

    // Update is called once per frame
    void Update()
    {
        CheckFacingDirection();

        //check distance to player on x axis
        if (!attackIsDone)
            return;
        if (math.distance(transform.position, player.position) < 10)
        {
            idleDone = true;
            Chase();
        }
        else if (!idleDone)
            return;
        else if (patrolingToSpawn)
            PatrolToSpawn();
        else if (patrolingLeft)
            PatrolLeft();
        else if (patrolingRight)
            PatrolRight();
        else
        {
            StartPatrol();
        }
    }

    private void CheckFacingDirection()
    {
        if (transform.position.x < lastXPos)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
        else if (transform.position.x > lastXPos)
        {
            transform.localScale = new Vector3(1, 1, 1);
        }

        lastXPos = transform.position.x;
    }

    public void StartPatrol()
    {
        //get a random number between 1 and 2
        if (UnityEngine.Random.Range(0, 2) == 0)
        {
            patrolingRight = true;
        }
        else
        {
            patrolingLeft = true;
        }
    }
    public void PatrolToSpawn()
    {
        if (transform.position.x == spawnLoc.x)
        {
            anim.CrossFade("Run", 0, 0);
            transform.Translate(math.sign(spawnLoc.x - transform.position.x) * Time.deltaTime * speed, 0, 0);
        }
        else
        {
            StartCoroutine(nameof(Idle));
            patrolingToSpawn = false;
        }
    }
    public void PatrolLeft()
    {
        if (transform.position.x >= leftEdge.x)
        {
            anim.CrossFade("Run", 0, 0);
            transform.Translate(Vector2.left * Time.deltaTime * speed);
        }
        else
        {
            StartCoroutine(nameof(Idle));
            patrolingLeft = false;

        }
    }

    public void PatrolRight()
    {
        if (transform.position.x <= rightEdge.x)
        {
            anim.CrossFade("Run", 0, 0);
            transform.Translate(Vector2.right * Time.deltaTime * speed);
        }
        else
        {
            StartCoroutine(nameof(Idle));
            patrolingRight = false;
        }
    }

    public void Chase()
    {
        anim.CrossFade("Run", 0, 0);

        //move towards player x
        transform.Translate(math.sign(player.position.x - transform.position.x) * chaseSpeed * Time.deltaTime, 0, 0);

        if (math.distance(transform.position, player.position) < 2)
        {
            StartCoroutine(nameof(Attack));
        }
    }
    public IEnumerator Attack()
    {
        attackIsDone = false;
        anim.CrossFade("Attack", 0, 0);
        yield return new WaitForSeconds(anim.GetCurrentAnimatorStateInfo(0).length);
        attackIsDone = true;
    }
    

    private IEnumerator Idle()
    {
        idleDone = false;
        anim.CrossFade(nameof(Idle), 0, 0);
        yield return new WaitForSeconds(3f);
        idleDone = true;
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.gameObject.GetComponent<PlayerHealth>().TakeDamage(20);
        }
    }
}
