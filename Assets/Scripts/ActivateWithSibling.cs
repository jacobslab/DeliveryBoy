using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivateWithSibling : MonoBehaviour
{
    public GameObject siblingFollower;
    public GameObject siblingLeader;

	void Update ()
    {
        siblingFollower.gameObject.SetActive(siblingLeader.activeSelf);
	}
}
