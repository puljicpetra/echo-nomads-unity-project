using UnityEngine;

public class WorldBoundaries : MonoBehaviour
{
    [Header("World Dimensions")]
    public float worldWidth = 2048f;
    public float worldHeight = 1000f;
    public float worldDepth = 2048f;

    [Header("Wall Thickness")]
    public float wallThickness = 10f;

    private void Start()
    {
        CreateWalls();
    }

    private void CreateWalls()
    {
        // Left wall
        CreateWall("LeftWall",
            new Vector3(0f, worldHeight * 0.5f, worldDepth * 0.5f),
            new Vector3(wallThickness, worldHeight, worldDepth));

        // Right wall
        CreateWall("RightWall",
            new Vector3(worldWidth, worldHeight * 0.5f, worldDepth * 0.5f),
            new Vector3(wallThickness, worldHeight, worldDepth));

        // Front wall
        CreateWall("FrontWall",
            new Vector3(worldWidth * 0.5f, worldHeight * 0.5f, 0f),
            new Vector3(worldWidth, worldHeight, wallThickness));

        // Back wall
        CreateWall("BackWall",
            new Vector3(worldWidth * 0.5f, worldHeight * 0.5f, worldDepth),
            new Vector3(worldWidth, worldHeight, wallThickness));
    }

    private void CreateWall(string wallName, Vector3 position, Vector3 scale)
    {
        // Create a new GameObject for our wall
        GameObject wall = new GameObject(wallName);
        wall.transform.SetParent(transform);
        wall.transform.position = position;
        wall.transform.localScale = scale;

        // Add a BoxCollider to make it solid
        BoxCollider collider = wall.AddComponent<BoxCollider>();

        // Make it invisible by not adding any renderer
        // (Alternatively, collider.hideFlags = HideFlags.HideInInspector; if you want to hide the collider in Inspector)
    }
}
