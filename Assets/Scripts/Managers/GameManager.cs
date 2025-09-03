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
    [SerializeField]private float bulletSize = 1f;
    [SerializeField] private float bulletReloadSize = 3f;
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
    public float GetBulletSize()
    {
        return bulletSize;
    }
    {
        bulletSpeed = speed;
    }
    public void AddBulletMax(int max)
    {
        bulletMax += max;
    }
    public int GetBulletMax()
    {
        return bulletMax;
    }


    private void InitGameSequence()
    {
        MapManager.Instance.GenerateMap();
        player.transform.position = MapManager.Instance.GetPlayerStartPosition();
        StartCoroutine(EnemyManager.Instance.Spawn());
        
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
        //MapManager.Instance.SetMapConfig(6, 12); //맵 수정 필요한 경우
        InitGameSequence();
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
