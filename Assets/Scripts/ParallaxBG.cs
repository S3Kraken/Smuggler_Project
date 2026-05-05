using UnityEngine;

public class ParallaxBG : MonoBehaviour
{
    float initalXPos = 7.99f;
    float finalXPos = 270.125f;
    float playerIntialXPos = -12;
    float playerFinalXPos = 287.679f;
    Transform playerTransform;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //find the player
        playerTransform = GameObject.Find("Player").GetComponent<Transform>();
    }

    // Update is called once per frame
    void Update()
    {
        if (playerTransform == null) return;

        // clamp player range to avoid division by zero
        float playerRange = playerFinalXPos - playerIntialXPos;
        if (Mathf.Approximately(playerRange, 0f)) return;

        // normalized progress [0..1]
        float t = Mathf.InverseLerp(playerIntialXPos, playerFinalXPos, playerTransform.position.x);

        // compute target X and set background position
        float targetX = Mathf.Lerp(initalXPos, finalXPos, t);
        Vector3 pos = transform.position;
        pos.x = targetX;
        transform.position = pos;
    }
}
