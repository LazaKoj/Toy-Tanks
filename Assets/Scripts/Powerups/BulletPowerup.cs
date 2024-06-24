using System.Collections;
using UnityEngine;

public class BulletPowerup : MonoBehaviour
{
    private PlayerTank playerTank;
    private GameObject player;
    public GameObject pickupEffect;
    public Vector3 impactNormal;

    private bool pickedUp;
    private bool powerupActive;

    public float rateGain;
    public float duration;

    private void Awake()
    {
        player = GameObject.Find("Player_Tank");
        playerTank = player.GetComponent<PlayerTank>();
        rateGain = 0.25f;
        duration = 3;
    }
    void Update()
    {
        transform.Rotate(new Vector3(0, 80, 0) * Time.deltaTime);
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

        pickupEffect = Instantiate(pickupEffect, transform.position, Quaternion.FromToRotation(Vector3.up, impactNormal));// as GameObject;

        playerTank.rateOfFire -= rateGain;
        gameObject.GetComponent<MeshRenderer>().enabled = false;
        gameObject.GetComponent<Collider>().enabled = false;
        yield return new WaitForSeconds(duration);
        playerTank.rateOfFire += rateGain;
        powerupActive = false;

        Destroy(gameObject);

        Debug.Log("Power up ended");
    }
}
