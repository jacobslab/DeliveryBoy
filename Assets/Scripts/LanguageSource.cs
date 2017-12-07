using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class LanguageSource
{
    public enum LANGUAGE { ENGLISH, GERMAN };
    public static LANGUAGE current_language;

    private static Dictionary<string, string[]> language_string_dict = new Dictionary<string, string[]>()
    {
        { "please find prompt", new string[] {"please find the", "bitte finden der"} },
    };

    public static string GetLanguageString(string string_name)
    {
        return language_string_dict[string_name][(int)current_language];
    }
}
