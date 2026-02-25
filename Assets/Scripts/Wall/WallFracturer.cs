using EzySlice;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.LightTransport;

public static class WallFracturer
{
    private static Material meshMaterial;

    public static FractureResult[] Fracture(Mesh wallMesh, ImpactData impact, int numChunks, Transform wallTransform, Material material)
    {
        meshMaterial = material;

        Vector3[] seeds = GenerateSeeds(impact.localPos, numChunks, wallMesh.bounds);

        // temp debug showing seeds
        foreach (var seed in seeds)
        {
            Vector3 seedWorldPos = wallTransform.TransformPoint(seed);

            Debug.DrawLine(impact.worldPos, seedWorldPos, Color.blue, 5.0f);
        }

        FractureResult[] chunks = SliceMeshBySeeds(wallMesh, seeds, impact, wallTransform);

        return chunks;
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

    private static FractureResult[] SliceMeshBySeeds(Mesh sourceMesh, Vector3[] seeds, ImpactData impact, Transform wallTransform)
    {
        var results = new List<FractureResult>();

        // Create a temporary object to be cut over and over again
        GameObject sourceObj = new GameObject();
        sourceObj.transform.position = wallTransform.position;
        sourceObj.transform.rotation = wallTransform.rotation;
        sourceObj.transform.localScale = wallTransform.localScale;

        var sourceMF = sourceObj.AddComponent<MeshFilter>();
        sourceMF.mesh = sourceMesh;
        sourceObj.AddComponent<MeshRenderer>();

        // For each seed, make ONE cut from the impact point to that seed
        for (int i = 0; i < seeds.Length; i++)
        {
            Vector3 seed = seeds[i];

            // Plane Normal
            Vector3 direction = (seed - impact.localPos).normalized;

            // Place the plane halfway between seed and impact point
            Vector3 cutPoint = Vector3.Lerp(impact.localPos, seed, 0.5f);

            // Transform to world space for ezy slice
            Vector3 worldCutPoint = wallTransform.TransformPoint(cutPoint);
            Vector3 worldNormal = wallTransform.TransformDirection(direction);

            // Slice the mesh
            GameObject[] slicedObjs = SliceWithPlane(sourceObj, worldCutPoint, worldNormal);

            if (slicedObjs != null)
            {
                // Take one pice of the cut
                GameObject chunkObj = slicedObjs[0];

                if (chunkObj != null)
                {
                    var chunkMF = chunkObj.GetComponent<MeshFilter>();
                    if (chunkMF != null && chunkMF.mesh != null && chunkMF.mesh.vertexCount > 0)
                    {
                        // Copy the mesh so we can destroy the tmp obj
                        Mesh chunkMesh = Object.Instantiate(chunkMF.mesh);
                        Vector3 worldCenter = wallTransform.TransformPoint(chunkMesh.bounds.center);

                        results.Add(new FractureResult
                        {
                            mesh = chunkMesh,
                            center = worldCenter
                        });
                    }

                    Object.Destroy(chunkObj);
                    Object.Destroy(slicedObjs[1]);
                }
            }
        }

        // Clean up
        Object.Destroy(sourceObj);

        return results.ToArray();
    }

    private static GameObject[] SliceWithPlane(GameObject target, Vector3 planePoint, Vector3 planeNormal)
    {
        SlicedHull hull = target.Slice(planePoint, planeNormal, meshMaterial);

        return new GameObject[] {
            hull.CreateUpperHull(target, meshMaterial),
            hull.CreateLowerHull(target, meshMaterial)
        };
    }
}

public struct FractureResult
{
    public Mesh mesh;
    public Vector3 center;
}