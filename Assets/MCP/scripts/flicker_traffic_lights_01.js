var Material1 : Material;
 
var Material2 : Material;

var Material3 : Material;

var Material4 : Material;
 
 
 
 
function Start() {
changeMaterial();
 
}
 
 
function Update () {
 
}
 
 
function changeMaterial () {
 
for(i=1;i>0;i++) {
 
yield WaitForSeconds(15);
 
GetComponent.<Renderer>().material = Material1;
 
yield WaitForSeconds(5);
 
GetComponent.<Renderer>().material = Material2;

yield WaitForSeconds(15);
 
GetComponent.<Renderer>().material = Material3;

yield WaitForSeconds(5);
 
GetComponent.<Renderer>().material = Material4;
 
}
 
}