using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LanguageToggler : MonoBehaviour
{


    public void SetCurrentLanguage(int language)
    {
        LanguageSource.current_language = (LanguageSource.LANGUAGE)language;
    }

}
