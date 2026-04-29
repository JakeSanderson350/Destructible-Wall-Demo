using UnityEngine;

public class DestructibleWall : MonoBehaviour
{
    [Header("Fracture Settings")]
    public int numChunks = 16;

    [Header("Materials")]
    public Material meshMaterial;
    public Material capMaterial;

    private MeshFilter meshFilter;

    private void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
    }

    public void RecieveImpact(ImpactData impact)
    {
        FractureResult[] chunks = WallFracturer.Fracture(meshFilter.mesh, impact, numChunks, transform, meshMaterial, capMaterial);

        SpawnChunks(chunks, impact);
    }

    private void SpawnChunks(FractureResult[] chunks, ImpactData impact)
    {
        // Create a parent object
        GameObject chunksParent = new GameObject($"{gameObject.name}_Chunks");
        chunksParent.transform.position = transform.position;
        chunksParent.transform.rotation = transform.rotation;

        foreach (var chunk in chunks)
        {
            GameObject chunkObj = new GameObject("Chunk");
            chunkObj.transform.SetParent(chunksParent.transform);
            chunkObj.transform.localPosition = chunk.localCenter;
            chunkObj.transform.rotation = transform.rotation;
            chunkObj.transform.localScale = transform.localScale;

            // Add mesh components
            MeshFilter mf = chunkObj.AddComponent<MeshFilter>();
            mf.mesh = chunk.mesh;

            MeshRenderer mr = chunkObj.AddComponent<MeshRenderer>();
            mr.material = gameObject.GetComponent<MeshRenderer>().material;

            // Add physics components
            MeshCollider mc = chunkObj.AddComponent<MeshCollider>();
            mc.sharedMesh = chunk.mesh;
            mc.convex = true;

            Rigidbody rb = chunkObj.AddComponent<Rigidbody>();
            rb.mass = 15.0f;
        }

        Destroy(gameObject);
    }
}
