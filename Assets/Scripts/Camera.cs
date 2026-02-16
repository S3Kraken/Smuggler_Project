using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class Camera : MonoBehaviour
{
    GameObject player;
    float xOffset = 5;
    float yOffset = -5;
    Vector3 lastPlayerPos;
    Vector3 currentVel;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GameObject.Find("Player");
        lastPlayerPos = player.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector3(transform.position.x, transform.position.y, -10);
        if (player.transform.position.x > transform.position.x + xOffset || player.transform.position.x < transform.position.x - xOffset)
        {
            //parent the camera to the player
            float targetX = player.transform.position.x - lastPlayerPos.x;
            Vector2 targetPos = new Vector2(targetX, transform.position.y);
            //transform.position = new Vector3(transform.position.x + targetX, transform.position.y, transform.position.z);
            //transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref currentVel, 2);
            //lerp to the player
            //transform.position = Vector3.Lerp(transform.position, player.transform.position, Time.deltaTime * 1);
            transform.position = Vector3.Slerp(transform.position, player.transform.position, Time.deltaTime * 1);

        }

        if (player.transform.position.y > transform.position.y + yOffset || player.transform.position.y < transform.position.y - yOffset)
        {
            float targetY = player.transform.position.y - lastPlayerPos.y;
            transform.position = new Vector3(transform.position.x, transform.position.y + targetY, transform.position.z);
        }

        //if (player.transform.position.x > transform.position.x + xOffset)
        //{
        //    transform.position = new Vector3(player.transform.position.x, transform.position.y, transform.position.z);
        //}
        //else if (player.transform.position.x < transform.position.x - xOffset)
        //{
        //    transform.position = new Vector3(player.transform.position.x, transform.position.y, transform.position.z);
        //}

        //if (player.transform.position.y > transform.position.y + yOffset)
        //{
        //    transform.position = new Vector2(transform.position.x, player.transform.position.y);
        //}
        //else if (player.transform.position.y < transform.position.y - yOffset)
        //{
        //    transform.position = new Vector2(transform.position.x, player.transform.position.y);
        //}

        lastPlayerPos = player.transform.position;
    }
}
