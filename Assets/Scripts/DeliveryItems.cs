using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeliveryItems : MonoBehaviour
{
    private static string[] storeNames = new string[] { "barber shop",
                                                        "bakery",
                                                        "bike shop",
                                                        "cafe",
                                                        "clothing store",
                                                        "dentist",
                                                        "craft shop",
                                                        "grocery store",
                                                        "jewelry store",
                                                        "florist",
                                                        "hardware store",
                                                        "gym",
                                                        "pizzeria",
                                                        "pet store",
                                                        "music store",
                                                        "pharmacy",
                                                        "toy store" };
    private static List<string> unused_store_names = new List<string>(storeNames);

    public Dictionary<string, List<AudioClip>> storeNamesToItems;

    public string PopStoreName()
    {
        if (unused_store_names.Count < 1)
        {
            throw new UnityException("I ran out of store names!");
        }
        unused_store_names.Shuffle();
        string storeName = unused_store_names[0];
        unused_store_names.RemoveAt(0);
        return storeName;
    }

    public AudioClip PopItem(string storeName)
    {
        
    }

    public string MostRecentlyPoppedItem(string storeName)
    {
        
    }

    public static bool ItemsExhausted()
    {
        return true;
    }
}