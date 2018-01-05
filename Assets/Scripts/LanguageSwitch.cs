using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LanguageSwitch : MonoBehaviour
{
    public string[] languageStrings;

	// Update is called once per frame
	void Update ()
    {
        GetComponent<UnityEngine.UI.Text>().text = languageStrings[(int)LanguageSource.current_language];
    }
}
