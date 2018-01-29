using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoreComponent : MonoBehaviour
{
    private DeliveryZone deliveryZone;
    private string storeName;

    public DeliveryItems deliveryItems;
    public GameObject familiarization_object;

    public string GetStoreName()
    {
        return storeName;
    }

    void Start()
    {
        deliveryZone = GetComponentInChildren<DeliveryZone>();

        storeName = deliveryItems.PopStoreName();
        DrawSigns();
    }

    void DrawSigns()
    {
		List<UnityEngine.TextMesh> updateUs = new List<UnityEngine.TextMesh>();
		updateUs.AddRange (gameObject.GetComponentsInChildren<UnityEngine.TextMesh> ());
		updateUs.AddRange (familiarization_object.GetComponentsInChildren<UnityEngine.TextMesh> ());
		foreach (UnityEngine.TextMesh textComponent in updateUs)
			textComponent.text = storeName.ToUpper();
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
        return deliveryItems.PopItem(storeName);
    }

    public string GetLastPoppedItemName()
    {
        return deliveryItems.MostRecentlyPoppedItem(storeName);
    }
}