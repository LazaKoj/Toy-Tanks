using System.Collections;
using UnityEngine;

public class SpeedPowerup : MonoBehaviour
{
    private PlayerTank playerTank;
    private GameObject player;
    public GameObject pickupEffect;
    public Vector3 impactNormal;

    private bool pickedUp;
    private bool powerupActive;

    public float speedGain;
    public float duration;

    private void Awake()
    {
        player = GameObject.Find("Player_Tank");
        playerTank = player.GetComponent<PlayerTank>();
        speedGain = 3;
        duration = 3;
    }
    void Update()
    {
        transform.Rotate(new Vector3(80, 0, 0) * Time.deltaTime);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!pickedUp && !powerupActive && other.CompareTag("Player"))
        {
            pickedUp = true;
            powerupActive = true;
            StartCoroutine(PowerUpTimer(duration));
        }
    }

    IEnumerator PowerUpTimer(float duration)
    {
        Debug.Log("Power up started");

        pickupEffect = Instantiate(pickupEffect, transform.position, Quaternion.FromToRotation(Vector3.up, impactNormal));

        playerTank.speed += speedGain;
        gameObject.GetComponent<MeshRenderer>().enabled = false;
        gameObject.GetComponent<Collider>().enabled = false;
        yield return new WaitForSeconds(duration);
        playerTank.speed -= speedGain;
        powerupActive = false;

        Destroy(gameObject);

        Debug.Log("Power up ended");
    }
}