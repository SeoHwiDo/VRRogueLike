using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public HashSet<string> AllowedTags = new HashSet<string> { "basic", "hard" };

    [Header("Player")]
    public GameObject player;
    [SerializeField]private float MaxHP = 10;
    [SerializeField]private float bulletSpeed = 10f;
    [SerializeField]private float bulletDamage = 1f;
    [SerializeField]private int bulletMax = 25;

    [Header("Game State")]
    [SerializeField] private int level = 1;
    [SerializeField] private int maxLevel = 4;
    [SerializeField] private bool isGameOver = false;
    public float GetPlayerMaxHP()
    {
        return MaxHP;
    }
    public float GetBulletDamage()
    {
        return bulletDamage;
    }
    public float GetBulletSpeed()
    {
        return bulletSpeed;
    }
    public void SetBulletSpeed(float speed)
    {
        bulletSpeed = speed;
    }
    public void AddBulletMax(int max)
    {
        bulletMax += max;
    }


    private IEnumerator InitGameSequence()
    {
        yield return StartCoroutine(AssetManager.Instance.AssetPreload(() =>
        {
            MapManager.Instance.SetInitializedPrefab();
            EnemyManager.Instance.SetInitializedPrefab();
            MapManager.Instance.GenerateMap();
            player.transform.position = MapManager.Instance.GetPlayerStartPosition();
            StartCoroutine(EnemyManager.Instance.Spawn());
        }));
    }
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }
    void Start()
    {
        MapManager.Instance.SetMapConfig(6, 12); //맵 수정 필요한 경우
        StartCoroutine(InitGameSequence());
        UIManager.Instance.UpdateLevelUI(level);
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
