using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeliveryZone : MonoBehaviour
{
    private bool playerInDeliveryZone = false;
    public GameObject pointerCircle;

    public bool PlayerInDeliveryZone()
    {
        return playerInDeliveryZone;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag.Equals("Player"))
            playerInDeliveryZone = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag.Equals("Player"))
            playerInDeliveryZone = false;
    }
}
