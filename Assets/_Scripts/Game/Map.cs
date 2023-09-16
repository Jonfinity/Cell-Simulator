using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Map : MonoBehaviour
{
    public static Map instance;

    private const int MAP_SIZE_HUGE = 1000;
    private const int MAP_SIZE_LARGE = 600;
    private const int MAP_SIZE_MEDIUM = 400;
    private const int MAP_SIZE_SMALL = 200;

    [SerializeField] private LineRenderer lineRenderer;
    //[SerializeField] private MeshFilter meshFilter;
    //[SerializeField] private MeshRenderer meshRenderer;

    [SerializeField] private Canvas mapCanvas;
    [SerializeField] public Image map;

    private static int mapSize;
    public static int halfMapSize;
    public static int maxFood;
    public static int maxWhiteHole;
    public static int maxViruses;
    private static int maxPlayers;
    public static float orthographicSpectatingSize;

    private void Awake() 
    {
        instance = this;

        switch(MainMenuHandler.mapSize)
        {
            case 0:
                mapSize = MAP_SIZE_HUGE;
                break;
            case 1:
                mapSize = MAP_SIZE_LARGE;
                break;
            case 2:
                mapSize = MAP_SIZE_MEDIUM;
                break;
            case 3:
                mapSize = MAP_SIZE_SMALL;
                break;
        }
        MainMenuHandler.mapSize = 0;

        halfMapSize = mapSize / 2;
        maxFood = Mathf.RoundToInt(mapSize * 7 - 1000);
        ////maxWhiteHole = Mathf.RoundToInt(mapSize / 60);
        maxViruses = Mathf.RoundToInt(mapSize / 30);
        maxPlayers = Mathf.RoundToInt(mapSize / 12);
        orthographicSpectatingSize = halfMapSize * 1.2f;

        mapCanvas.GetComponent<RectTransform>().sizeDelta = new Vector2(mapSize * 10, mapSize * 10);
        map.rectTransform.sizeDelta = new Vector2(mapSize * 10, mapSize * 10);
        
        lineRenderer.positionCount = 4;
        lineRenderer.loop = true;
        lineRenderer.startWidth = 0.5f;
        lineRenderer.SetPosition(0, new Vector2(-halfMapSize, -halfMapSize));//bottomLeft
        lineRenderer.SetPosition(1, new Vector2(-halfMapSize, halfMapSize));//topLeft
        lineRenderer.SetPosition(2, new Vector2(halfMapSize, halfMapSize));//topRight
        lineRenderer.SetPosition(3, new Vector2(halfMapSize, -halfMapSize));//bottomRight

        /*Mesh mesh = new Mesh();
        List<Vector3> verticies = new List<Vector3>();
        List<int> indicies = new List<int>();
        for (int i = 0; i <= mapSize; i++)
        {
            verticies.Add(new Vector2(i, 0));
            verticies.Add(new Vector2(i, mapSize));

            indicies.Add((4 * i));
            indicies.Add((4 * i + 1));

            verticies.Add(new Vector2(0, i));
            verticies.Add(new Vector2(mapSize, i));

            indicies.Add((4 * i + 2));
            indicies.Add((4 * i + 3));
        }

        mesh.vertices = verticies.ToArray();
        mesh.SetIndices(indicies.ToArray(), MeshTopology.Lines, 0);
        meshFilter.mesh = mesh;
        meshRenderer.material.color = Color.white;
        meshRenderer.transform.position = corner;*/
    }

    private void Start()
    {
        int bots = Mathf.RoundToInt(mapSize / 60);
        //bots = 0;
        for (int i = 0; i < bots; i++)
        {
            Instantiate(Game.instance.ai, GenerateRandomPosition(), Quaternion.identity);
        }
    }

    public static Vector2 GenerateRandomPosition()
    {
        return new Vector2(Random.Range(-Map.halfMapSize, Map.halfMapSize), Random.Range(-Map.halfMapSize, Map.halfMapSize));
    }

    public static void BorderCheck(Rigidbody2D rb)
    {
        if (rb.transform.position.x >= Map.halfMapSize)
        {
            rb.transform.position = new Vector2(Map.halfMapSize, rb.transform.position.y);
        } else if (rb.transform.position.x <= -Map.halfMapSize)
        {
            rb.transform.position = new Vector2(-Map.halfMapSize, rb.transform.position.y);
        }

        if (rb.transform.position.y >= Map.halfMapSize)
        {
            rb.transform.position = new Vector2(rb.transform.position.x, Map.halfMapSize);
        } else if (rb.transform.position.y <= -Map.halfMapSize)
        {
            rb.transform.position = new Vector2(rb.transform.position.x, -Map.halfMapSize);
        }
    }
}
