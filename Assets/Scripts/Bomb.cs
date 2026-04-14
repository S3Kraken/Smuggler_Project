using System.Collections;
using UnityEngine;

public class Bomb : MonoBehaviour
{
    Transform player;
    Animator anim;
    Rigidbody2D rb;
    bool grounded;

    LayerMask _groundLayer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GameObject.Find("Player").GetComponent<Transform>();
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Physics2D.OverlapBox(transform.position, new Vector2(3,3), 0, _groundLayer) && !grounded) //checks if set box overlaps with ground
        {
            StartCoroutine(nameof(Grounded));
        }

    }

    public void Launch(Vector2 facing)
    {
        if (facing == Vector2.left)
            transform.localScale = new Vector3(-1, 1, 1);

        //arc towards player
        Vector2 direction = (player.position - transform.position).normalized;
        rb.linearVelocity = direction * 5f;
        anim.CrossFade("Thrown", 0, 0);
    }
    public IEnumerable Grounded()
    {
        grounded = true;
        anim.CrossFade("Ground", 0, 0);
        yield return new WaitForSeconds(anim.GetCurrentAnimatorStateInfo(0).length);
        if (!anim.GetCurrentAnimatorStateInfo(0).IsName("Explode"))
        {
            StartCoroutine(nameof(Explode));
        }
    }

    public IEnumerable Explode()
    {
        anim.CrossFade("Explode", 0, 0);
        yield return new WaitForSeconds(anim.GetCurrentAnimatorStateInfo(0).length);
        Destroy(this.gameObject);
    }
}