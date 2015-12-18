#pragma strict


private var drawGUI = false;


function Update () 
{
	if (drawGUI == true && Input.GetKeyDown(KeyCode.E))
	{
		Application.LoadLevel ("mcp_day");
	}
}

function OnTriggerEnter (theCollider : Collider)
{
	if (theCollider.tag == "Player")
	{
		drawGUI = true;
	}
}

function OnTriggerExit (theCollider : Collider)
{
	if (theCollider.tag == "Player")
	{
		drawGUI = false;
	}
}

function OnGUI ()
{
	if (drawGUI == true)
	{
		GUI.Box (Rect (Screen.width*0.48-51, 200, 202, 22), "Press E to sleep until morning");
	}
}

