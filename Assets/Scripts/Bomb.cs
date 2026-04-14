using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class Bomb : MonoBehaviour
{
    Vector3 playerPos;
    Animator anim;
    Rigidbody2D rb;
    bool grounded = false;
    //Vector3 groundCheckPos;
    [SerializeField] Vector2 _groundCheckSize = new Vector2(0.49f, 0.03f);
    [SerializeField] float yOffset = 0.12253f;

    [SerializeField] LayerMask _groundLayer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        playerPos = GameObject.Find("Player").transform.position;
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 bottom = GetComponent<Renderer>().bounds.center - transform.up * GetComponent<Renderer>().bounds.extents.y;
        if (Physics2D.OverlapBox(transform.position - (Vector3.up * transform.localScale.y / 1.6f), _groundCheckSize, 0, _groundLayer) && !grounded) //checks if set box overlaps with ground
        {
            StartCoroutine(nameof(Grounded));
            rb.linearVelocity = new Vector2(0,0);
            rb.gravityScale = 0;
        }
    }

    public void Launch(Vector2 facing, bool inRange)
    {
        if (facing == Vector2.left)
            transform.localScale = new Vector3(-1, 1, 1);
        if (inRange)
        {
            //arc towards player
            Vector2 direction = (playerPos + new Vector3(0,5,0) - transform.position).normalized;
            rb.linearVelocity = direction * 10f;
            anim.CrossFade("Thrown", 0, 0);

        }
        else
        {
            Vector2 direction = (transform.position + new Vector3(15 * facing.x, 5, 0) - transform.position).normalized;
            rb.linearVelocity = direction * 10f;
            anim.CrossFade("Thrown", 0, 0);
            Debug.Log("Launched bomb in direction: " + facing);
        }
    }
    public IEnumerator Grounded()
    {
        grounded = true;
        anim.CrossFade("Ground", 0, 0);
        yield return new WaitForSeconds(anim.GetCurrentAnimatorStateInfo(0).length);
        if (!anim.GetCurrentAnimatorStateInfo(0).IsName("Explode"))
        {
            StartCoroutine(nameof(Explode));
        }
    }

    public IEnumerator Explode()
    {
        rb.linearVelocity = new Vector2(0, 0);
        rb.gravityScale = 0;

        anim.CrossFade("Explode", 0, 0);
        transform.position = new Vector3(transform.position.x, transform.position.y + 2, transform.position.z);
        yield return new WaitForSeconds(anim.GetCurrentAnimatorStateInfo(0).length);
        Destroy(this.gameObject);
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(new Vector3 (transform.position.x, transform.position.y -0.6665f +  yOffset, transform.position.z), _groundCheckSize);
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.gameObject.GetComponent<PlayerHealth>().TakeDamage(30);
            if (!anim.GetCurrentAnimatorStateInfo(0).IsName("Explode"))
            {
                StartCoroutine(nameof(Explode));
            }
        }
    }
}