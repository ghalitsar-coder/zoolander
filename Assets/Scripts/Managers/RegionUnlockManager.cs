using UnityEngine;
using System.Collections.Generic;

public class RegionUnlockManager : MonoBehaviour
{
    public static RegionUnlockManager instance;

    private Dictionary<int, bool> unlockStatus = new Dictionary<int, bool>();

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Status awal: Wilayah 1 terbuka, Wilayah 2 terkunci
        unlockStatus[1] = true;
        unlockStatus[2] = false;
    }

    public void UnlockRegion(int id)
    {
        if (!unlockStatus.ContainsKey(id))
            unlockStatus[id] = false;

        if (!unlockStatus[id])
        {
            unlockStatus[id] = true;
            Debug.Log("? Wilayah " + id + " sekarang terbuka!");
        }
    }

    public bool IsRegionUnlocked(int id)
    {
        return unlockStatus.ContainsKey(id) && unlockStatus[id];
    }
}