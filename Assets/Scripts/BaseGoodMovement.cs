using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
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
    float speed = 5;
    string weight = "med"; // "low", "med", "high"
    float lowWeightSpeed = 12.5f;
    float medWeightSpeed = 10;
    float highWeightSpeed = 5f;
    private float health = 100;

    public ContactFilter2D groundFilter; // Set in Inspector (layer & angle)
    bool IsGrounded;

    TMPro.TextMeshProUGUI onSlopeText;
    public LayerMask groundLayer; // Set this to the layer(s) that represent the ground in your game

    public bool canClimb = false;
    TMPro.TextMeshProUGUI ChangeWeightButtonText;
    TMPro.TextMeshProUGUI healthText;

    bool jumping = false;
    bool sliding = false;
    float deceleration = 80f;
    float slideSpeedTimer = 0f;


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
    bool wasJustSliding = false;
    Vector2 slopeNormal;

    bool killMovement = false;
    CamSwitcher camSwitcher;

    public InputActionAsset inputActions;
    private InputAction slideAction;

    void Start()
    {
        tf = GetComponent<Transform>();
        rb = GetComponent<Rigidbody2D>();
        groundCheckEmpty = GameObject.Find("GroundCheckEmpty").GetComponent<Transform>();
        onSlopeText = GameObject.Find("Slope (TMP)").GetComponent<TMPro.TextMeshProUGUI>();
        healthText = GameObject.Find("Health (TMP)").GetComponent<TMPro.TextMeshProUGUI>();
        ChangeWeightButtonText = GameObject.Find("ChangeWeightButton (TMP)").GetComponent<TMPro.TextMeshProUGUI>();
        ChangeWeightButtonText.text = weight;

        camSwitcher = GetComponent<CamSwitcher>();
        switch (weight)
        {
            case "low": speed = lowWeightSpeed; break;
            case "med": speed = medWeightSpeed; break;
            case "high": speed = highWeightSpeed; break;
        }
        inputActions = GetComponent<PlayerInput>().actions;
        slideAction = inputActions.FindAction("Slide");
        slideAction.Enable();
    }
    private void Update()
    {

        killMovement = camSwitcher.panModeActive;

        sliding = slideAction.IsPressed();

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
            if (slideSpeedTimer >= 0.2f)
            {
                Debug.Log($"Increasing slide speed. Current speed: {speed}");
                speed *= 1.5f;
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

        //if (Input.GetKey(KeyCode.LeftShift))
        //{
        //    sliding = true;
        //}
        //else
        //{
        //    sliding = false;
        //}

        //onSlopeText.text = "On Slope: " + (canClimb && movey != 0);
        //onSlopeText.text = $"Speed: {speed}\nLinear vel: {rb.linearVelocity.x}";
        onSlopeText.text = $"On Slope: {sliding}";
        //report linear velocity and slope status for debugging
    }
    IEnumerator CarrySpeed()
    {
        yield return new WaitForSeconds(1.5f);
        slideSpeedTimer = 0f;
        switch (weight)
        {
            case "low": speed = lowWeightSpeed; break;
            case "med": speed = medWeightSpeed; break;
            case "high": speed = highWeightSpeed; break;
        }
        deceleration = 20f;
        yield return new WaitForSeconds(1.5f);
        deceleration = 80f;

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
        else if (onSlope && movex == 0f && !jumping)
        {
            rb.linearVelocity = new Vector2(Mathf.MoveTowards(rb.linearVelocity.x, 0, 60f * Time.fixedDeltaTime), 0f);
            rb.gravityScale = -0f;
        }
        else if (!jumping)
        {
            Vector3 adjustedGravity = -slopeNormal * Physics.gravity.magnitude * 2;
            rb.AddForce(adjustedGravity, ForceMode2D.Force);

            isDownhill = (slopeNormalPerp.y > 0f && movex > 0f) || (slopeNormalPerp.y < 0f && movex < 0f);
            if (isDownhill && rb.linearVelocityY < 0 && sliding)
            {
                rb.gravityScale = 2f; // Disable gravity while sliding down slope
                slidingDownSlope = true;
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
                rb.gravityScale = 2f;
                slidingDownSlope = false;
                rb.linearVelocity = new Vector2(speed * slopeNormalPerp.x * -movex, speed * slopeNormalPerp.y * -movex);

                //var slideMovement = new Rigidbody2D.SlideMovement
                //{
                //    surfaceSlideAngle = 75f,  // Stick to slopes up to 75°
                //    surfaceAnchor = new Vector2(0, -0.1f)  // Anchor to ground
                //};

                //Vector2 slideVelocity = new Vector2(speed * slopeNormalPerp.x * -movex,
                //                                   speed * slopeNormalPerp.y * -movex);

                //rb.Slide(slideVelocity, Time.fixedDeltaTime, slideMovement);

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

    IEnumerator justJumped()
    {
        slopeCheckDistance = 0.0f; // Reduce slope check distance immediately after jumping to prevent sticking to slopes
        yield return new WaitForSeconds(0.15f);
        slopeCheckDistance = 1f; // Restore slope check distance after a short delay
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
            slopeNormal = hit.normal;
            //Debug.Log($"Grounded on {hit.collider.name}");
            // Slope info
            //Debug.Log($"Hit: {hit.collider.name} at distance {hit.distance}");
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
        if (killMovement) return; // Ignore player movement input when in pan mode
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
        if (killMovement) return; // Ignore jump input when in pan mode
        if (IsGrounded)
        {
            IsGrounded = false;
            jumping = true;
            StartCoroutine(justJumped());
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
        healthText.text = "Health: " + health;
    }

    public void TakeDamage(float amount)
    {
        health -= amount;
        if (health <= 0)
        {
            health = 0;
            SceneManager.LoadScene("DemoStage");

        }
        healthText.text = "Health: " + health;
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

        if (!sliding)
        {
            switch (weight)
            {
                case "low": speed = lowWeightSpeed; break;
                case "med": speed = medWeightSpeed; break;
                case "high": speed = highWeightSpeed; break;
            }
        }
        
    }
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.name == "Ladders")
        { canClimb = true; }
        else if (collision.gameObject.tag == "Bullet")
        {
            Debug.Log("Hit by bullet!");
            TakeDamage(20);
        }
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
