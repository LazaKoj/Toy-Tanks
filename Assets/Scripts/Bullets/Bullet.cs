using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField]
    private float speed;
    protected int type;

    public Bullet()
    {

    }

    public void Update()
    {
        if (!UIController.paused)
        {
            transform.position += transform.forward * speed;
            if (transform.position.x > 100 || transform.position.x < -100 || transform.position.z > 100 || transform.position.z < -100)
                Destroy(gameObject);
        }
    }
}
