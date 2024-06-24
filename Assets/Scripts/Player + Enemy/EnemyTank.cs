using System;
using System.Collections;
using UnityEngine;

public class EnemyTank : MonoBehaviour
{
    int health;

    [SerializeField]
    private Animator animator;

    Transform bodyTransform; //position of tank body
    Transform turretTransform; //position of tank turret
    Transform target; //target to pursue, will interrupt patrol if spotted (not implemented.)
                      //defaults to player transform

    GameObject bulletPrefab;
    GameObject player; //constantly check if player is in line of sight (currently not implemented)
    GameObject lastKnownPosition;

    public GameObject originalEnemyTank;
    public GameObject fracturedEnemyTank;

    public GameObject[] patrolRoute; //array of empty game objects the tank will travel along until player is spotted
    public AudioSource tankSound;

    Rigidbody rb;

    Vector3 currentBodyFacing; //current facing of the tank body
    Vector3 currentTurretFacing;
    Vector3 currentWaypoint; //current patrol point the tank is pursuing
    Vector3[] path;

    bool hasFired;
    bool needsNewPatrolPoint; //when a patrol point is reached, this bool is used to request a path to the next point
    bool canSeePlayer; //can draw a ray to the player
    bool isAlerted; //is aware of the player
    bool isInPursuit; //has lost sight of the player and is chasing after them

    int targetIndex;
    int patrolIndex;

    LayerMask mask; //used so the tank can track the obstacle layer

    Coroutine coroutine; //variable used so coroutines can be canceled directly

    [SerializeField]
    private float speed; //velocity of the tank's movement
    [SerializeField]
    private float rotateBodySpeed; //speed at which the tank's body can rotate
    [SerializeField]
    private float rotateTurretSpeed; //speed at which the tank's turret can rotate
    [SerializeField]
    private float rateOfFire;
    [SerializeField]
    private float patrolPause; //time the tank pauses at each patrol point before continuing
    [SerializeField]
    private float viewingAngle;

    Vector3 currentPosition; //tank's position on this frame
    Vector3 previousPosition; //tank's position on last frame

    void Start()
    {
        // Initialization.
        health = 3;
        bodyTransform = gameObject.transform.GetChild(0);
        turretTransform = gameObject.transform.GetChild(1);
        bulletPrefab = Resources.Load("Prefabs/Enemy/EnemyBullet") as GameObject;
        rb = GetComponent<Rigidbody>();
        tankSound = GetComponent<AudioSource>();
        player = GameObject.FindGameObjectWithTag("Player");
        mask = LayerMask.GetMask("Obstacle");
        target = player.transform;
        viewingAngle = viewingAngle / 2;
        lastKnownPosition = new GameObject();
        
        isAlerted = false; 
        isInPursuit = false;
        patrolIndex = 0;

        PathManager.RequestPath(new PathRequest(transform.position, patrolRoute[patrolIndex].transform.position, OnPathFound));
    }

    void Update()
    {
        if (!UIController.paused && !GameState.gameOver)
        {
            // Controls for shooting a bullet from the tank turret.
            CalculateFOV(); //will be used to detect the player
            CalculateFacing(); //each frame, determine which direction the tank is facing

            Debug.Log("Is Alerted: " + isAlerted);
            Debug.Log("Is In Pursuit: " + isInPursuit);

            if (needsNewPatrolPoint)
                coroutine = StartCoroutine("NewPatrolPoint");
        }

        if (previousPosition == null)
            previousPosition = transform.position;
        currentPosition = transform.position;

        if (!CompareVector3s(currentPosition, previousPosition))
        {
            if (!tankSound.isPlaying)
                tankSound.Play();
        }
        else
            tankSound.Stop();

        previousPosition = currentPosition;
    }

    // Rotates the body of the tank n degrees.
    void RotateBody(float n)
    {
        bodyTransform.Rotate(0, n, 0);
    }

    // Moves the tank forward relative to the direction the body is facing.
    void MoveForward()
    {
        transform.Translate(bodyTransform.forward * Time.deltaTime * speed);
    }

    // Moves the tank backward relative to the direction the body is facing.
    void MoveBackward()
    {
        transform.Translate(-bodyTransform.forward * Time.deltaTime * speed);
    }

