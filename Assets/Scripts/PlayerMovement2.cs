//using System.Linq;
//using UnityEngine;
//using UnityEngine.InputSystem;
//using UnityEngine.UIElements;
//using static Unity.Burst.Intrinsics.X86.Avx;
//using static UnityEngine.UI.Image;

//public class PlayerMovement2 : MonoBehaviour
//{
//    private Transform tf;
//    private Rigidbody2D rb;
//    private float movex, movey;
//    float speed = 5;
//    string weight = "med"; // "low", "med", "high"
//    float lowWeightSpeed = 12.5f;
//    float medWeightSpeed = 10;
//    float highWeightSpeed = 5f;
//    private float health = 100;

//    public ContactFilter2D groundFilter; // Set in Inspector (layer & angle)
//    bool IsGrounded => rb.IsTouching(groundFilter);

//    private RaycastHit2D hit; // Store the result of the raycast
//    private float slopeMultiplier = 10000; // Adjust this value to control how much the slope affects speed
//    bool isOnSlope = false; // Track if the player is currently on a slope
//    public TMPro.TextMeshProUGUI onSlopeText;
//    public LayerMask groundLayer; // Set this to the layer(s) that represent the ground in your game

//    public bool canClimb = false;
//    public TMPro.TextMeshProUGUI ChangeWeightButtonText;

//    bool slopeBoost = false;
//    float slopeBoostTimer = 2f;

//    bool jumping = false;
//    private Vector2 smoothVelocity = Vector2.zero;

//    // Start is called once before the first execution of Update after the MonoBehaviour is created
//    void Start()
//    {
//        tf = GetComponent<Transform>();
//        rb = GetComponent<Rigidbody2D>();
//        onSlopeText = GameObject.Find("Slope (TMP)").GetComponent<TMPro.TextMeshProUGUI>();
//        ChangeWeightButtonText = GameObject.Find("ChangeWeightButton (TMP)").GetComponent<TMPro.TextMeshProUGUI>();
//        ChangeWeightButtonText.text = weight;
//    }

//    private void Update()
//    {
//        if (IsGrounded && !jumping)  // not jumping
//        {
//            // Tiny downward nudge to stick to slope
//            rb.gravityScale = 10;
//        }
//        else
//        {
//            rb.gravityScale = 2; // Normal gravity when not grounded or jumping
//        }
//    }
//    void FixedUpdate()
//    {
//        //onSlopeText.text = "On Slope: " + isOnSlope;
//        //onSlopeText.text = "On Slope: " + (canClimb && movey != 0);
//        slopeBoostTimer += Time.deltaTime;
//        if (slopeBoostTimer > 1f)
//        {
//            switch (weight)
//            {
//                case "low": speed = lowWeightSpeed; break;
//                case "med": speed = medWeightSpeed; break;
//                case "high": speed = highWeightSpeed; break;
//            }
//        }


//        if (IsGrounded)
//        {
//            jumping = false;
//        }
//        float targetX = movex * speed;

//        if (movex == 0f)
//        {
//            // Smoothly decelerate
//            smoothVelocity.x = Mathf.MoveTowards(smoothVelocity.x, 0, 60f * Time.fixedDeltaTime);
//        }
//        else
//        {
//            // Smoothly accelerate
//            smoothVelocity.x = Mathf.MoveTowards(smoothVelocity.x, targetX, 40f * Time.fixedDeltaTime);
//        }

//        // Apply as position change (same math as your rb.linearVelocity version)
//        tf.position += (Vector3)smoothVelocity * Time.fixedDeltaTime;
    

//        if (IsGrounded)
//        {
//            float rayDistance = 2f;
//            Vector2 origin = transform.position;

//            Debug.DrawRay(origin, Vector2.down * rayDistance, Color.green);

//            RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, rayDistance, groundLayer);

//            if (hit.collider != null)
//            {
//                float angle = Vector2.Angle(hit.normal, Vector2.up);
//                Debug.Log("Hit: " + hit.collider.name + " angle: " + angle);
//                isOnSlope = angle > 0; // Consider it a slope if the angle is greater than 0
//            }
//        }
//    }
//    private void OnMove(InputValue movementValue)
//    {
//        Vector2 movementVector = movementValue.Get<Vector2>();
//        movex = movementVector.x;

//        if (canClimb && movementVector.y != 0)
//        {
//            movey = movementVector.y;
//            rb.gravityScale = 0f; // Disable gravity while climbing
//            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 5 * movementVector.y); // Stop any vertical velocity from physics while climbing

//        }
//        else if (canClimb && movementVector.y == 0)
//        {
//            movey = 0;
//            rb.gravityScale = 0f; // Keep gravity disabled when not moving vertically on the ladder
//            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0); // Stop any vertical velocity from physics while on the ladder
//        }
//        else
//        {
//            movey = 0;
//            rb.gravityScale = 2f; // Re-enable gravity when not climbing
//        }


//    }
//    void OnJump()
//    {
//        if (IsGrounded)
//        {
//            jumping = true;
//            rb.AddForce(new Vector2(0, 10), ForceMode2D.Impulse);
//        }
//    }

//    void OnSlide()
//    {
//        if (isOnSlope)
//        {
//            speed = 20f; // Increase speed when sliding down a slope
//            slopeBoost = true;
//            slopeBoostTimer = 0f;
//            //Vector2 normal2D = hit.normal.normalized;
//            //Vector2 tangent2D = new Vector2(-normal2D.y, normal2D.x); // perpendicular to normal

//            //// Your intended move direction in 2D
//            //Vector2 moveDir2D = new Vector2(movex, movey).normalized;

//            //// How much you're moving along the slope (signed)
//            //float slopeFactor = Vector2.Dot(moveDir2D, tangent2D);

//            //// Apply slope-based speed boost (use a baseSpeed so it doesn't explode)
//            //float finalSpeed = speed * (1f + slopeFactor * slopeMultiplier);

//            //// Move along your original move vector
//            //Vector3 move = new Vector3(movex, movey, 0f).normalized;
//            //tf.position += move * finalSpeed * Time.fixedDeltaTime;
//            //rb.AddForce(tangent2D * slopeFactor * slopeMultiplier, ForceMode2D.Force);
//            //Debug.Log("Moving on slope with final speed: " + finalSpeed);
//        }
//    }
//    public void Heal(float amount)
//    {
//        health += amount;
//        if (health > 100)
//        {
//            health = 100;
//        }
//        //healthText.text = "Health: " + health;
//    }

//    public void ChangeWeight()
//    {
//        if (weight == "low")
//        {
//            weight = "med";
//        }
//        else if (weight == "med")
//        {
//            weight = "high";
//        }
//        else
//        {
//            weight = "low";
//        }
//        ChangeWeightButtonText.text = weight;
//    }
//    void OnTriggerEnter2D(Collider2D collision)
//    {
//        if (collision.gameObject.name == "Ladders")
//            canClimb = true;
//    }

//    void OnTriggerExit2D(Collider2D collision)
//    {
//        if (collision.gameObject.name == "Ladders")
//        {
//            canClimb = false;
//            rb.linearVelocity = new Vector2(rb.linearVelocity.x, Mathf.Min(rb.linearVelocity.y, 0f));
//        }
//    }
//}