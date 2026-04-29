using UnityEngine;

public class ProjectileLauncher : MonoBehaviour
{
    [Header("Projectile Settings")]
    public float impactForce = 100.0f;
    public float impactRadius = 1.0f;
    public float projectileVelocity = 1000.0f;
    public GameObject projectile;

    private Camera cam;

    public void Awake()
    {
        cam = Camera.main;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
            TryFireAtWall();
    }

    private void TryFireAtWall()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 100.0f))
        {
            Projectile newProjectile = Instantiate(projectile, transform.position, transform.rotation).GetComponent<Projectile>();

            Vector3 direction = hit.point - transform.position;

            newProjectile.Fire(direction, projectileVelocity, impactForce, impactRadius);
        }
    }
}

public struct ImpactData
{
    public Vector3 worldPos;
    public Vector3 localPos;
    public float force;
    public float radius;
}