    // Rotates the turret of the tank n degrees.
    void RotateTurret(float n)
    {
        turretTransform.Rotate(0, n, 0);
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

    void CalculateFOV()
    {
        if (player != null)
        {
            Vector3 direction; //vector between enemy and player
            bool hit; //used to raycast the player and see if line of sight exists
            float angleTo; //inverse cosine of vectors to calculate angle

            direction = player.transform.position - transform.position;
            angleTo = Mathf.Acos(Vector3.Dot(direction.normalized, currentTurretFacing.normalized)) * Mathf.Rad2Deg;

            //raycast through obstacle layer to see if tank can see player
            hit = Physics.Raycast(transform.position, direction, direction.magnitude, mask);

            if (!hit && angleTo < viewingAngle)
            {
                Debug.DrawRay(transform.position, direction, Color.green);

                if (coroutine != null)
                {
                    StopCoroutine(coroutine);
                    coroutine = null;
                }

                currentTurretFacing = turretTransform.forward;
                canSeePlayer = true;
                isAlerted = true;
                isInPursuit = false;

                lastKnownPosition.transform.position = new Vector3(player.transform.position.x, 0, player.transform.position.z);


                Vector3 newFacing = Vector3.RotateTowards(currentTurretFacing, direction.normalized, rotateTurretSpeed * Time.fixedDeltaTime, 0f);
                turretTransform.rotation = Quaternion.LookRotation(newFacing);

                if (CompareVector3s(currentTurretFacing.normalized, newFacing.normalized) && !hasFired)
                    StartCoroutine("Shoot");
            }
            else
            {
                Debug.DrawRay(transform.position, direction, Color.red);
                currentTurretFacing = turretTransform.forward;

                if (coroutine == null && !isAlerted)
                    PathManager.RequestPath(new PathRequest(transform.position, patrolRoute[patrolIndex].transform.position, OnPathFound));

                if (isAlerted && !isInPursuit)
                {
                    isInPursuit = true;

                    if (coroutine != null)
                        StopCoroutine(coroutine);

                    PathManager.RequestPath(new PathRequest(transform.position, lastKnownPosition.transform.position, OnPathFound));
                    coroutine = StartCoroutine("FollowPath");
                }

                Vector3 newFacing = Vector3.RotateTowards(currentTurretFacing, bodyTransform.forward, rotateTurretSpeed * Time.fixedDeltaTime, 0f);
                turretTransform.rotation = Quaternion.LookRotation(newFacing);

                canSeePlayer = false;
            }
        }
    }

    void CalculateFacing()
    {
        currentTurretFacing = turretTransform.forward;
        currentBodyFacing = bodyTransform.forward;
        Debug.DrawRay(transform.position, currentBodyFacing, Color.magenta);
    }

    public void OnPathFound(Vector3[] newPath, bool pathSuccessful)
    {
        if (pathSuccessful)
        {
            if (coroutine != null)
                StopCoroutine(coroutine); //stop existing pathfinding if coroutine already running

            path = newPath; //set path equal to the path found by pathfinding
            targetIndex = 0; //start at start of path
            coroutine = StartCoroutine("FollowPath");
        } 
    }

    IEnumerator FollowPath() //coroutine for following a path
    {
        Vector3 direction;
        currentWaypoint = path[0]; //start on first element

        if (path.Length < 1) //if path is nonexistent, generate a new one
        {
            if (!isAlerted)
            {
                if (patrolIndex < patrolRoute.Length - 1)
                    patrolIndex++;
                else
                {
                    Array.Reverse(patrolRoute);
                    patrolIndex = 1;
                }
            }

            isAlerted = false;
            isInPursuit = false;
            needsNewPatrolPoint = true;

            yield break;
        }

        while (true)
        {
            while (UIController.paused)
                yield return null;

            if (transform.position == currentWaypoint) //if enemy has arrived at position, get a new target
            {
                targetIndex++;

                if (targetIndex >= path.Length) //if reached or exceeded patrol points in path, request a new path
                {
                    rb.velocity = Vector3.zero;
                    isInPursuit = false;
                    isAlerted = false;

                    if (!isAlerted)
                    {
                        if (patrolIndex < patrolRoute.Length - 1) //if there are elements left in patrol path, keep going
                            patrolIndex++;
                        else //else reverse the patrol path and trace a path to the second element (enemy is already at first)
                        {
                            Array.Reverse(patrolRoute); 
                            patrolIndex = 1;
                        }
                    }

                    needsNewPatrolPoint = true;
                    yield break;
                }

                currentWaypoint = path[targetIndex]; //otherwise proceed to next element
            }

            direction = currentWaypoint - bodyTransform.position; //vector between enemy and next point

            if (CompareVector3s(currentBodyFacing, direction.normalized)) //if facing towards next waypoint, move that way
                transform.position = Vector3.MoveTowards(transform.position, currentWaypoint, speed * Time.fixedDeltaTime);
            else //if not facing towards next waypoint, rotate that direction
            {
                Vector3 newDirection = Vector3.RotateTowards(currentBodyFacing, direction.normalized, rotateBodySpeed * Time.fixedDeltaTime, 0f);
                transform.rotation = Quaternion.LookRotation(newDirection);
            }

            yield return null;
        }
    }

    IEnumerator NewPatrolPoint() //when a patrol point is reached, request a new one
    {
        needsNewPatrolPoint = false;

        yield return new WaitForSeconds(patrolPause); //wait patrolPause seconds before proceeding to next point

        PathManager.RequestPath(new PathRequest(transform.position, patrolRoute[patrolIndex].transform.position, OnPathFound));

        if (coroutine == null)
            coroutine = StartCoroutine("FollowPath");
    }

    public void OnDrawGizmos() { //only visible in scene mode, debug for visualizing patrol routes
		if (path != null)
			for (int i = targetIndex; i < path.Length; i ++)
            {
				Gizmos.color = Color.black;
				Gizmos.DrawCube(path[i], Vector3.one);

				if (i == targetIndex)
					Gizmos.DrawLine(transform.position, path[i]);
				else
					Gizmos.DrawLine(path[i-1],path[i]);
			}
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

    
    public void TakeDamage()
    {
        if (--health == 0)
        {
            Vector3 deathPos = this.gameObject.transform.position;
            Destroy(this.gameObject);
            GameObject fractEnemyTank = Instantiate(fracturedEnemyTank, deathPos, Quaternion.identity) as GameObject;
            fractEnemyTank.GetComponent<VisualFX>().Explode();

        }
    }
}
