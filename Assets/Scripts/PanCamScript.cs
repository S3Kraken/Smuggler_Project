using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using static Unity.Cinemachine.CinemachineTargetGroup;
using static UnityEngine.GraphicsBuffer;

public class PanCamScript : MonoBehaviour
{
    float movex, movey;
    Vector2 smoothVelocity = Vector2.zero;
    float speed = 5;
    public GameObject mainCam;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //this.enabled = false;
    }

    private void FixedUpdate()
    {
        float targetX = movex * speed;
        float targetY = movey * speed;

        if (movex == 0f)
        {
            // Smoothly decelerate
            smoothVelocity.x = Mathf.MoveTowards(smoothVelocity.x, 0f, 60f * Time.fixedDeltaTime);
        }
        else
        {
            smoothVelocity.x = Mathf.MoveTowards(smoothVelocity.x, targetX, 60f * Time.fixedDeltaTime);
        }
        if (movey == 0f)
        {
            smoothVelocity.y = Mathf.MoveTowards(smoothVelocity.y, 0f, 60f * Time.fixedDeltaTime);
        }
        else
        {
            smoothVelocity.y = Mathf.MoveTowards(smoothVelocity.y, targetY, 60f * Time.fixedDeltaTime);
        }

        transform.Translate(smoothVelocity * Time.fixedDeltaTime, Space.World);

    }

    private void OnMove(InputValue movementValue)
    {
        Debug.Log("Moving");
        Vector2 movementVector = movementValue.Get<Vector2>();
        movex = movementVector.x;
        movey = movementVector.y;
    }
}