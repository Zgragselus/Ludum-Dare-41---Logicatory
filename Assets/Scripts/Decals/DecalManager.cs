using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DecalManager : MonoBehaviour
{
    [SerializeField]
    [Tooltip("The prefab for the bullet hole")]
    private GameObject _decalPrefab;

    [SerializeField]
    [Tooltip("The number of decals to keep alive at a time.  After this number are around, old ones will be replaced.")]
    private int _decalMaxCount = 10;

    private Queue<GameObject> _decalsInPool;
    private Queue<GameObject> _decalsActiveInWorld;

    void Awake()
    {
        InitializeDecals();
    }

    void InitializeDecals()
    {
        _decalsInPool = new Queue<GameObject>();
        _decalsActiveInWorld = new Queue<GameObject>();

        for (int i = 0; i < _decalMaxCount; i++)
        {
            InstantiateDecal();
        }
    }

    void InstantiateDecal()
    {
        var spawned = GameObject.Instantiate(_decalPrefab);
        Vector3 scale = spawned.transform.localScale;
        spawned.transform.SetParent(this.transform);
        spawned.transform.localScale = scale;

        _decalsInPool.Enqueue(spawned);
        spawned.SetActive(false);
    }

    public void SpawnDecal(RaycastHit hit)
    {
        GameObject decal = GetNextAvailableDecal();
        if (decal != null)
        {
            decal.transform.position = hit.point;
            decal.transform.rotation = Quaternion.FromToRotation(-Vector3.forward, hit.normal);

            decal.SetActive(true);

            _decalsActiveInWorld.Enqueue(decal);
        }
    }

    GameObject GetNextAvailableDecal()
    {
        if (_decalsInPool.Count > 0)
            return _decalsInPool.Dequeue();

        var oldestActiveDecal = _decalsActiveInWorld.Dequeue();
        return oldestActiveDecal;
    }
}
