using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using UnityEngine.Windows;
using static Unity.Burst.Intrinsics.X86.Avx;
using static UnityEngine.UI.Image;
using Input = UnityEngine.Input;

public class BaseGoodMovement : MonoBehaviour
{
    private Transform tf;
    private Rigidbody2D rb;
    private float movex, movey;
    float oldSpeed;
    float speed = 5;
    string weight = "med"; // "low", "med", "high"
    float lowWeightSpeed = 12.5f;
    float medWeightSpeed = 10;
    float highWeightSpeed = 5f;
    private float health = 100;

    public ContactFilter2D groundFilter; // Set in Inspector (layer & angle)
    bool IsGrounded;

    private RaycastHit2D hit; // Store the result of the raycast
    private float slopeMultiplier = 10000; // Adjust this value to control how much the slope affects speed
    public TMPro.TextMeshProUGUI onSlopeText;
    public LayerMask groundLayer; // Set this to the layer(s) that represent the ground in your game

    public bool canClimb = false;
    public TMPro.TextMeshProUGUI ChangeWeightButtonText;

    bool jumping = false;
    bool sliding = false;
    float deceleration = 80f;
    float slideSpeedTimer = 2f;


    [SerializeField] float groundCheckRadius = 20f;
    float groundCheckDistance = 0f;
    [SerializeField] float slopeCheckDistance = 1f;


    bool onSlope;
    float slopeAngle;
    Transform groundCheckEmpty;

    Vector2 slopeNormalPerp;
    bool isDownhill;
    private Vector2 smoothVelocity = Vector2.zero;
    bool slidingDownSlope = false;
    float timer = 0f;
    bool wasJustSliding = false;

    void Start()
    {
        tf = GetComponent<Transform>();
        rb = GetComponent<Rigidbody2D>();
        groundCheckEmpty = GameObject.Find("GroundCheckEmpty").GetComponent<Transform>();
        onSlopeText = GameObject.Find("Slope (TMP)").GetComponent<TMPro.TextMeshProUGUI>();
        ChangeWeightButtonText = GameObject.Find("ChangeWeightButton (TMP)").GetComponent<TMPro.TextMeshProUGUI>();
        ChangeWeightButtonText.text = weight;

        switch (weight)
        {
            case "low": speed = lowWeightSpeed; break;
            case "med": speed = medWeightSpeed; break;
            case "high": speed = highWeightSpeed; break;
        }
    }
    private void Update()
    {
        timer += Time.deltaTime;
        if (slidingDownSlope)
        {
            float tempSpeed = 0;
            switch (weight)
            {
                case "low": tempSpeed = lowWeightSpeed; break;
                case "med": tempSpeed = medWeightSpeed; break;
                case "high": tempSpeed = highWeightSpeed; break;
            }
            slideSpeedTimer += Time.deltaTime;
            //every .5 seconds, increase the slide speed by 10% up to a maximum of 3x the normal speed
            if (slideSpeedTimer >= 0.5f)
            {
                Debug.Log($"Increasing slide speed. Current speed: {speed}");
                speed *= 3f;
                if (speed > tempSpeed * 3f)
                {
                    speed = tempSpeed * 3f;
                }
                slideSpeedTimer = 0f; // Reset the timer
            }
        }
        else if (wasJustSliding)
        {
            StartCoroutine(CarrySpeed());
        }

        if (Input.GetKey(KeyCode.LeftShift))
        {
            sliding = true;
        }
        else
        {
            sliding = false;
        }

        //onSlopeText.text = "On Slope: " + (canClimb && movey != 0);
        onSlopeText.text = "On Slope: " + onSlope;
    }
    IEnumerator CarrySpeed()
    {
        yield return new WaitForSeconds(1.0f);
        slideSpeedTimer = 0f;
        switch (weight)
        {
            case "low": speed = lowWeightSpeed; break;
            case "med": speed = medWeightSpeed; break;
            case "high": speed = highWeightSpeed; break;
        }
    }

