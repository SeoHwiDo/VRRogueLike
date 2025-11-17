using UnityEngine;
[CreateAssetMenu(fileName = "EnemyConfig", menuName = "PrefabConfig/Enemy")]

public class EnemyConfig : ScriptableObject
{
    [Header("name")]
    public string Name;

    [Header("prefab")]
    public GameObject[] enemyPrefab;
    public GameObject enemyDeadPtc;
    public GameObject enemySpawnPtc;
    public GameObject enemyHitPtc;

}
