using UnityEngine;

public class PlayerSmokeFXs : MonoBehaviour
{
    Rigidbody2D playerRB;
    Animator anim;
    void Awake()
    {
        playerRB = GameObject.Find("Player").GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //if (playerRB.linearVelocity.y < 0)
        //{
        //    anim.Play("Smoke_Land");
        //    Destroy(gameObject, anim.GetCurrentAnimatorStateInfo(0).length);
        //}
        //else if (playerRB.linearVelocity.y > 0)
        //{
        //    anim.Play("Smoke_Jump");
        //    Destroy(gameObject, anim.GetCurrentAnimatorStateInfo(0).length);
        //}
        //else
        //{
        //    anim.Play("Smoke_Land");
        //    Destroy(gameObject, anim.GetCurrentAnimatorStateInfo(0).length);
        //}
    }

    public void JumpSmoke()
    {
        anim.Play("Smoke_Jump");
        Destroy(gameObject, anim.GetCurrentAnimatorStateInfo(0).length);
    }
    public void LandSmoke()
    {
        anim.Play("Smoke_Land");
        Destroy(gameObject, anim.GetCurrentAnimatorStateInfo(0).length);
    }
}
