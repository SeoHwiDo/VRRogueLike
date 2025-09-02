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


}
