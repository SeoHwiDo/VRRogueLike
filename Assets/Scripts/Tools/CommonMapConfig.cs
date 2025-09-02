using UnityEngine;
[CreateAssetMenu(fileName = "CommonMapConfig", menuName = "PrefabConfig/CommonMap")]
public class CommonMapConfig  : ScriptableObject
{
    [Header("prefab")]
    public GameObject enemySpawnPointPrefab;
}
