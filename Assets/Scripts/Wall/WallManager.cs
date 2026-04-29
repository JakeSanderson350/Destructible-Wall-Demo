using System.Collections.Generic;
using UnityEngine;

public class WallManager : MonoBehaviour
{
    public static WallManager Instance { get; private set; }

    [Header("Input")]
    public KeyCode resetKey = KeyCode.Space;

    [Header("Wall Prefab")]
    public GameObject wallPrefab;

    private class WallSnapshot
    {
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;
        public string name;

        public int numChunks;
        public Material meshMaterial;
        public Material capMaterial;

        public WallSnapshot(DestructibleWall wall)
        {
            position = wall.transform.position;
            rotation = wall.transform.rotation;
            scale = wall.transform.localScale;
            name = wall.name;

            numChunks = wall.numChunks;
            meshMaterial = wall.meshMaterial;
            capMaterial = wall.capMaterial;
        }
    }

    private readonly List<WallSnapshot> snapshots = new List<WallSnapshot>();

    // Live wall references (null when destroyed)
    private readonly List<DestructibleWall> liveWalls = new List<DestructibleWall>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        SnapshotAllWalls();
    }

    private void Update()
    {
        if (Input.GetKeyDown(resetKey))
            ResetAllWalls();
    }

    public void ResetAllWalls()
    {
        DestroyAllChunks();
        RestoreAllWalls();
        Debug.Log("Reset walls");
    }

    private void SnapshotAllWalls()
    {
        snapshots.Clear();
        liveWalls.Clear();

        DestructibleWall[] found = FindObjectsByType<DestructibleWall>(
            FindObjectsSortMode.None);

        foreach (DestructibleWall wall in found)
        {
            liveWalls.Add(wall);
            snapshots.Add(new WallSnapshot(wall));
        }
    }

    private static void DestroyAllChunks()
    {
        GameObject[] allObjects = FindObjectsByType<GameObject>(
            FindObjectsSortMode.None);

        int count = 0;
        foreach (GameObject go in allObjects)
        {
            if (go.name.EndsWith("_Chunks", System.StringComparison.Ordinal))
            {
                Destroy(go);
                count++;
            }
        }
    }

    private void RestoreAllWalls()
    {
        // Destroy surviving walls so we start clean
        foreach (DestructibleWall wall in liveWalls)
        {
            if (wall != null)
                Destroy(wall.gameObject);
        }
        liveWalls.Clear();

        foreach (WallSnapshot snap in snapshots)
        {
            GameObject newWall = Instantiate(wallPrefab, snap.position,
                                             snap.rotation);
            newWall.name = snap.name;
            newWall.transform.localScale = snap.scale;

            // Restore DestructibleWall settings
            DestructibleWall dw = newWall.GetComponent<DestructibleWall>();
            if (dw != null)
            {
                dw.numChunks = snap.numChunks;
                dw.meshMaterial = snap.meshMaterial;
                dw.capMaterial = snap.capMaterial;
            }

            liveWalls.Add(dw);
        }
    }
}
