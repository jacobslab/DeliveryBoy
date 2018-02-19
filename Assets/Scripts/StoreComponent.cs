using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoreComponent : MonoBehaviour
{
    private DeliveryZone deliveryZone;
    private string storeName;
    private string mostRecentlyPoppedItem;

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
            string displayString = LanguageSource.GetLanguageString(storeName).ToUpper();
            if (displayString.Equals("GROCERY STORE"))
                displayString = "GROCERY";
            if (displayString.Equals("HARDWARE STORE"))
                displayString = "HARDWARE";
            if (displayString.Equals("CLOTHING STORE"))
                displayString = "CLOTHING";
            if (displayString.Equals("JEWELRY STORE"))
                displayString = "JEWELRY";
            if (LanguageSource.current_language == LanguageSource.LANGUAGE.GERMAN)
            {
                string[] germanWords = displayString.Split(' ');
                if (germanWords.Length == 2)
                    displayString = germanWords[1];
                displayString = displayString[0].ToString().ToUpper() + displayString.Substring(1).ToLower();
            }
            if (displayString.Equals("Kleidungsgeschäft"))
                displayString = "Kleidung";
            if (displayString.Equals("Spielwarenladen"))
                displayString = "Spielwaren";
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
        AudioClip item = deliveryItems.PopItem(storeName);
        mostRecentlyPoppedItem = item.name;
        return item;
    }

    public string GetLastPoppedItemName()
    {
        return mostRecentlyPoppedItem;
    }
}