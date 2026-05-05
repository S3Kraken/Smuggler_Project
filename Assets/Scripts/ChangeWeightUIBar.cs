using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class ChangeWeightUIBar : MonoBehaviour
{
    PlayerMovementWithDash playerData;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerData = FindFirstObjectByType<PlayerMovementWithDash>();
    }

    // Update is called once per frame
    void Update()
    {
        float newScale = Mathf.Clamp01(math.remap(playerData.changeWeightCooldown,0,0,1, playerData.changeWeightTimer));
        transform.localScale = new Vector3(newScale, 1, 1);

        if (newScale >= 1)
        {
            this.GetComponent<Image>().color = Color.red;
        }
        else
        {
            this.GetComponent<Image>().color = Color.white;
        }
    }
}
