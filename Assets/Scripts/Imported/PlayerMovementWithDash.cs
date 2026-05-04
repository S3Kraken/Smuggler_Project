/*
	Originally created by @DawnosaurDev at youtube.com/c/DawnosaurStudios
	
    Modified by us to fit our game
 */

using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerMovementWithDash : MonoBehaviour
{
    //Scriptable object which holds all the player's movement parameters. If you don't want to use it
    //just paste in all the parameters, though you will need to manuly change all references in this script
    public PlayerDataWithDash currentData;
    public PlayerDataWithDash lightData;
    public PlayerDataWithDash mediumData;
    public PlayerDataWithDash heavyData;

    #region COMPONENTS
    public Rigidbody2D RB { get; private set; }
    public Animator anim { get; private set; }
    public GameObject jumpFX;
    TMPro.TextMeshProUGUI velocityText;

    TMPro.TextMeshProUGUI ChangeWeightButtonText;

    #endregion

    #region STATE PARAMETERS
    //Variables control the various actions the player can perform at any time.
    //These are fields which can are public allowing for other sctipts to read them
    //but can only be privately written to.
    public bool IsFacingRight { get; private set; }
    public bool IsJumping { get; private set; }
    public bool IsGrounded { get; private set; }

    public bool IsWallJumping { get; private set; }
    public bool IsDashing { get; private set; }
    public bool IsWallSliding { get; private set; }

    //Timers (also all fields, could be private and a method returning a bool could be used)
    [SerializeField] public float LastOnGroundTime { get; private set; }
    public float LastOnWallTime { get; private set; }
    public float LastOnWallRightTime { get; private set; }
    public float LastOnWallLeftTime { get; private set; }

    //Jump
    private bool _isJumpCut;
    public bool IsFalling { get; private set; }

    //Wall Jump
    private float _wallJumpStartTime;
    private int _lastWallJumpDir;

    //Dash
    private int _dashesLeft;
    private bool _dashRefilling;
    private Vector2 _lastDashDir;
    private bool _isDashAttacking;

    //Slopes
    string weight = "light"; // "light", "medium", "heavy"
    private bool _onSlope;
    private float _slopeAngle;
    private Vector2 _slopeNormalPerp;
    [SerializeField] float _slopeCheckDistance = 1f;
    public bool OnDownhillSlope { get; private set; }
    public bool IsSlopeSliding { get; private set; }
    public bool WasOnSlope { get; private set; }

    //Animations
    public float LockState { get; set; }

    //Text
    [SerializeField] bool showVelocity;
    [SerializeField] bool startFlipped;

    #endregion

    #region INPUT PARAMETERS
    private Vector2 _moveInput;

    public float LastPressedJumpTime { get; private set; }
    public float LastPressedDashTime { get; private set; }
    #endregion

    #region CHECK PARAMETERS
    //Set all of these up in the inspector
    [Header("Checks")]
    [SerializeField] private Transform _groundCheckPoint;
    //Size of groundCheck depends on the size of your character generally you want them slightly small than width (for ground) and height (for the wall check)
    [SerializeField] private Vector2 _groundCheckSize = new Vector2(0.49f, 0.03f);
    [Space(5)]
    [SerializeField] private Transform _frontWallCheckPoint;
    [SerializeField] private Transform _backWallCheckPoint;
    [SerializeField] private Vector2 _wallCheckSize = new Vector2(0.5f, 1f);
    #endregion

    #region LAYERS & TAGS
    [Header("Layers & Tags")]
    [SerializeField] private LayerMask _groundLayer;
    #endregion

    private void Awake()
    {
        RB = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        ChangeWeightButtonText = GameObject.Find("ChangeWeightButton (TMP)").GetComponent<TMPro.TextMeshProUGUI>();
        ChangeWeightButtonText.text = weight;
        SetWeight();

        velocityText = GameObject.Find("Velocity (TMP)").GetComponent<TMPro.TextMeshProUGUI>();
        velocityText.text = "Velocity: " + RB.linearVelocity;

        if (!showVelocity)
            velocityText.gameObject.SetActive(false);
    }

    private void Start()
    {
        SetGravityScale(currentData.gravityScale);
        IsFacingRight = true;
        if (startFlipped)
            Turn();
    }

    private void Update()
    {
        #region TIMERS
        LastOnGroundTime -= Time.deltaTime;
        LastOnWallTime -= Time.deltaTime;
        LastOnWallRightTime -= Time.deltaTime;
        LastOnWallLeftTime -= Time.deltaTime;

        LastPressedJumpTime -= Time.deltaTime;
        LastPressedDashTime -= Time.deltaTime;
        #endregion

        #region INPUT HANDLER
        _moveInput.x = Input.GetAxisRaw("Horizontal");
        _moveInput.y = Input.GetAxisRaw("Vertical");

        if (_moveInput.x != 0)
            CheckDirectionToFace(_moveInput.x > 0);

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.C) || Input.GetKeyDown(KeyCode.J))
        {
            OnJumpInput();
        }

        if (Input.GetKeyUp(KeyCode.Space) || Input.GetKeyUp(KeyCode.C) || Input.GetKeyUp(KeyCode.J))
        {
            OnJumpUpInput();
        }

        if (Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.K))
        {
            OnDashInput();
        }
        #endregion

        #region COLLISION CHECKS

        GroundAndSlopeDetection();

        if (!IsDashing && !IsJumping)
        {


            //Wider Ground collision check to use with the small raycast
            if (Physics2D.OverlapBox(_groundCheckPoint.position, _groundCheckSize, 0, _groundLayer) && !IsJumping) //checks if set box overlaps with ground
            {
                LastOnGroundTime = currentData.coyoteTime; //if so sets the lastGrounded to coyoteTime
            }

            //Right Wall Check
            if (((Physics2D.OverlapBox(_frontWallCheckPoint.position, _wallCheckSize, 0, _groundLayer) && IsFacingRight)
                    || (Physics2D.OverlapBox(_backWallCheckPoint.position, _wallCheckSize, 0, _groundLayer) && !IsFacingRight)) && !IsWallJumping)
                LastOnWallRightTime = currentData.coyoteTime;

            //Right Wall Check
            if (((Physics2D.OverlapBox(_frontWallCheckPoint.position, _wallCheckSize, 0, _groundLayer) && !IsFacingRight)
                || (Physics2D.OverlapBox(_backWallCheckPoint.position, _wallCheckSize, 0, _groundLayer) && IsFacingRight)) && !IsWallJumping)
                LastOnWallLeftTime = currentData.coyoteTime;

            //Two checks needed for both left and right walls since whenever the play turns the wall checkPoints swap sides
            LastOnWallTime = Mathf.Max(LastOnWallLeftTime, LastOnWallRightTime);
        }
        #endregion

        #region JUMP CHECKS
        //if (IsJumping && RB.linearVelocity.y < 0)
        //{
        //    //falling
        //    IsJumping = false;

        //    if (!IsWallJumping)
        //        _isFalling = true;
        //}

        if (LastOnGroundTime < 0)
        {
            IsGrounded = false;
        }
        else
        {
            IsGrounded = true;
        }

        if (RB.linearVelocity.y < 0 && LastOnGroundTime < 0.1f)
        {
            IsJumping = false;
            if (!IsWallJumping)
                IsFalling = true;
        }

        if (IsWallJumping && Time.time - _wallJumpStartTime > currentData.wallJumpTime)
        {
            IsWallJumping = false;
        }

        if (LastOnGroundTime > 0 && !IsJumping && !IsWallJumping)
        {
            _isJumpCut = false;

            if (!IsJumping)
                IsFalling = false;
        }

        if (!IsDashing)
        {
            //Jump
            if (CanJump() && LastPressedJumpTime > 0)
            {
                IsJumping = true;
                IsWallJumping = false;
                _isJumpCut = false;
                IsFalling = false;

                Jump();
            }
            //WALL JUMP
            else if (CanWallJump() && LastPressedJumpTime > 0)
            {
                IsWallJumping = true;
                IsJumping = false;
                _isJumpCut = false;
                IsFalling = false;

                _wallJumpStartTime = Time.time;
                _lastWallJumpDir = (LastOnWallRightTime > 0) ? -1 : 1;

                WallJump(_lastWallJumpDir);
            }
        }
        #endregion

        #region DASH CHECKS
        if (CanDash() && LastPressedDashTime > 0)
        {
            //Freeze game for split second. Adds juiciness and a bit of forgiveness over directional input
            //Sleep(Data.dashSleepTime);

            //If not direction pressed, dash forward
            if (_moveInput != Vector2.zero)
                _lastDashDir = _moveInput;
            else
                _lastDashDir = IsFacingRight ? Vector2.right : Vector2.left;



            IsDashing = true;
            IsJumping = false;
            IsWallJumping = false;
            _isJumpCut = false;
            IsSlopeSliding = false;

            StartCoroutine(nameof(StartDash), _lastDashDir);
        }
        #endregion

        #region WALLSLIDE CHECKS
        if (CanWallSlide() && ((LastOnWallLeftTime > 0 && _moveInput.x < 0) || (LastOnWallRightTime > 0 && _moveInput.x > 0)))
            IsWallSliding = true;
        else
            IsWallSliding = false;
        #endregion

        #region GRAVITY
        if (!_isDashAttacking)
        {
            //Higher gravity if we've released the jump input or are falling
            if (IsWallSliding)
            {
                SetGravityScale(0);
            }
            else if (RB.linearVelocity.y < 0 && _moveInput.y < 0 && !IsGrounded)
            {
                //Much higher gravity if holding down
                SetGravityScale(currentData.gravityScale * currentData.fastFallGravityMult);
                //Caps maximum fall speed, so when falling over large distances we don't accelerate to insanely high speeds
                RB.linearVelocity = new Vector2(RB.linearVelocity.x, Mathf.Max(RB.linearVelocity.y, -currentData.maxFastFallSpeed));
            }
            else if (_isJumpCut)
            {
                //Higher gravity if jump button released
                SetGravityScale(currentData.gravityScale * currentData.jumpCutGravityMult);
                RB.linearVelocity = new Vector2(RB.linearVelocity.x, Mathf.Max(RB.linearVelocity.y, -currentData.maxFallSpeed));
            }
            else if ((IsJumping || IsWallJumping || IsFalling) && Mathf.Abs(RB.linearVelocity.y) < currentData.jumpHangTimeThreshold)
            {
                SetGravityScale(currentData.gravityScale * currentData.jumpHangGravityMult);
            }
            else if (RB.linearVelocity.y < 0)
            {
                //Higher gravity if falling
                SetGravityScale(currentData.gravityScale * currentData.fallGravityMult);
                //Caps maximum fall speed, so when falling over large distances we don't accelerate to insanely high speeds
                RB.linearVelocity = new Vector2(RB.linearVelocity.x, Mathf.Max(RB.linearVelocity.y, -currentData.maxFallSpeed));
            }
            else
            {
                //Default gravity if standing on a platform or moving upwards
                SetGravityScale(currentData.gravityScale);
            }
        }
        else
        {
            //No gravity when dashing (returns to normal once initial dashAttack phase over)
            SetGravityScale(0);
        }
        #endregion

        #region ANIMATION CHECKS

        if (LockState >= 0)
        {
            LockState -= Time.deltaTime;
            return;
        }
        else
        {
            if (Mathf.Abs(RB.linearVelocity.x) > 0.1f && !IsJumping && !IsFalling)
            {
                anim.CrossFade("Run", 0, 0);
            }
            else if (IsJumping)
            {
                anim.CrossFade("Jumping", 0, 0);
            }
            else if (RB.linearVelocity.y < 0 && LastOnGroundTime < 0.1f && !IsJumping)
            {
                anim.CrossFade("Falling", 0, 0);
            }
            else if (!IsJumping && !IsFalling)
            {
                anim.CrossFade("Idle", 0, 0);
            }
        }

        #endregion

        #region SET TEXT
        if (showVelocity)
            velocityText.text = "Velocity: " + RB.linearVelocity;
        #endregion
    }

    private void FixedUpdate()
    {
        //Handle Run
        if (!IsDashing)
        {
            if (IsWallJumping)
                Run(currentData.wallJumpRunLerp);
            else
                Run(1);
        }
        else if (_isDashAttacking)
        {
            Run(currentData.dashEndRunLerp);
        }

        //Handle Slide
        if (IsWallSliding)
            WallSlide();
    }

    #region INPUT CALLBACKS
    //Methods which whandle input detected in Update()
    public void OnJumpInput()
    {
        LastPressedJumpTime = currentData.jumpInputBufferTime;
    }

    public void OnJumpUpInput()
    {
        if (CanJumpCut() || CanWallJumpCut())
            _isJumpCut = true;
    }

    public void OnDashInput()
    {
        LastPressedDashTime = currentData.dashInputBufferTime;
    }
    #endregion

    #region GENERAL METHODS
    public void SetGravityScale(float scale)
    {
        RB.gravityScale = scale;
    }

    private void Sleep(float duration)
    {
        //Method used so we don't need to call StartCoroutine everywhere
        //nameof() notation means we don't need to input a string directly.
        //Removes chance of spelling mistakes and will improve error messages if any
        StartCoroutine(nameof(PerformSleep), duration);
    }

    private IEnumerator PerformSleep(float duration)
    {
        Time.timeScale = 0;
        yield return new WaitForSecondsRealtime(duration); //Must be Realtime since timeScale with be 0 
        Time.timeScale = 1;
    }
    #endregion

    //MOVEMENT METHODS
    #region RUN METHODS
    private void Run(float lerpAmount)
    {
        //Calculate the direction we want to move in and our desired velocity
        float targetSpeed = _moveInput.x * currentData.runMaxSpeed;

        //We can reduce are control using Lerp() this smooths changes to are direction and speed
        targetSpeed = Mathf.Lerp(RB.linearVelocity.x, targetSpeed, lerpAmount);

        #region Calculate AccelRate
        float accelRate;

        //Gets an acceleration value based on if we are accelerating (includes turning) 
        //or trying to decelerate (stop). As well as applying a multiplier if we're air borne.
        if (LastOnGroundTime > 0)
            accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? currentData.runAccelAmount : currentData.runDeccelAmount;
        else
            accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? currentData.runAccelAmount * currentData.accelInAir : currentData.runDeccelAmount * currentData.deccelInAir;
        #endregion

        #region Add Bonus Jump Apex Acceleration
        //Increase are acceleration and maxSpeed when at the apex of their jump, makes the jump feel a bit more bouncy, responsive and natural
        if ((IsJumping || IsWallJumping || IsFalling) && Mathf.Abs(RB.linearVelocity.y) < currentData.jumpHangTimeThreshold)
        {
            accelRate *= currentData.jumpHangAccelerationMult;
            targetSpeed *= currentData.jumpHangMaxSpeedMult;
        }
        #endregion

        #region Conserve Momentum
        //We won't slow the player down if they are moving in their desired direction but at a greater speed than their maxSpeed
        if (currentData.doConserveMomentum && Mathf.Abs(RB.linearVelocity.x) > Mathf.Abs(targetSpeed) && Mathf.Sign(RB.linearVelocity.x) == Mathf.Sign(targetSpeed) && Mathf.Abs(targetSpeed) > 0.01f)
        {
            //Prevent any deceleration from happening, or in other words conserve are current momentum
            //You could experiment with allowing for the player to slightly increase their speed whilst in this "state"
            accelRate = 0;
        }
        #endregion


        //Calculate difference between current velocity and desired velocity
        float speedDif = targetSpeed - RB.linearVelocity.x;

        //Calculate force along x-axis to apply to thr player
        float movement = speedDif * accelRate;

        //Convert this to a vector and apply to rigidbody
        //Debug.Log(RB.linearVelocity);
        if (!_onSlope) //If on flat ground, apply a force normally
        {
            //SetGravityScale(Data.gravityScale);
            RB.AddForce(movement * Vector2.right, ForceMode2D.Force);
            IsSlopeSliding = false;
            WasOnSlope = false;
            _slopeCheckDistance = 1;
            //Debug.Log("Not on slope");
        }
        else if (_onSlope && _moveInput.x == 0 && !IsJumping && !IsDashing && !IsSlopeSliding) //Don't slide down if still on a slope
        {
            RB.linearVelocity = new Vector2(0, 0);
            SetGravityScale(0);
            _slopeCheckDistance = 1.5f;
            //Debug.Log("On slope, no input");
        }
        else //Move along slope if on a slope
        {
            SetGravityScale(0);
            _slopeCheckDistance = 1.5f;

            //if (!WasOnSlope)
            //{
            //    //Data.doConserveMomentum = false;
            //    SetGravityScale(0);

            //    WasOnSlope = true;
            //}
            //else
            //{
            //    SetGravityScale(Data.gravityScale);
            //    //SetGravityScale(0);
            //    //Data.doConserveMomentum = true;
            //}


            #region MOVE ALONG SLOPE
            // Ensure slopeNormalPerp is a unit tangent that points the same way as positive move input (right along the slope)
            Vector2 tangent = _slopeNormalPerp.normalized;

            // signed target speed along tangent (preserve input sign)
            float targetSpeedAlongSlope = currentData.runMaxSpeed * -_moveInput.x;

            //Speed up my the slope multiplier if sliding down
            if (IsSlopeSliding)
                targetSpeedAlongSlope = currentData.runMaxSpeed * -currentData.slopeSlideSpeedMultiplier;

            // current velocity projected onto tangent (signed)
            float currentSpeedAlongSlope = Vector2.Dot(RB.linearVelocity, tangent);

            // lerp target like on flat (optional)
            targetSpeedAlongSlope = Mathf.Lerp(currentSpeedAlongSlope, targetSpeedAlongSlope, lerpAmount);

            // acceleration rate same as earlier (accelRate variable already computed)

            // speed difference along slope
            float speedDifAlongSlope = targetSpeedAlongSlope - currentSpeedAlongSlope;


            // movement scalar along tangent
            float movementAlongSlope;
            if (!IsSlopeSliding)
            {
                movementAlongSlope = speedDifAlongSlope * accelRate;
            }
            else
            {
                float slopeAccelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? currentData.slopeSlideAccelAmount : currentData.slopeSlideDeccelAmount;
                movementAlongSlope = speedDifAlongSlope * slopeAccelRate;
            }

            // force vector along slope tangent
            Vector2 forceAlongSlope = tangent * movementAlongSlope;

            // apply force
            RB.AddForce(forceAlongSlope, ForceMode2D.Force);
            #endregion

            //Start slope sliding if your press down
            if (_moveInput.y < 0)
            {
                IsSlopeSliding = true;
            }

            #region OLD SLOPE CODE
            //SetGravityScale(0);

            //Vector2 slopeDir = slopeNormalPerp.normalized;
            //float slopeSpeed = Mathf.Clamp(movement, -Data.runMaxSpeed*2, Data.runMaxSpeed*2);

            //RB.AddForce(-slopeDir * slopeSpeed, ForceMode2D.Force);

            //Vector3 adjustedGravity = -slopeNormal * Physics.gravity.magnitude * 2;
            //RB.AddForce(adjustedGravity, ForceMode2D.Force);

            //RB.AddForce(slopeNormalPerp * movement * -_moveInput.x, ForceMode2D.Force);
            //RB.AddForce(new Vector2(speedDif * slopeNormalPerp.x * -_moveInput.x, speedDif * slopeNormalPerp.y * -_moveInput.x));
            //Debug.Log(_moveInput.x);
            //RB.linearVelocity = new Vector2(Data.runMaxSpeed * slopeNormalPerp.x * -_moveInput.x, Data.runMaxSpeed * slopeNormalPerp.y * -_moveInput.x);
            #endregion
        }
    }

    private void Turn()
    {
        //stores scale and flips the player along the x axis, 
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;

        IsFacingRight = !IsFacingRight;
    }
    #endregion

    #region JUMP METHODS
    private void Jump()
    {
        IsSlopeSliding = false;
        //Reset gravity in case we were on a slope or wall before jumping
        SetGravityScale(currentData.gravityScale);

        //Jump Smoke
        GameObject obj = Instantiate(jumpFX, transform.position, Quaternion.Euler(0, 0, 0));
        PlayerSmokeFXs smoke = obj.GetComponent<PlayerSmokeFXs>();
        smoke.JumpSmoke();

        //Ensures we can't call Jump multiple times from one press
        LastPressedJumpTime = 0;
        LastOnGroundTime = 0;

        #region Perform Jump
        //We increase the force applied if we are falling
        //This means we'll always feel like we jump the same amount 
        //(setting the player's Y velocity to 0 beforehand will likely work the same, but I find this more elegant :D)
        float force = currentData.jumpForce;
        if (RB.linearVelocity.y < 0)
            force -= RB.linearVelocity.y;

        RB.AddForce(Vector2.up * force, ForceMode2D.Impulse);
        #endregion
    }

    private void WallJump(int dir)
    {
        //Ensures we can't call Wall Jump multiple times from one press
        LastPressedJumpTime = 0;
        LastOnGroundTime = 0;
        LastOnWallRightTime = 0;
        LastOnWallLeftTime = 0;

        #region Perform Wall Jump
        Vector2 force = new Vector2(currentData.wallJumpForce.x, currentData.wallJumpForce.y);
        force.x *= dir; //apply force in opposite direction of wall

        if (Mathf.Sign(RB.linearVelocity.x) != Mathf.Sign(force.x))
            force.x -= RB.linearVelocity.x;

        if (RB.linearVelocity.y < 0) //checks whether player is falling, if so we subtract the velocity.y (counteracting force of gravity). This ensures the player always reaches our desired jump force or greater
            force.y -= RB.linearVelocity.y;

        //Unlike in the run we want to use the Impulse mode.
        //The default mode will apply are force instantly ignoring masss
        RB.AddForce(force, ForceMode2D.Impulse);
        #endregion
    }
    #endregion

    #region DASH METHODS
    //Dash Coroutine
    private IEnumerator StartDash(Vector2 dir)
    {
        //Overall this method of dashing aims to mimic Celeste, if you're looking for
        // a more physics-based approach try a method similar to that used in the jump

        LastOnGroundTime = 0;
        LastPressedDashTime = 0;

        float startTime = Time.time;

        _dashesLeft--;
        _isDashAttacking = true;

        SetGravityScale(0);

        //We keep the player's velocity at the dash speed during the "attack" phase (in celeste the first 0.15s)
        while (Time.time - startTime <= currentData.dashAttackTime)
        {
            RB.linearVelocity = dir.normalized * currentData.dashSpeed;
            //Pauses the loop until the next frame, creating something of a Update loop. 
            //This is a cleaner implementation opposed to multiple timers and this coroutine approach is actually what is used in Celeste :D
            yield return null;
        }

        startTime = Time.time;

        _isDashAttacking = false;

        //Begins the "end" of our dash where we return some control to the player but still limit run acceleration (see Update() and Run())
        SetGravityScale(currentData.gravityScale);
        RB.linearVelocity = currentData.dashEndSpeed * dir.normalized;

        while (Time.time - startTime <= currentData.dashEndTime)
        {
            yield return null;
        }

        //Dash over
        IsDashing = false;
    }

    //Short period before the player is able to dash again
    private IEnumerator RefillDash(int amount)
    {
        //SHoet cooldown, so we can't constantly dash along the ground, again this is the implementation in Celeste, feel free to change it up
        _dashRefilling = true;
        yield return new WaitForSeconds(currentData.dashRefillTime);
        _dashRefilling = false;
        _dashesLeft = Mathf.Min(currentData.dashAmount, _dashesLeft + 1);
    }
    #endregion

    #region OTHER MOVEMENT METHODS
    private void WallSlide()
    {
        //Works the same as the Run but only in the y-axis
        //THis seems to work fine, buit maybe you'll find a better way to implement a slide into this system
        float speedDif = currentData.slideSpeed - RB.linearVelocity.y;
        float movement = speedDif * currentData.slideAccel;
        //So, we clamp the movement here to prevent any over corrections (these aren't noticeable in the Run)
        //The force applied can't be greater than the (negative) speedDifference * by how many times a second FixedUpdate() is called. For more info research how force are applied to rigidbodies.
        movement = Mathf.Clamp(movement, -Mathf.Abs(speedDif) * (1 / Time.fixedDeltaTime), Mathf.Abs(speedDif) * (1 / Time.fixedDeltaTime));

        RB.AddForce(movement * Vector2.up);
    }

    public void ChangeWeight()
    {
        if (weight == "light")
        {
            weight = "medium";
        }
        else if (weight == "medium")
        {
            weight = "heavy";
        }
        else
        {
            weight = "light";
        }
        SetWeight();
    }

    private void SetWeight()
    {
        switch (weight)
        {
            case "light": currentData = lightData; break;
            case "medium": currentData = mediumData; break;
            case "heavy": currentData = heavyData; break;
        }
        ChangeWeightButtonText.text = weight;
    }
    #endregion


    #region CHECK METHODS
    void GroundAndSlopeDetection()
    {
        Vector2 origin = _groundCheckPoint.position;
        RaycastHit2D hit;
        if (!IsJumping)
        {
            hit = Physics2D.Raycast(
            origin,
            Vector2.down,
            _slopeCheckDistance,
            _groundLayer
            );
            // Debug visualize
            Debug.DrawRay(origin, Vector2.down * (_slopeCheckDistance), Color.green);
        }
        else
        {
            hit = Physics2D.Raycast(
            origin,
            Vector2.down,
            0,
            _groundLayer
            );
            // Debug visualize
            Debug.DrawRay(origin, Vector2.down * (0), Color.green);
        }






        if (hit.collider != null)
        {
            LastOnGroundTime = currentData.coyoteTime;

            if (!IsGrounded)
            {
                //making land FX
                GameObject obj = Instantiate(jumpFX, transform.position - (Vector3.up * transform.localScale.y / 2), Quaternion.Euler(0, 0, 0));
                PlayerSmokeFXs smoke = obj.GetComponent<PlayerSmokeFXs>();
                smoke.LandSmoke();
            }
            IsGrounded = true;
            _slopeNormalPerp = Vector2.Perpendicular(hit.normal).normalized;
            Vector2 _slopeNormal = hit.normal;
            _slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
            _onSlope = _slopeAngle > 1f;
            OnDownhillSlope = (_slopeNormalPerp.y > 0f && _moveInput.x > 0f) || (_slopeNormalPerp.y < 0f && _moveInput.x < 0f);


            //Debug.Log($"Grounded on {hit.collider.name}");
            // Slope info
            //Debug.Log($"Hit: {hit.collider.name} at distance {hit.distance}");

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
            _onSlope = false;
            _slopeAngle = 0f;
            //_slopeCheckDistance = 1f;
        }
    }
    public void CheckDirectionToFace(bool isMovingRight)
    {
        if (isMovingRight != IsFacingRight && !IsSlopeSliding)
            Turn();
    }

    private bool CanJump()
    {
        return LastOnGroundTime > 0 && !IsJumping;
    }

    private bool CanWallJump()
    {
        return LastPressedJumpTime > 0 && LastOnWallTime > 0 && LastOnGroundTime <= 0 && (!IsWallJumping ||
             (LastOnWallRightTime > 0 && _lastWallJumpDir == 1) || (LastOnWallLeftTime > 0 && _lastWallJumpDir == -1));
    }

    private bool CanJumpCut()
    {
        return IsJumping && RB.linearVelocity.y > 0;
    }

    private bool CanWallJumpCut()
    {
        return IsWallJumping && RB.linearVelocity.y > 0;
    }

    private bool CanDash()
    {
        if (!IsDashing && _dashesLeft < currentData.dashAmount && LastOnGroundTime > 0 && !_dashRefilling)
        {
            StartCoroutine(nameof(RefillDash), 1);
        }

        return _dashesLeft > 0;
    }

    public bool CanWallSlide()
    {
        if (LastOnWallTime > 0 && !IsJumping && !IsWallJumping && !IsDashing && LastOnGroundTime <= 0)
            return true;
        else
            return false;
    }
    #endregion


    #region EDITOR METHODS
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(_groundCheckPoint.position, _groundCheckSize);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(_frontWallCheckPoint.position, _wallCheckSize);
        Gizmos.DrawWireCube(_backWallCheckPoint.position, _wallCheckSize);
    }
    #endregion
}

// created by Dawnosaur :D