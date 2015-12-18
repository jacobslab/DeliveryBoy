function Start ()
{
    Cursor.visible = false;
}
 
function Update () 
{
 if (Input.GetKey (KeyCode.Tab))
 {
     Screen.lockCursor = false;
     Cursor.visible = false;
 }   
 else
 {
      Screen.lockCursor = true; 
      Cursor.visible = true;
 }
 
}