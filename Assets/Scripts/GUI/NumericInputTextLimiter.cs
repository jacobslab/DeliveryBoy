using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class NumericInputTextLimiter : MonoBehaviour {

	public InputField myNumericTextField;
	public int maxNum;
	public int minNum;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {

	}
	
	public void UpdateText(){
		if (int.Parse(myNumericTextField.text) < minNum) {
			myNumericTextField.text = minNum.ToString();
		}
		else if (int.Parse(myNumericTextField.text) > maxNum) {
			myNumericTextField.text = maxNum.ToString();
		}
	}

	public int GetNumValue(){
		return int.Parse (myNumericTextField.text);
	}
}
