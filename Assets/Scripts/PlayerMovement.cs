//using System.Linq;
//using UnityEngine;
//using UnityEngine.InputSystem;
//using UnityEngine.UIElements;
//using static Unity.Burst.Intrinsics.X86.Avx;
//using static UnityEditor.Searcher.SearcherWindow.Alignment;
//using static UnityEngine.UI.Image;

//public class PlayerMovement : MonoBehaviour
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
//    bool IsGrounded;

//    private RaycastHit2D hit; // Store the result of the raycast
//    private float slopeMultiplier = 10000; // Adjust this value to control how much the slope affects speed
//    public TMPro.TextMeshProUGUI onSlopeText;
//    public LayerMask groundLayer; // Set this to the layer(s) that represent the ground in your game

//    public bool canClimb = false;
//    public TMPro.TextMeshProUGUI ChangeWeightButtonText;

//    bool slopeBoost = false;
//    float slopeBoostTimer = 2f;

//    bool jumping = false;


//    [SerializeField] float groundCheckRadius = 20f;
//    float groundCheckDistance = 1f;

//    bool onSlope;
//    float slopeAngle;
//    Transform groundCheckEmpty;

//    void Start()
//    {
//        tf = GetComponent<Transform>();
//        rb = GetComponent<Rigidbody2D>();
//        groundCheckEmpty = GameObject.Find("GroundCheckEmpty").GetComponent<Transform>();
//        onSlopeText = GameObject.Find("Slope (TMP)").GetComponent<TMPro.TextMeshProUGUI>();
//        ChangeWeightButtonText = GameObject.Find("ChangeWeightButton (TMP)").GetComponent<TMPro.TextMeshProUGUI>();
//        ChangeWeightButtonText.text = weight;
//    }


//    void FixedUpdate()
//    {
//        //onSlopeText.text = "On Slope: " + onSlope;
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

//        float targetX = movex * speed;

//        if (IsGrounded)
//        {
//            jumping = false;

//            // SLIDE instead of manual velocity!
//            Vector2 slideVelocity = new Vector2(targetX, rb.linearVelocity.y);
//            var slideMovement = new Rigidbody2D.SlideMovement
//            {
//                surfaceSlideAngle = 70f,  // Stick to slopes up to 70°
//                surfaceAnchor = new Vector2(0, -1f)  // Look down 0.1 units for ground
//            };
//            rb.Slide(slideVelocity, Time.fixedDeltaTime, slideMovement);
//        }
//        else
//        {
//            // Airborne: simple velocity control
//            float targetAirX = movex * speed * 0.7f;  // Reduced air control
//            rb.linearVelocity = new Vector2(
//                Mathf.MoveTowards(rb.linearVelocity.x, targetAirX, 40f * Time.fixedDeltaTime),
//                rb.linearVelocity.y
//            );
//        }

//        //if (onSlope)
//        //{
//        //    RaycastHit2D hit = Physics2D.Raycast(groundCheckEmpty.position, Vector2.down, groundCheckDistance, groundLayer);
//        //    if (hit.collider != null)
//        //    {
//        //        Vector2 groundNormal = hit.normal;
//        //        float slopeSteepness = Vector2.Dot(groundNormal, Vector2.up);  // 1=flat, 0=90°

//        //        if (slopeSteepness < .95f)  // On slope
//        //        {
//        //            // Movement direction = perpendicular to normal (along slope surface)
//        //            Vector2 slopeDirection = Vector2.Perpendicular(groundNormal).normalized;

//        //            // Face correct direction (left/right slope)
//        //            if (Vector2.Dot(slopeDirection, Vector2.right) * movex < 0)
//        //                slopeDirection = -slopeDirection;

//        //            Vector2 slopeVelocity = slopeDirection * speed * Mathf.Abs(movex);

//        //            rb.linearVelocity = new Vector2(
//        //                Mathf.MoveTowards(rb.linearVelocity.x, slopeVelocity.x, 40f * Time.fixedDeltaTime),
//        //                rb.linearVelocity.y);

