using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class MazeGenerator : MonoBehaviour
{
    [Header("Maze Dimensions")]
    public int width = 10;
    public int length = 10;
    public float cellSize = 2f;

    [Header("Visual Properties")]
    public float wallHeight = 3f;
    public float wallThickness = 0.5f;
    public Material mazeMaterial;
    public Material floorMaterial;

    [Header("Entities")]
    public GameObject playerPrefab;
    public GameObject enemyPrefab;
    public int numberOfEnemies = 5;
    public float entitySpawnHeight = 1f;

    [Header("Level Generation")]
    public int levelSeed = 12345;
    public bool autoUpdateInEditor = true;

    private int previousSeed;

    private class Cell
    {
        public bool visited = false;
        public bool northWall = true;
        public bool southWall = true;
        public bool eastWall = true;
        public bool westWall = true;
    }

    private Cell[,] grid;
    private GameObject mazeParent;

    void Start()
    {
        previousSeed = levelSeed;
        if (Application.isPlaying) GenerateMaze();
    }

    void Update()
    {
        if (Application.isPlaying && levelSeed != previousSeed)
        {
            previousSeed = levelSeed;
            GenerateMaze();
        }
    }


    public void GenerateMaze()
    {
        if (mazeParent != null)
        {
            #if UNITY_EDITOR
                        // Force Unity to deselect the maze so the Inspector doesn't crash
                        if (UnityEditor.Selection.activeGameObject != null && 
                            UnityEditor.Selection.activeGameObject.transform.IsChildOf(mazeParent.transform))
                        {
                            UnityEditor.Selection.activeObject = null;
                        }
            #endif

                        if (Application.isPlaying) Destroy(mazeParent);
                        else DestroyImmediate(mazeParent);
        }
        
        mazeParent = new GameObject("Generated_Maze");
        mazeParent.transform.SetParent(this.transform);

        Random.InitState(levelSeed);

        grid = new Cell[width, length];
        for (int x = 0; x < width; x++)
            for (int z = 0; z < length; z++)
                grid[x, z] = new Cell();

        Stack<Vector2Int> stack = new Stack<Vector2Int>();
        Vector2Int currentCell = new Vector2Int(0, 0);
        grid[currentCell.x, currentCell.y].visited = true;

        int unvisitedCount = (width * length) - 1;

        while (unvisitedCount > 0)
        {
            List<Vector2Int> unvisitedNeighbors = GetUnvisitedNeighbors(currentCell);

            if (unvisitedNeighbors.Count > 0)
            {
                Vector2Int chosen = unvisitedNeighbors[Random.Range(0, unvisitedNeighbors.Count)];
                stack.Push(currentCell);
                RemoveWall(currentCell, chosen);
                
                currentCell = chosen;
                grid[currentCell.x, currentCell.y].visited = true;
                unvisitedCount--;
            }
            else if (stack.Count > 0)
            {
                currentCell = stack.Pop();
            }
        }

        BuildOptimizedVisualMaze();
        BuildFloor();
        BuildNavMesh();
        SpawnPlayer();
        SpawnEnemies();
    }

    public void SaveMazeToScene()
    {
        if (mazeParent != null)
        {
            mazeParent.name = "Saved_Maze_Seed_" + levelSeed;
            mazeParent.transform.SetParent(null);
            mazeParent = null; 
            Debug.Log("Maze saved permanently to the scene!");
        }
    }

    private void BuildNavMesh()
    {
        NavMeshSurface surface = mazeParent.AddComponent<NavMeshSurface>();
        surface.collectObjects = CollectObjects.Children;
        surface.BuildNavMesh();
    }

    private void SpawnPlayer()
    {
        if (playerPrefab == null) return;
        Vector3 playerStartPos = new Vector3(0, entitySpawnHeight, 0);
        GameObject player = Instantiate(playerPrefab, playerStartPos, Quaternion.identity);
        player.name = "Player";
        player.transform.SetParent(mazeParent.transform);
    }

    private void SpawnEnemies()
    {
        if (enemyPrefab == null || numberOfEnemies <= 0) return;

        List<Vector2Int> possibleSpawns = new List<Vector2Int>();
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < length; z++)
            {
                if (x == 0 && z == 0) continue; 
                possibleSpawns.Add(new Vector2Int(x, z));
            }
        }

        int enemiesToSpawn = Mathf.Min(numberOfEnemies, possibleSpawns.Count);

        for (int i = 0; i < enemiesToSpawn; i++)
        {
            int randomIndex = Random.Range(0, possibleSpawns.Count);
            Vector2Int spawnGridPos = possibleSpawns[randomIndex];
            possibleSpawns.RemoveAt(randomIndex);

            Vector3 spawnPos = new Vector3(spawnGridPos.x * cellSize, entitySpawnHeight, spawnGridPos.y * cellSize);
            GameObject enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
            enemy.name = "Enemy_" + i;
            enemy.transform.SetParent(mazeParent.transform);
        }
    }

    private void BuildFloor()
    {
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        floor.name = "Floor";
        floor.transform.SetParent(mazeParent.transform);

        float centerX = (width - 1) * cellSize / 2f;
        float centerZ = (length - 1) * cellSize / 2f;
        floor.transform.position = new Vector3(centerX, 0, centerZ);

        float totalWidth = width * cellSize;
        float totalLength = length * cellSize;
        floor.transform.localScale = new Vector3(totalWidth / 10f, 1, totalLength / 10f);

        if (floorMaterial != null)
        {
            Renderer renderer = floor.GetComponent<Renderer>();
            if (renderer != null) renderer.material = floorMaterial;
        }
    }

    private List<Vector2Int> GetUnvisitedNeighbors(Vector2Int cell)
    {
        List<Vector2Int> neighbors = new List<Vector2Int>();
        if (cell.y + 1 < length && !grid[cell.x, cell.y + 1].visited) neighbors.Add(new Vector2Int(cell.x, cell.y + 1));
        if (cell.y - 1 >= 0 && !grid[cell.x, cell.y - 1].visited) neighbors.Add(new Vector2Int(cell.x, cell.y - 1));
        if (cell.x + 1 < width && !grid[cell.x + 1, cell.y].visited) neighbors.Add(new Vector2Int(cell.x + 1, cell.y));
        if (cell.x - 1 >= 0 && !grid[cell.x - 1, cell.y].visited) neighbors.Add(new Vector2Int(cell.x - 1, cell.y));
        return neighbors;
    }

    private void RemoveWall(Vector2Int current, Vector2Int neighbor)
    {
        if (current.x == neighbor.x)
        {
            if (current.y < neighbor.y) { grid[current.x, current.y].northWall = false; grid[neighbor.x, neighbor.y].southWall = false; }
            else { grid[current.x, current.y].southWall = false; grid[neighbor.x, neighbor.y].northWall = false; }
        }
        else
        {
            if (current.x < neighbor.x) { grid[current.x, current.y].eastWall = false; grid[neighbor.x, neighbor.y].westWall = false; }
            else { grid[current.x, current.y].westWall = false; grid[neighbor.x, neighbor.y].eastWall = false; }
        }
    }

    private void BuildOptimizedVisualMaze()
    {
        bool[,] hWalls = new bool[width, length + 1];
        bool[,] vWalls = new bool[width + 1, length];

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < length; z++)
            {
                if (grid[x, z].northWall) hWalls[x, z + 1] = true;
                if (grid[x, z].southWall && z == 0) hWalls[x, 0] = true;
                if (grid[x, z].eastWall) vWalls[x + 1, z] = true;
                if (grid[x, z].westWall && x == 0) vWalls[0, z] = true;
            }
        }

        for (int z = 0; z <= length; z++)
        {
            int startX = -1;
            for (int x = 0; x <= width; x++)
            {
                bool isWall = (x < width) && hWalls[x, z];

                if (isWall && startX == -1) startX = x; 
                else if (!isWall && startX != -1)
                {
                    int endX = x - 1; 
                    float centerX = (startX + endX) * cellSize / 2f;
                    float centerZ = z * cellSize - (cellSize / 2f);
                    float stretchLength = ((endX - startX + 1) * cellSize) + wallThickness;

                    CreateCubeWall(new Vector3(centerX, wallHeight / 2f, centerZ), new Vector3(stretchLength, wallHeight, wallThickness));
                    startX = -1;
                }
            }
        }

        for (int x = 0; x <= width; x++)
        {
            int startZ = -1;
            for (int z = 0; z <= length; z++)
            {
                bool isWall = (z < length) && vWalls[x, z];

                if (isWall && startZ == -1) startZ = z; 
                else if (!isWall && startZ != -1)
                {
                    int endZ = z - 1; 
                    float centerX = x * cellSize - (cellSize / 2f);
                    float centerZ = (startZ + endZ) * cellSize / 2f;
                    float stretchLength = ((endZ - startZ + 1) * cellSize) - wallThickness;

                    if (stretchLength > 0)
                    {
                        CreateCubeWall(new Vector3(centerX, wallHeight / 2f, centerZ), new Vector3(wallThickness, wallHeight, stretchLength));
                    }
                    startZ = -1;
                }
            }
        }
    }

    private void CreateCubeWall(Vector3 position, Vector3 scale)
    {
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        
        wall.transform.position = position;
        wall.transform.localScale = scale;
        wall.transform.SetParent(mazeParent.transform);

        if (mazeMaterial != null)
        {
            Renderer renderer = wall.GetComponent<Renderer>();
            if (renderer != null) renderer.material = mazeMaterial;
        }
    }
}

// =========================================================================
// CUSTOM EDITOR
// =========================================================================
#if UNITY_EDITOR
[CustomEditor(typeof(MazeGenerator))]
public class MazeGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        MazeGenerator generator = (MazeGenerator)target;

        EditorGUI.BeginChangeCheck();
        
        DrawDefaultInspector();

        if (EditorGUI.EndChangeCheck())
        {
            if (!Application.isPlaying && generator.autoUpdateInEditor)
            {
                EditorApplication.delayCall += () => {
                    if (generator != null) generator.GenerateMaze();
                };
            }
        }

        GUILayout.Space(15);
        if (GUILayout.Button("Generate Maze Now", GUILayout.Height(30)))
        {
            generator.GenerateMaze();
        }

        GUILayout.Space(5);
        
        Color originalColor = GUI.backgroundColor;
        GUI.backgroundColor = Color.green;
        
        if (GUILayout.Button("YES - Save/Bake Maze to Scene", GUILayout.Height(40)))
        {
            generator.SaveMazeToScene();
        }
        
        GUI.backgroundColor = originalColor;
    }
}
#endif