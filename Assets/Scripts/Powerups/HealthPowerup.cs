using UnityEngine;

public class HealthPowerup : MonoBehaviour
{
    private PlayerTank playerTank;
    private GameObject player;
    public GameObject pickupEffect;
    public Vector3 impactNormal;

    private bool pickedUp;
    public int healthGain;

    void Awake()
    {
        player = GameObject.Find("Player_Tank");
        playerTank = player.GetComponent<PlayerTank>();
        healthGain = 1;
    }

    void Update()
    {
        transform.Rotate(new Vector3(0, 80, 0) * Time.deltaTime);
    }

    public void OnTriggerEnter(Collider other)
    {
        if (!pickedUp && other.gameObject.CompareTag("Player"))
        {
            pickedUp = true;
            pickupEffect = Instantiate(pickupEffect, transform.position, Quaternion.FromToRotation(Vector3.up, impactNormal));
            playerTank.health += healthGain;
            Destroy(gameObject);
        }
    }
}
