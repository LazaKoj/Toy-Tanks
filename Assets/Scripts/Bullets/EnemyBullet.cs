using UnityEngine;

public class EnemyBullet : Bullet
{
    public EnemyBullet() : base()
    {
        type = 0;
    }

    public ParticleSystem bulletTrail;
    public GameObject impactParticle;
    public Vector3 impactNormal;
    public void ProjectileCollision()
    {

        if (impactParticle != null)
        {
            impactParticle = Instantiate(impactParticle, transform.position, Quaternion.FromToRotation(Vector3.up, impactNormal)) as GameObject;
            Destroy(impactParticle, 3);
        }
    }

    public void DetachParticle()
    {
        bulletTrail.transform.parent = null;
        Destroy(bulletTrail.gameObject, 3);
    }

    private void OnTriggerEnter(Collider other)
    {
        GameObject victim = other.gameObject;
        if (victim.GetComponentInParent<PlayerTank>() != null)
            victim.GetComponentInParent<PlayerTank>().TakeDamage();

        DetachParticle();
        Destroy(gameObject);
        ProjectileCollision();
    }
}
