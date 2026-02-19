using UnityEngine;
using UnityEngine.InputSystem;

public class PanCamScript : MonoBehaviour
{
    float movex, movey;
    Vector2 smoothVelocity = Vector2.zero;
    float speed = 30;
    public GameObject mainCam;
    GameObject player;

    public InputActionAsset inputActions;
    private InputAction moveAction;

    void Start()
    {

        moveAction = inputActions.FindAction("Move");  // "Move" = your action name
        player = GameObject.Find("Player");
        moveAction.Enable();
    }

    private void FixedUpdate()
    {
        //if (moveAction.WasPressedThisFrame()) Debug.Log("Move pressed!");
        //if (moveAction.WasReleasedThisFrame()) Debug.Log("Move released!");
        if (moveAction.IsPressed())
        {
            Vector2 moveInput = moveAction.ReadValue<Vector2>();
            movex = moveInput.x;
            movey = moveInput.y;
        }
        else
        {
            movex = 0f;
            movey = 0f;
        }

        //float movex = Input.GetAxisRaw("Horizontal");
        //float movey = Input.GetAxisRaw("Vertical");

        float targetX = movex * speed;
        float targetY = movey * speed;

        if (movex == 0f)
        {
            // Smoothly decelerate
            smoothVelocity.x = Mathf.MoveTowards(smoothVelocity.x, 0f, 80f * Time.fixedDeltaTime);
        }
        else
        {
            smoothVelocity.x = Mathf.MoveTowards(smoothVelocity.x, targetX, 60f * Time.fixedDeltaTime);
        }
        if (movey == 0f)
        {
            smoothVelocity.y = Mathf.MoveTowards(smoothVelocity.y, 0f, 80f * Time.fixedDeltaTime);
        }
        else
        {
            smoothVelocity.y = Mathf.MoveTowards(smoothVelocity.y, targetY, 60f * Time.fixedDeltaTime);
        }

        transform.Translate(smoothVelocity * Time.fixedDeltaTime, Space.World);

    }

    //private void OnMove(Vector2 v)
    //{
    //    Debug.Log("Moving");
    //    //Vector2 movementVector = movementValue.Get<Vector2>();
    //    movex = v.x;
    //    movey = v.y;
    //}

    public void ResetSpeed()
    {
        smoothVelocity = Vector2.zero;
        this.transform.position = new Vector3(
        player.transform.position.x,
        player.transform.position.y,
        this.transform.position.z);
    }
}