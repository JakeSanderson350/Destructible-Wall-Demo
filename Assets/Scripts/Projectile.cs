using System.Collections;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public Rigidbody rb;
    private float impactForce;
    private float impactRadius;

    public void Fire(Vector3 direction, float force, float impactForce, float impactRadius)
    {
        this.impactForce = impactForce;
        this.impactRadius = impactRadius;

        rb.AddForce(direction * force);

        StartCoroutine(DelayedDestroy());
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.gameObject.TryGetComponent(out DestructibleWall wall) && collision.impulse.magnitude > 2.0f)
        {
            ImpactData impact = new ImpactData
            {
                worldPos = collision.GetContact(0).point,
                localPos = wall.transform.InverseTransformPoint(collision.GetContact(0).point),
                force = impactForce,
                radius = impactRadius
            };

            wall.RecieveImpact(impact);
        }
    }

    private IEnumerator DelayedDestroy()
    {
        yield return new WaitForSeconds(5.0f);

        Destroy(this.gameObject);
    }
}
