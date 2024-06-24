using System.Collections;
using UnityEngine;

public class EnemyStationaryTurret : MonoBehaviour
{
    [SerializeField]
    private Animator animator;

    Transform turretTransform;

    Vector3 currentFacing;
    Vector3 originalFacing;

    LayerMask mask; //used so the tank can track the obstacle layer

    GameObject bulletPrefab;

    public GameObject originalEnemyTurret;
    public GameObject fracturedEnemyTurret;

    GameObject player;

    int health;
    bool hasFired;

    bool canSeePlayer;

    [SerializeField]
    private float rotateTurretSpeed;
    [SerializeField]
    private float rateOfFire;
    [SerializeField]
    private float viewingAngle;

    void Start()
    {
        // Initialization.
        health = 3;
        player = GameObject.FindGameObjectWithTag("Player");
        turretTransform = gameObject.transform.GetChild(1);
        bulletPrefab = Resources.Load("Prefabs/Enemy/EnemyBullet") as GameObject;
        mask = LayerMask.GetMask("Obstacle");
        originalFacing = turretTransform.forward;
        currentFacing = originalFacing;
        viewingAngle = viewingAngle / 2;
    }

    void Update()
    {
        if (!UIController.paused && !GameState.gameOver)
            CalculateFOV();
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
            turretTransform.localPosition + (turretTransform.forward * .6f + turretTransform.right * .15f) + turretTransform.position + new Vector3(0, .25f, 0),
            turretTransform.rotation);
        Instantiate(bulletPrefab,
            turretTransform.localPosition + (turretTransform.forward * .6f - turretTransform.right * .15f) + turretTransform.position + new Vector3(0, .25f, 0),
            turretTransform.rotation);
        yield return new WaitForSeconds(rateOfFire);
        hasFired = false;

        animator.ResetTrigger("Fired");
    }

   
    public void TakeDamage()
    {
        if (--health == 0)
        {
            Vector3 deathPos = this.gameObject.transform.position;
            Destroy(this.gameObject);
            GameObject fractEnemyTurret = Instantiate(fracturedEnemyTurret, deathPos, Quaternion.identity) as GameObject;
            fractEnemyTurret.GetComponent<VisualFX>().Explode();

        }
    }

    void CalculateFOV() 
    {
        if (player != null)
        {
            Vector3 direction; //vector between enemy and player
            bool hit; //used to raycast the player and see if line of sight exists
            float angleTo; //inverse cosine of vectors to calculate angle

            direction = player.transform.position - transform.position;
            angleTo = Mathf.Acos(Vector3.Dot(direction.normalized, currentFacing.normalized)) * Mathf.Rad2Deg;

            //raycast through obstacle layer to see if tank can see player
            hit = Physics.Raycast(transform.position, direction, direction.magnitude, mask);

            if (!hit && angleTo < viewingAngle)
            {
                Debug.DrawRay(transform.position, direction, Color.green);
                CalculateFacing();
                canSeePlayer = true;
                Debug.Log("Can See Player");
            }
            else
            {
                Debug.DrawRay(transform.position, direction, Color.red);
                canSeePlayer = false;
            }

            if (canSeePlayer)
            {
                Vector3 newFacing = Vector3.RotateTowards(currentFacing, direction.normalized, rotateTurretSpeed * Time.fixedDeltaTime, 0f);
                transform.rotation = Quaternion.LookRotation(newFacing);

                if (CompareVector3s(newFacing.normalized, currentFacing.normalized) && !hasFired)
                    StartCoroutine("Shoot");
            }
        }
    }

    void CalculateFacing() 
    {
        currentFacing = turretTransform.forward;
        Debug.DrawRay(transform.position, currentFacing, Color.magenta);
    }

    bool CompareVector3s(Vector3 a, Vector3 b) //compare two Vector3s and account for floating point variance
    {
        float leeway = 0.01f;

        if (Mathf.Abs(a.x - b.x) < leeway)
            if (Mathf.Abs(a.y - b.y) < leeway)
                if (Mathf.Abs(a.z - b.z) < leeway)
                    return true;

        return false;
    }
}
