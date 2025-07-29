using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
   public static PlayerManager Instance { get; private set; }
    [SerializeField] private InputManager _inputManager;
    [SerializeField] private CtrlManager _ctrlManager;
    private List<GameObject> atkEnemy = new List<GameObject>();
    private float MaxHP = 10;
    private float HP = 10;

    void Awake()
    {

        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;

        DontDestroyOnLoad(gameObject); // 필요 시
    }
    public float GetPlayerMaxHP()
    {
        return MaxHP;
    }
    public float GetPlayerHP()
    {
        return HP;
    }
    public void LosePlayerHP(float dmg)
    {
        HP -= dmg;
    }
    public List<GameObject> getatkEnemy()
    {
        return atkEnemy;
    }
    public void addAtkEnemy(GameObject enemyObj)
    {
        atkEnemy.Add(enemyObj);
    }
    public void removeAtkEnemy(GameObject enemyObj)
    {
        atkEnemy.Remove(enemyObj);
    }
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    void Update()
    {
        if (_inputManager == null || _ctrlManager == null)
            return;

        Vector2 lookDelta = _inputManager.GetLookDelta();

        if (lookDelta != Vector2.zero)
        {
            _ctrlManager.Look(lookDelta);
        }
    }

}
