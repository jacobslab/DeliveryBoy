using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivateWithSibling : MonoBehaviour
{
    public GameObject sibling;

	void Update ()
    {
        gameObject.SetActive(sibling.activeSelf);
	}
}
