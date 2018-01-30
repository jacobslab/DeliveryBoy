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
        {
            string displayString = storeName.ToUpper();
            if (displayString.Equals("GROCERY STORE"))
                displayString = "GROCERY";
            if (displayString.Equals("HARDWARE STORE"))
                displayString = "HARDWARE";
            if (displayString.Equals("CLOTHING STORE"))
                displayString = "CLOTHING";
            if (displayString.Equals("JEWELRY STORE"))
                displayString = "JEWELRY";
            textComponent.text = displayString;
        }
    }

    public bool IsVisible()
    {
        bool thisIsVisible = false;
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        List<bool> renderesVisible = new List<bool>();
        foreach (Renderer theRenderer in renderers)
            renderesVisible.Add(theRenderer.isVisible);
        foreach (bool isVisible in renderesVisible)
            thisIsVisible |= isVisible;
        Debug.Log(storeName + " is visible: " + thisIsVisible.ToString());
        return thisIsVisible;
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