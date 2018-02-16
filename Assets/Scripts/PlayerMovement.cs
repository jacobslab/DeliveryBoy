﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
	public float forwardSpeed = 1f;
	public float backwardSpeed = 0.5f;
	public float turnSpeed = 1f;
    public float turnThreshhold = 0f;

	public GameObject rotateMe;
	public float maxRotation = 30f;

    private bool frozen;
	void Update ()
	{
        float turnAmount = Input.GetAxis("Horizontal");
        Debug.Log(turnAmount);
        if (Mathf.Abs(turnAmount) < turnThreshhold)
            turnAmount = 0;
        turnAmount = turnAmount * turnSpeed * Time.deltaTime;
        if (!frozen)
        {
            this.gameObject.transform.Rotate(new Vector3(0, turnAmount, 0));

            //move forward or more slowly backward
            if (Input.GetAxis("Vertical") > 0)
                this.gameObject.transform.position = Vector3.Lerp(this.gameObject.transform.position, this.gameObject.transform.position + Input.GetAxis("Vertical") * this.gameObject.transform.forward, forwardSpeed * Time.deltaTime);
            else
                this.gameObject.transform.position = Vector3.Lerp(this.gameObject.transform.position, this.gameObject.transform.position + Input.GetAxis("Vertical") * this.gameObject.transform.forward, backwardSpeed * Time.deltaTime);

            //rotate the handlebars smoothly, limit to maxRotation
            rotateMe.transform.localRotation = Quaternion.Euler(rotateMe.transform.rotation.eulerAngles.x, Input.GetAxis("Horizontal") * maxRotation, rotateMe.transform.rotation.eulerAngles.z);
        }
	}

    public void Freeze()
    {
        frozen = true;
    }

    public void Unfreeze()
    {
        frozen = false;
    }
}