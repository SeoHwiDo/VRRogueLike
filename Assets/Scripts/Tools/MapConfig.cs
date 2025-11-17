using UnityEngine;

[CreateAssetMenu(fileName = "MapConfig", menuName = "PrefabConfig/Map")]
public class MapConfig : ScriptableObject
{
    [Header("name")]
    public string Name;

    [Header("prefab")]
    public GameObject cornerPrefab;
    public GameObject wallPrefab;
    public GameObject doorPrefab;
    public GameObject[] innerTilePrefab;

}
