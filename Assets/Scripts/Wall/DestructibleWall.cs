using UnityEngine;

public class DestructibleWall : MonoBehaviour
{
    [Header("Fracture Settings")]
    public int numChunks = 16;

    private MeshFilter meshFilter;

    private void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
    }

    public void RecieveImpact(ImpactData impact)
    {
        Debug.Log("BAbabooey");
        FractureResult[] chunks = WallFracturer.Fracture(meshFilter.mesh, impact, numChunks, transform);
    }
}
