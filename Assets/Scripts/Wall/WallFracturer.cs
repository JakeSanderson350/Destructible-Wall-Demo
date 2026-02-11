using UnityEngine;
using UnityEngine.LightTransport;

public static class WallFracturer
{
    public static FractureResult[] Fracture(Mesh wallMesh, ImpactData impact, int numChunks, Transform wallTransform)
    {
        Vector3[] seeds = GenerateSeeds(impact.localPos, numChunks, wallMesh.bounds);

        // temp debug showing seeds
        foreach (var seed in seeds)
        {
            Vector3 seedWorldPos = wallTransform.TransformPoint(seed);

            Debug.DrawLine(impact.worldPos, seedWorldPos, Color.blue, 5.0f);
        }

        return null;
    }

    private static Vector3[] GenerateSeeds(Vector3 impactLocal, int count, Bounds bounds)
    {
        Vector3[] seeds = new Vector3[count];

        for (int i = 0; i < count; i++)
        {
            // Make more seeds closer to impact and fewer farther away
            float t = (float)i / (float)count;
            float radius = Mathf.Lerp(0.05f, 0.5f, Mathf.Pow(t, 0.5f));
            radius *= Mathf.Max(bounds.size.x, bounds.size.y);

            // Get random point
            Vector2 randCircle = Random.insideUnitCircle * radius;
            seeds[i] = new Vector3(
                impactLocal.x + randCircle.x,
                impactLocal.y + randCircle.y,
                impactLocal.z
            );

            // Clamp to bounds
            seeds[i] = new Vector3(
                Mathf.Clamp(seeds[i].x, bounds.min.x, bounds.max.x),
                Mathf.Clamp(seeds[i].y, bounds.min.y, bounds.max.y),
                seeds[i].z
            );
        }

        return seeds;
    }
}

public struct FractureResult
{
    public Mesh mesh;
    public Vector3 center;
}