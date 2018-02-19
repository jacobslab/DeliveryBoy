using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LanguageToggler : MonoBehaviour
{
    public GameObject[] hideUsOnGerman;
    public UnityEngine.UI.Toggle toggleMeOnGerman;

    public void SetCurrentLanguage(int language)
    {
        LanguageSource.current_language = (LanguageSource.LANGUAGE)language;
        toggleMeOnGerman.isOn = false;
        foreach (GameObject us in hideUsOnGerman)
        {
            us.SetActive(language != (int)LanguageSource.LANGUAGE.GERMAN);
        }
    }
}