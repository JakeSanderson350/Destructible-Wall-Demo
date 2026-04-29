using EzySlice;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.LightTransport;

public static class WallFracturer
{
    private static Material meshMaterial;
    private static Material capMaterial;

    public static FractureResult[] Fracture(Mesh wallMesh, ImpactData impact, int numChunks, Transform wallTransform, Material material, Material capMat)
    {
        meshMaterial = material;
        capMaterial = capMat;

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
        List<GameObject> activePieces = new List<GameObject>();

        // Create the initial piece
        GameObject initialObj = new GameObject("InitialPiece");
        initialObj.transform.position = wallTransform.position;
        initialObj.transform.rotation = wallTransform.rotation;
        initialObj.transform.localScale = wallTransform.localScale;

        var initialMF = initialObj.AddComponent<MeshFilter>();
        initialMF.mesh = sourceMesh;
        initialObj.AddComponent<MeshRenderer>();

        activePieces.Add(initialObj);

        // For each seed, create a cutting plane and slice ALL active pieces
        for (int i = 0; i < seeds.Length; i++)
        {
            Vector3 seed = seeds[i];

            // Define the cutting plane
            Vector3 direction = (seed - impact.localPos).normalized;
            Vector3 cutPoint = Vector3.Lerp(impact.localPos, seed, 0.5f);

            // Transform to world space
            Vector3 worldCutPoint = wallTransform.TransformPoint(cutPoint);
            Vector3 worldNormal = wallTransform.TransformDirection(direction);

            List<GameObject> newActivePieces = new List<GameObject>();

            // Slice each active piece with this plane
            foreach (GameObject piece in activePieces)
            {
                SlicedHull hull = piece.Slice(worldCutPoint, worldNormal, meshMaterial);

                if (hull != null)
                {
                    GameObject upperHull = hull.CreateUpperHull(piece, capMaterial);
                    GameObject lowerHull = hull.CreateLowerHull(piece, capMaterial);

                    if (upperHull != null && upperHull.GetComponent<MeshFilter>().mesh.vertexCount > 0)
                        newActivePieces.Add(upperHull);

                    if (lowerHull != null && lowerHull.GetComponent<MeshFilter>().mesh.vertexCount > 0)
                        newActivePieces.Add(lowerHull);

                    Object.Destroy(piece);
                }
                else
                {
                    // The plane didn't intersect this piece - keep it as-is
                    newActivePieces.Add(piece);
                }
            }

            // Replace active pieces with the newly sliced set
            activePieces = newActivePieces;
        }

        // Convert all remaining active pieces to FractureResults
        var results = new List<FractureResult>();

        foreach (GameObject piece in activePieces)
        {
            var pieceMF = piece.GetComponent<MeshFilter>();
            if (pieceMF != null && pieceMF.mesh != null && pieceMF.mesh.vertexCount > 0)
            {
                Mesh chunkMesh = Object.Instantiate(pieceMF.mesh);
                Vector3 localCenter = chunkMesh.bounds.center;

                results.Add(new FractureResult
                {
                    mesh = chunkMesh,
                    localCenter = localCenter
                });
            }

            // Clean up the temporary piece
            Object.Destroy(piece);
        }

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
    public Vector3 localCenter;
}