//        //            Debug.Log($"Moving on slope with target velocity: {slopeVelocity.x}");
//        //            // Stick to surface
//        //            //Vector2 targetPos = rb.position;
//        //            //targetPos.y = hit.point.y + 0.1f;
//        //            //rb.position = targetPos;
//        //        }
//        //        else
//        //        {
//        //            // Flat ground - original code
//        //            if (movex == 0f)
//        //            {
//        //                rb.linearVelocity = new Vector2(
//        //                    Mathf.MoveTowards(rb.linearVelocity.x, 0, 60f * Time.fixedDeltaTime),
//        //                    rb.linearVelocity.y);
//        //            }
//        //            else
//        //            {
//        //                rb.linearVelocity = new Vector2(
//        //                    Mathf.MoveTowards(rb.linearVelocity.x, targetX, 40f * Time.fixedDeltaTime),
//        //                    rb.linearVelocity.y);
//        //            }
//        //        }
//        //    }
//        //}
//        //else
//        //{
//        //    if (IsGrounded)
//        //    {
//        //        jumping = false;
//        //    }
//        //    if (movex == 0f)
//        //    {
//        //        // Smoothly decelerate to 0
//        //        rb.linearVelocity = new Vector2(
//        //            Mathf.MoveTowards(rb.linearVelocity.x, 0, 60f * Time.fixedDeltaTime),  // ground decel
//        //            rb.linearVelocity.y);
//        //    }
//        //    else
//        //    {
//        //        //Smoothly accelerate to target speed
//        //        rb.linearVelocity = new Vector2(
//        //            Mathf.MoveTowards(rb.linearVelocity.x, targetX, 40f * Time.fixedDeltaTime),  // accel
//        //            rb.linearVelocity.y);
//        //    }
//        //}


//        CheckGrounded();
//        //if (IsGrounded)
//        //{
//        //    float rayDistance = 2f;
//        //    Vector2 origin = transform.position;

//        //    Debug.DrawRay(origin, Vector2.down * rayDistance, Color.green);

//        //    RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, rayDistance, groundLayer);

//        //    if (hit.collider != null)
//        //    {
//        //        float angle = Vector2.Angle(hit.normal, Vector2.up);
//        //        Debug.Log("Hit: " + hit.collider.name + " angle: " + angle);
//        //        onSlope = angle > 0; // Consider it a slope if the angle is greater than 0
//        //    }
//        //}
//    }

//    void CheckGrounded()
//    {
//        // Start a bit below the center (near feet)
//        Vector2 origin = groundCheckEmpty.position;

//        // 1) Circle downwards to detect ground
//        //RaycastHit2D hit = Physics2D.CircleCast(
//        //    origin,
//        //    groundCheckRadius,
//        //    Vector2.down,
//        //    groundCheckDistance,
//        //    groundLayer
//        //);

//        RaycastHit2D hit = Physics2D.Raycast(
//            origin,
//            Vector2.down,
//            groundCheckDistance,
//            groundLayer
//        );

//        // Debug visualize
//        Debug.DrawRay(origin, Vector2.down * (groundCheckRadius + groundCheckDistance), Color.green);

//        if (hit.collider != null)
//        {
//            IsGrounded = true;
//            //Debug.Log($"Grounded on {hit.collider.name}");
//            // Slope info
//            slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
//            onSlope = slopeAngle > 1f; // small tolerance so flat ground isn�t �slope�
//                                       // Debug.Log($"Hit {hit.collider.name}, angle {slopeAngle}");
//                                       //if (onSlope && !jumping)
//                                       //{
//            //Vector2 temp = tf.position;
//            //temp.y = hit.point.y + 2f; // Adjust 0.5f based on your character's pivot/height
//            //tf.position = temp;
//            //}
//        }
//        else
//        {
//            IsGrounded = false;
//            onSlope = false;
//            slopeAngle = 0f;
//        }
//    }
//    void OnDrawGizmos()
//    {
//        // Draw circle in Scene view (only works in OnDrawGizmos)
//        Gizmos.color = Color.green;
//        Gizmos.DrawWireSphere(groundCheckEmpty.position, groundCheckRadius);
//        Gizmos.DrawCube(groundCheckEmpty.position + Vector3.down * groundCheckDistance, new Vector3(2f, 0.2f, 0)); // visualize boxcast area
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
//            Debug.Log("Jumping");
//            jumping = true;
//            rb.AddForce(new Vector2(0, 10), ForceMode2D.Impulse);
//        }
//    }

//    void OnSlide()
//    {
//        if (onSlope)
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
