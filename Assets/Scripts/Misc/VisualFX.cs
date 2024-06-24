using UnityEngine;

public class VisualFX : MonoBehaviour
{
    public float minForce;
    public float maxForce;
    public float radius;
    public float destroyDelay;
    public GameObject explosion;
    public Vector3 explosionOffset;

    public GameObject smokeTrail;
    public int maximumSmoke;

    public void Explode()
    {
        int smokeCounter = 0;

        if(explosion != null)
        {
            GameObject explosionFX = Instantiate(explosion, transform.position + explosionOffset, Quaternion.identity) as GameObject;
            Destroy(explosionFX, 5);
        }
      
        foreach(Transform t in transform)
        {
            var rb = t.GetComponent<Rigidbody>();

            if(rb != null)
                rb.AddExplosionForce(Random.Range(minForce, maxForce), transform.position, radius);

            if (smokeTrail != null && smokeCounter < maximumSmoke && Random.Range(1, 4) == 1)
            {
                GameObject smokeFX = Instantiate(smokeTrail, t.transform) as GameObject;
                smokeCounter++;
                Destroy(smokeFX, 5);
            }
            Destroy(t.gameObject, destroyDelay);
        }

        Destroy(gameObject, 5);
    }
}
