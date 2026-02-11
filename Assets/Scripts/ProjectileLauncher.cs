using System.Runtime.CompilerServices;
using UnityEngine;

public class ProjectileLauncher : MonoBehaviour
{
    [Header("Projectile Settings")]
    public float impactForce = 100.0f;
    public float impactRadius = 1.0f;

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
            DestructibleWall wall = hit.collider.GetComponent<DestructibleWall>();
            if (wall == null)
                return;

            ImpactData impact = new ImpactData
            {
                worldPos = hit.point,
                localPos = wall.transform.InverseTransformPoint(hit.point),
                force = impactForce,
                radius = impactRadius
            };

            wall.RecieveImpact(impact);
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
