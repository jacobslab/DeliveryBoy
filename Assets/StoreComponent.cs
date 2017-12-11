using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoreComponent : MonoBehaviour
{
    private static Dictionary<string, StoreComponent> storesByName = new Dictionary<string, StoreComponent>();
    private Collider deliveryBox;
    private bool playerInDeliveryBox = false;

    public string storeName;
    public List<AudioClip> englishItems;
    public List<AudioClip> germanItems;

    void Start()
    {
        storesByName[storeName] = this;
        deliveryBox = GetComponent<Collider>();
    }

    public bool PlayerInDeliveryBox()
    {
        return playerInDeliveryBox;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag.Equals("Player"))
            playerInDeliveryBox = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag.Equals("Player"))
            playerInDeliveryBox = false;
    }
}