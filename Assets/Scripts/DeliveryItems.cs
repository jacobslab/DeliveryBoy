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

    private System.Random random;

    public Dictionary<string, List<AudioClip>> storeNamesToItems;

    void Start()
    {
        random = new System.Random(UnityEPL.GetParticipants()[0].GetHashCode());
    }

    public string PopStoreName()
    {
        if (unused_store_names.Count < 1)
        {
            throw new UnityException("I ran out of store names!");
        }
        unused_store_names.Shuffle(random);
        string storeName = unused_store_names[0];
        unused_store_names.RemoveAt(0);
        return storeName;
    }

    public AudioClip PopItem(string storeName)
    {
        return new AudioClip();
    }

    public string MostRecentlyPoppedItem(string storeName)
    {
        return "";
    }

    public static bool ItemsExhausted()
    {
        return true;
    }
}