var Material1 : Material;
 
var Material2 : Material;
 
 
 
 
function Start() {
changeMaterial();
 
}
 
 
function Update () {
 
}
 
 
function changeMaterial () {
 
for(i=1;i>0;i++) {
 
yield WaitForSeconds(20);
 
GetComponent.<Renderer>().material = Material1;
 
yield WaitForSeconds(20);
 
GetComponent.<Renderer>().material = Material2;
 
}
 
}