using System.Collections;
using UnityEngine;

public class PlayerTank : MonoBehaviour
{
    public int health;
    bool invincibile;

    [SerializeField]
    private Animator animator;
    Rigidbody rb;

    Transform bodyTransform;
    Transform turretTransform;

    GameObject bulletPrefab;

    public GameObject originalPlayerTank;
    public GameObject fracturedPlayerTank;

    public AudioSource tankSound;
    
    bool hasFired;

    [SerializeField]
    public float speed;
    [SerializeField]
    private float rotateBodySpeed;
    [SerializeField]
    private float rotateTurretSpeed;
    [SerializeField]
    public float rateOfFire;
    
    void Start()
    {
        // Initialization.
        bodyTransform = gameObject.transform.GetChild(0);
        turretTransform = gameObject.transform.GetChild(1);
        bulletPrefab = Resources.Load("Prefabs/Player/PlayerBullet") as GameObject;
        rb = GetComponent<Rigidbody>();
        tankSound = GetComponent<AudioSource>();

    }

    void Update()
    {
        if (!UIController.paused)
        {
            // Controls for shooting a bullet from the tank turret.
            if (!hasFired && Input.GetKeyDown("space"))
                StartCoroutine(Shoot());
            Debug.DrawRay(transform.position, transform.GetChild(0).forward, Color.cyan);
        }

        if (Input.GetKey("w") || (Input.GetKey("s")) || (Input.GetKey("a")) || (Input.GetKey("d")))
        {
            if (!tankSound.isPlaying)
                tankSound.Play();
        }
        else
            tankSound.Stop();
    }

    void FixedUpdate()
    {
        if (!UIController.paused)
        {
            // Controls for turning tank left and right.
            if (Input.GetKey("a"))
                RotateBody(-rotateBodySpeed);
            else if (Input.GetKey("d"))
                RotateBody(rotateBodySpeed);

            // Controls for moving a tank forward and backward.
            if (Input.GetKey("w"))
                MoveForward();
            else if (Input.GetKey("s"))
                MoveBackward();

            // Controls for turning tank turret left and right.
            if (Input.GetKey("left"))
                RotateTurret(-rotateTurretSpeed);
            else if (Input.GetKey("right"))
                RotateTurret(rotateTurretSpeed);
        }
    }

    // Rotates the body of the tank n degrees.
    void RotateBody(float n)
    {
        bodyTransform.Rotate(0, n, 0);
    }

    // Moves the tank forward relative to the direction the body is facing.
    void MoveForward()
    {
        transform.Translate(bodyTransform.forward * Time.deltaTime * speed, Space.Self);
    }

    // Moves the tank backward relative to the direction the body is facing.
    void MoveBackward()
    {
        transform.Translate(-bodyTransform.forward * Time.deltaTime * speed, Space.Self);
    }

    // Rotates the turret of the tank n degrees.
    void RotateTurret(float n)
    {
        turretTransform.Rotate(0, n, 0, Space.Self);
    }

    IEnumerator Shoot()
    {
        hasFired = true;
        animator.SetTrigger("Fired");
        
        Instantiate(bulletPrefab,
            turretTransform.localPosition + (turretTransform.forward * .6f) + turretTransform.position + new Vector3(0, .25f, 0),
            turretTransform.rotation);
        yield return new WaitForSeconds(rateOfFire);
        hasFired = false;

        animator.ResetTrigger("Fired");
    }

    public void TakeDamage()
    {
        Debug.Log("Damage Taken");
        if (!invincibile)
        {
            health--;
            StartCoroutine(InvincibleBlinking());
        }

        if (health == 0)
        {
            GameObject camera = GetComponentInChildren<Camera>().gameObject;
            Instantiate(camera, camera.transform.position, camera.transform.rotation);
            Vector3 deathPos = this.gameObject.transform.position;
            Destroy(this.gameObject);
            GameObject fractPlayerTank = Instantiate(fracturedPlayerTank, deathPos, Quaternion.identity) as GameObject;
            fractPlayerTank.GetComponent<VisualFX>().Explode();
        }
    }



    IEnumerator InvincibleBlinking()
    {
        Debug.Log("Invincible");
        invincibile = true;

        MeshRenderer[] renderers = GetComponentsInChildren<MeshRenderer>();
        for (int i = 0; i < 5; i++)
        {
            foreach (MeshRenderer r in renderers)
                r.enabled = false;
            yield return new WaitForSeconds(.2f);
            foreach (MeshRenderer r in renderers)
                r.enabled = true;
            yield return new WaitForSeconds(.2f);
        }
        invincibile = false;
    }
}
