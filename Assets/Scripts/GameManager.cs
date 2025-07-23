using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public HashSet<string> AllowedTags = new HashSet<string> { "basic", "hard" };

    [Header("Player")]
    public GameObject player;

    [Header("UI")]
    public GameObject endUI;


    [Header("Game State")]
    public int level = 1;
    public int maxLevel = 4;
    public float HP = 10;
    public int droneNum;
    public int droneKill = 0;
    public bool isGameOver = false;

    private IEnumerator InitGameSequence()
    {
        yield return StartCoroutine(MapManager.Instance.PreloadMapPrefabs(() =>
        {
            MapManager.Instance.GenerateMap();
            player.transform.position = MapManager.Instance.GetPlayerStartPosition();
        }));

        yield return StartCoroutine(EnemySpawnManager.Instance.PreloadEnemyPrefabs(() =>
        {
            StartCoroutine(EnemySpawnManager.Instance.Spawn());
        }));
    }
    void Awake()
    {

        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;

        DontDestroyOnLoad(gameObject); // 필요 시
    }
    void Start()
    {
        MapManager.Instance.SetMapConfig(6, 12); //맵 수정 필요한 경우
        StartCoroutine(InitGameSequence());

    }
    void Update()
    {
        //if (Input.GetMouseButtonDown(0))
        //{
        //    MapManager.Instance.refreshMap();
        //}
        
    }
    public void EnterSelectSkill()
    {
        Time.timeScale = 0;
    }
    public void OutSelectSkill()
    {
        Time.timeScale = 1;
    }
    
}
