using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class PlayerMovement : MonoBehaviour
{
    private Transform tf;
    private Rigidbody2D rb;
    private float movex, movey;
    public float speed = 5;
    private float health = 100;

    public ContactFilter2D groundFilter; // Set in Inspector (layer & angle)
    bool IsGrounded => rb.IsTouching(groundFilter);

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        tf = GetComponent<Transform>();
        rb = GetComponent<Rigidbody2D>();
    }
    void FixedUpdate()
    {
        Vector3 move = new Vector3(movex, movey, 0);
        tf.position += move * speed * Time.fixedDeltaTime;
    }
    private void OnMove(InputValue movementValue)
    {
        Vector2 movementVector = movementValue.Get<Vector2>();
        movex = movementVector.x;
        //movey = movementVector.y;
    }
    void OnJump()
    {
        if (IsGrounded)
        {
            rb.AddForce(new Vector2(0, 10), ForceMode2D.Impulse);
        }
    }
    public void Heal(float amount)
    {
        health += amount;
        if (health > 100)
        {
            health = 100;
        }
        //healthText.text = "Health: " + health;
    }
}