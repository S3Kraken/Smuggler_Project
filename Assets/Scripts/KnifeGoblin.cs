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
    Animator anim;

    Vector2 spawnLoc;
    Vector2 leftEdge;
    Vector2 rightEdge;

    float speed;
    float chaseSpeed;

    bool patrolingRight;
    bool patrolingLeft;
    
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
        
        //check distance to player on x axis
        if (math.distance(transform.position, player.position) < 5)
        {
            Chase();
        }
        else if (patrolingRight)
            PatrolRight();
        else if (patrolingLeft)
            PatrolLeft();
        else
        {
            StartPatrol();
        }
    }

    public void StartPatrol()
    {
        anim.CrossFade("Run",0,0);
        //get a random number between 1 and 2
        Random = new Random();

        //Walk to the left if were not at the left edge
        if (transform.position.x > leftEdge.x)
        {
            patrolingLeft = true;
        }
        else if (transform.position.x < rightEdge.x)
        {
            patrolingRight = true;
        }
    }
    public void PatrolRight()
    {
        if (transform.position.x > leftEdge.x)
        {
            transform.localScale = new Vector3(-1, 1, 1);
            transform.Translate(Vector2.left * Time.deltaTime * speed);
        }
        else
        {
            StartCoroutine(nameof(Idle));
            patrolingRight = false;

        }
    }

    public void PatrolLeft()
    {
        if (transform.position.x < rightEdge.x)
        {
            transform.localScale = new Vector3(1, 1, 1);
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
        //move towards player x
        transform.movetow
    }

    private IEnumerator Idle()
    {
        anim.CrossFade(nameof(Idle), 0, 0);
        yield return new WaitForSeconds(3f);
    }
}
