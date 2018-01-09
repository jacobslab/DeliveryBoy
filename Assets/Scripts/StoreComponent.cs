using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoreComponent : MonoBehaviour
{
    private static Dictionary<string, StoreComponent> storesByName = new Dictionary<string, StoreComponent>();
    private DeliveryZone deliveryZone;
    private List<AudioClip>[] itemLists;
    private string last_popped_item_name;

    public string storeName;
    public GameObject familiarization_object;
    public List<AudioClip> englishItems;
    public List<AudioClip> germanItems;

    void Start()
    {
        deliveryZone = GetComponentInChildren<DeliveryZone>();
        storesByName[storeName] = this;
        itemLists = new List<AudioClip>[] { englishItems, germanItems };
    }

    public bool IsVisible()
    {
        return GetComponentInChildren<Renderer>().isVisible;
    }

    public bool PlayerInDeliveryPosition()
    {
        return deliveryZone.PlayerInDeliveryZone();
    }

    public AudioClip PopItem()
    {
        List<AudioClip> languageItems = itemLists[(int)LanguageSource.current_language];
        int randomIndex = Random.Range(0, languageItems.Count);
        AudioClip randomItem = languageItems[randomIndex];
        languageItems.RemoveAt(randomIndex);
        last_popped_item_name = randomItem.name;
        return randomItem;
    }

    public string GetLastPoppedItemName()
    {
        return last_popped_item_name;
    }
}