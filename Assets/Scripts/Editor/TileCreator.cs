using UnityEngine;
using UnityEditor;

public class TileCreator : EditorWindow
{
    private GameObject tilePrefab;
    private GameObject parent;
    [SerializeField] private GameObject[] sideTiles; // Добавляем SerializeField
    private int width = 10;
    private int height = 10;
    private float spacing = 1.0f;

    private SerializedObject serializedObject;
    private SerializedProperty sideTilesProperty;

    [MenuItem("Tools/Tile Map Generator")]
    public static void ShowWindow()
    {
        GetWindow<TileCreator>("Tile Map Generator");
    }

    private void OnEnable()
    {
        // Инициализация SerializedObject
        serializedObject = new SerializedObject(this);
        sideTilesProperty = serializedObject.FindProperty("sideTiles");
    }

    private void OnGUI()
    {
        GUILayout.Label("Tile Map Generator", EditorStyles.boldLabel);

        // Обновляем SerializedObject
        serializedObject.Update();

        // Основной префаб тайла
        tilePrefab = (GameObject)EditorGUILayout.ObjectField(
            "Tile Prefab",
            tilePrefab,
            typeof(GameObject),
            false
        );

        // Родительский объект
        parent = (GameObject)EditorGUILayout.ObjectField(
            "Parent",
            parent, 
            typeof(GameObject),
            true
        );

        // Боковые тайлы - правильная реализация
        EditorGUILayout.PropertyField(sideTilesProperty, new GUIContent("Боковые тайлы"), true);

        // Параметры генерации
        width = EditorGUILayout.IntField("Width", width);
        height = EditorGUILayout.IntField("Height", height);
        spacing = EditorGUILayout.FloatField("Spacing", spacing);

        // Применяем изменения
        serializedObject.ApplyModifiedProperties();

        if (GUILayout.Button("Generate"))
        {
            GenerateTileMap();
        }

        if (GUILayout.Button("Clear Map") && parent != null)
        {
            DestroyImmediate(parent);
            parent = null;
        }
    }

    private void GenerateTileMap()
    {
        if (tilePrefab == null)
        {
            Debug.LogError("Tile Prefab is not assigned!");
            return;
        }

        if (parent == null)
        {
            parent = new GameObject("TileMap");
        }

        // Очищаем старые тайлы
        foreach (Transform child in parent.transform)
        {
            DestroyImmediate(child.gameObject);
        }

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 position = new Vector3(x * spacing, 0, y * spacing);
                GameObject tile = (GameObject)PrefabUtility.InstantiatePrefab(tilePrefab, parent.transform);
                tile.transform.position = position;
                tile.name = $"Tile_{x}_{y}";

                // Если есть боковые тайлы и это граница - используем их
                if (sideTiles != null && sideTiles.Length > 0 && 
                    (x == 0 || x == width-1 || y == 0 || y == height-1))
                {
                    DestroyImmediate(tile);
                    GameObject sideTilePrefab = sideTiles[Random.Range(0, sideTiles.Length)];
                    tile = (GameObject)PrefabUtility.InstantiatePrefab(sideTilePrefab, parent.transform);
                    tile.transform.position = position;
                    tile.name = $"SideTile_{x}_{y}";
                }
            }
        }

        Debug.Log($"Generated {width}x{height} tile map!");
    }
}