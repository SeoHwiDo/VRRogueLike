using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance { get; private set; }
    private List<GameObject> atkEnemy = new List<GameObject>();
    private float HP;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void Start()
    {
        HP=GameManager.Instance.GetPlayerMaxHP();
    }
    public float GetPlayerHP()
    {
        return HP;
    }
    public void LosePlayerHP(float dmg)
    {
        HP = ((HP-dmg)>=0)?HP-dmg:0 ;
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
        if (enemyObj == null) return;
        // atkEnemy가 List<GameObject> 또는 ICollection이라 가정
        if (atkEnemy != null && atkEnemy.Contains(enemyObj))
        {
            atkEnemy.Remove(enemyObj);
        }
    }


}
