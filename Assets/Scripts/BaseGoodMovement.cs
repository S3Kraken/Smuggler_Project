using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using UnityEngine.Windows;
using static Unity.Burst.Intrinsics.X86.Avx;
using static UnityEngine.UI.Image;

public class BaseGoodMovement : MonoBehaviour
{
    private Transform tf;
    private Rigidbody2D rb;
    private float movex, movey;
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

    bool slopeBoost = false;
    float slopeBoostTimer = 2f;

    bool jumping = false;


    [SerializeField] float groundCheckRadius = 20f;
    [SerializeField] float groundCheckDistance = .5f;

    bool onSlope;
    float slopeAngle;
    Transform groundCheckEmpty;

    Vector2 slopeNormalPerp;
    bool isDownhill;
    public bool useTransform = false;
    private Vector2 smoothVelocity = Vector2.zero;

    void Start()
    {
        tf = GetComponent<Transform>();
        rb = GetComponent<Rigidbody2D>();
        groundCheckEmpty = GameObject.Find("GroundCheckEmpty").GetComponent<Transform>();
        onSlopeText = GameObject.Find("Slope (TMP)").GetComponent<TMPro.TextMeshProUGUI>();
        ChangeWeightButtonText = GameObject.Find("ChangeWeightButton (TMP)").GetComponent<TMPro.TextMeshProUGUI>();
        ChangeWeightButtonText.text = weight;
    }


    void FixedUpdate()
    {
        onSlopeText.text = "On Slope: " + onSlope;
        //onSlopeText.text = "On Slope: " + (canClimb && movey != 0);
        slopeBoostTimer += Time.deltaTime;
        if (slopeBoostTimer > 1f)
        {
            switch (weight)
            {
                case "low": speed = lowWeightSpeed; break;
                case "med": speed = medWeightSpeed; break;
                case "high": speed = highWeightSpeed; break;
            }
        }

        float targetX = movex * speed;

        if (IsGrounded)
        {
            jumping = false;
        }

        if (!useTransform)
        {
            if (!onSlope)
            {
                rb.gravityScale = 2f; // Ensure gravity is normal when not on slope
                if (movex == 0f)
                {
                    // Smoothly decelerate to 0
                    rb.linearVelocity = new Vector2(
                        Mathf.MoveTowards(rb.linearVelocity.x, 0, 60f * Time.fixedDeltaTime),  // ground decel
                        rb.linearVelocity.y);
                }
                else
                {
                    //Smoothly accelerate to target speed
                    rb.linearVelocity = new Vector2(
                        Mathf.MoveTowards(rb.linearVelocity.x, targetX, 80f * Time.fixedDeltaTime),  // accel
                        rb.linearVelocity.y);
                }
            }
            else if (onSlope && movex == 0f)
            {
                rb.linearVelocity = new Vector2(Mathf.MoveTowards(rb.linearVelocity.x, 0, 60f * Time.fixedDeltaTime), 0f);
                rb.gravityScale = 0f;
            }
            else
            {
                isDownhill = (slopeNormalPerp.y > 0f && movex > 0f) || (slopeNormalPerp.y < 0f && movex < 0f);
                rb.gravityScale = 2f; // Ensure gravity is normal when not on slope
                if (false)
                {
                    float slopeSpeed = speed * 3f;
                    float slopeAccel = 60f * Time.fixedDeltaTime;
                    float currentX = rb.linearVelocity.x;
                    float targetSlopeVelX = slopeSpeed * slopeNormalPerp.x * -movex;
                    float targetSlopeVelY = slopeSpeed * slopeNormalPerp.y * -movex;

                    rb.linearVelocity = new Vector2(
                        Mathf.MoveTowards(currentX, targetSlopeVelX, slopeAccel),
                        Mathf.MoveTowards(rb.linearVelocity.y, targetSlopeVelY, slopeAccel)
                    ); ;
                }
                else
                {
                    rb.linearVelocity = new Vector2(speed * slopeNormalPerp.x * -movex, speed * slopeNormalPerp.y * -movex);
                    Debug.Log($"Moving on slope with velocity: {rb.linearVelocity}");
                    Debug.Log($"Slope normal: {slopeNormalPerp}, movex: {movex}");
                }
            }
        }
        else
        {
            if (!onSlope)
            {
                rb.gravityScale = 2f; // Ensure gravity is normal when not on slope
                if (movex == 0f)
                {
                    // Smoothly decelerate
                    smoothVelocity.x = Mathf.MoveTowards(smoothVelocity.x, 0, 60f * Time.fixedDeltaTime);
                }
                else
                {
                    // Smoothly accelerate
                    smoothVelocity.x = Mathf.MoveTowards(smoothVelocity.x, targetX, 80f * Time.fixedDeltaTime);
                }
            }
            else if (onSlope && movex == 0f)
            {
                smoothVelocity.x = Mathf.MoveTowards(smoothVelocity.x, 0, 60f * Time.fixedDeltaTime);
                rb.gravityScale = 0f;
            }
            else
            {
                //isDownhill = (slopeNormalPerp.y > 0f && movex > 0f) || (slopeNormalPerp.y < 0f && movex < 0f);
                //rb.gravityScale = 2f; // Ensure gravity is normal when not on slope
                //if (isDownhill)
                //{
                //    float slopeSpeed = speed * 3f;
                //    float slopeAccel = 60f * Time.fixedDeltaTime;
                //    float currentX = rb.linearVelocity.x;
                //    float targetSlopeVelX = slopeSpeed * slopeNormalPerp.x * -movex;
                //    float targetSlopeVelY = slopeSpeed * slopeNormalPerp.y * -movex;

                //    rb.linearVelocity = new Vector2(
                //        Mathf.MoveTowards(currentX, targetSlopeVelX, slopeAccel),
                //        Mathf.MoveTowards(rb.linearVelocity.y, targetSlopeVelY, slopeAccel)
                //    ); ;
                //}
                //else
                //{
                //    rb.linearVelocity = new Vector2(speed * slopeNormalPerp.x * -movex, speed * slopeNormalPerp.y * -movex);
                //    Debug.Log($"Moving on slope with velocity: {rb.linearVelocity}");
                //    Debug.Log($"Slope normal: {slopeNormalPerp}, movex: {movex}");
                //}
            }
            tf.position += (Vector3)smoothVelocity * Time.fixedDeltaTime;
        }


        GroundAndSlopeDetection();
        //if (IsGrounded)
        //{
        //    float rayDistance = 2f;
        //    Vector2 origin = transform.position;

        //    Debug.DrawRay(origin, Vector2.down * rayDistance, Color.green);

        //    RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, rayDistance, groundLayer);

        //    if (hit.collider != null)
        //    {
        //        float angle = Vector2.Angle(hit.normal, Vector2.up);
        //        Debug.Log("Hit: " + hit.collider.name + " angle: " + angle);
        //        onSlope = angle > 0; // Consider it a slope if the angle is greater than 0
        //    }
        //}
    }


    void GroundAndSlopeDetection()
    {
        // Start a bit below the center (near feet)
        Vector2 origin = groundCheckEmpty.position;

        // 1) Circle downwards to detect ground
        //    RaycastHit2D hit = Physics2D.CircleCast(
        //    origin,
        //    groundCheckRadius,
        //    Vector2.down,
        //    groundCheckDistance,
        //    groundLayer
        //);

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
            slopeNormalPerp = Vector2.Perpendicular(hit.normal).normalized;
            //Debug.Log($"Grounded on {hit.collider.name}");
            // Slope info
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