    void FixedUpdate()
    {
        wasJustSliding = slidingDownSlope;

        float targetX = movex * speed;

        if (IsGrounded)
        {
            jumping = false;
        }

        if (!onSlope)
        {
            slidingDownSlope = false;
            if (!canClimb)
            {
                rb.gravityScale = 2f; // Ensure gravity is normal when not on slope
            }
            if (movex == 0f)
            {
                // Smoothly decelerate to 0
                rb.linearVelocity = new Vector2(Mathf.MoveTowards(rb.linearVelocity.x, 0, 60f * Time.fixedDeltaTime), rb.linearVelocity.y);
            }
            else
            {
                //Smoothly accelerate to target speed
                rb.linearVelocity = new Vector2(
                    Mathf.MoveTowards(rb.linearVelocity.x, targetX, deceleration * Time.fixedDeltaTime),  // accel
                    rb.linearVelocity.y);
            }
        }
        else if (onSlope && movex == 0f)
        {
            rb.linearVelocity = new Vector2(Mathf.MoveTowards(rb.linearVelocity.x, 0, 60f * Time.fixedDeltaTime), 0f);
            rb.gravityScale = -0f;
        }
        else
        {
            isDownhill = (slopeNormalPerp.y > 0f && movex > 0f) || (slopeNormalPerp.y < 0f && movex < 0f);
            if (isDownhill && rb.linearVelocityY < 0 && sliding)
            {
                rb.gravityScale = 10f; // Disable gravity while sliding down slope
                slidingDownSlope = true;
                if (!(oldSpeed < speed))
                {

                }
                float slopeSpeed = speed * 3f;
                float slopeAccel = 60f * Time.fixedDeltaTime;
                float currentX = rb.linearVelocity.x;
                float targetSlopeVelX = speed * slopeNormalPerp.x * -movex;
                float targetSlopeVelY = speed * slopeNormalPerp.y * -movex;

                rb.linearVelocity = new Vector2(
                    Mathf.MoveTowards(currentX, targetSlopeVelX, slopeAccel),
                    Mathf.MoveTowards(rb.linearVelocity.y, targetSlopeVelY, slopeAccel)
                ); ;
            }
            else
            {
                rb.gravityScale = 2f; // Ensure gravity is normal when not on slope
                slidingDownSlope = false;
                rb.linearVelocity = new Vector2(speed * slopeNormalPerp.x * -movex, speed * slopeNormalPerp.y * -movex);
                //Debug.Log($"Moving on slope with velocity: {rb.linearVelocity}");
                //Debug.Log($"Slope normal: {slopeNormalPerp}, movex: {movex}");
            }
        }



        GroundAndSlopeDetection();
    }

    void GroundDetection()
    {
        Vector2 origin = groundCheckEmpty.position;

        RaycastHit2D hit = Physics2D.Raycast(
            origin,
            Vector2.down,
            groundCheckDistance,
            groundLayer
        );

        // Debug visualize
        Debug.DrawRay(origin, Vector2.down * (groundCheckDistance), Color.green);

        if (hit.collider != null)
        {
            IsGrounded = true;
        }
        else
        {
            IsGrounded = false;
        }
    }

    void GroundAndSlopeDetection()
    {
        Vector2 origin = groundCheckEmpty.position;

        RaycastHit2D hit = Physics2D.Raycast(
            origin,
            Vector2.down,
            slopeCheckDistance,
            groundLayer
        );

        // Debug visualize
        Debug.DrawRay(origin, Vector2.down * (slopeCheckDistance), Color.green);

        if (hit.collider != null)
        {
            IsGrounded = true;
            slopeNormalPerp = Vector2.Perpendicular(hit.normal).normalized;
            //Debug.Log($"Grounded on {hit.collider.name}");
            // Slope info
            Debug.Log($"Hit: {hit.collider.name} at distance {hit.distance}");
            slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
            onSlope = slopeAngle > 1f;
            // small tolerance so flat ground isn�t �slope�
            // Debug.Log($"Hit {hit.collider.name}, angle {slopeAngle}");
            //if (onSlope && !jumping)
            //{
            //Vector2 temp = tf.position;
            //temp.y = hit.point.y + 2f; // Adjust 0.5f based on your character's pivot/height
            //tf.position = temp;
            //}
        }
        else
        {
            IsGrounded = false;
            onSlope = false;
            slopeAngle = 0f;
        }
    }

    private void OnMove(InputValue movementValue)
    {
        Vector2 movementVector = movementValue.Get<Vector2>();
        movex = movementVector.x;

        if (canClimb && movementVector.y != 0)
        {
            movey = movementVector.y;
            rb.gravityScale = 0f; // Disable gravity while climbing
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 5 * movementVector.y); // Stop any vertical velocity from physics while climbing

        }
        else if (canClimb && movementVector.y == 0)
        {
            movey = 0;
            rb.gravityScale = 0f; // Keep gravity disabled when not moving vertically on the ladder
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0); // Stop any vertical velocity from physics while on the ladder
        }
        else
        {
            movey = 0;
            rb.gravityScale = 2f; // Re-enable gravity when not climbing
        }


    }
    void OnJump()
    {
        if (IsGrounded)
        {
            jumping = true;
            rb.AddForce(new Vector2(0, 10), ForceMode2D.Impulse);
        }
    }

    void OnSlide()
    {

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

    public void ChangeWeight()
    {
        if (weight == "low")
        {
            weight = "med";
        }
        else if (weight == "med")
        {
            weight = "high";
        }
        else
        {
            weight = "low";
        }
        ChangeWeightButtonText.text = weight;
    }
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.name == "Ladders")
            canClimb = true;
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.name == "Ladders")
        {
            canClimb = false;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, Mathf.Min(rb.linearVelocity.y, 0f));
        }
    }
}
