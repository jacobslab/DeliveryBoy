using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoreComponent : MonoBehaviour
{
    private static Dictionary<string, StoreComponent> storesByName = new Dictionary<string, StoreComponent>();

    public string storeName;
    public List<AudioClip> englishItems;
    public List<AudioClip> germanItems;

    void Start()
    {
        storesByName[storeName] = this;
    }

    public bool PlayerInDeliveryBox()
    {
        return false;
    }
}