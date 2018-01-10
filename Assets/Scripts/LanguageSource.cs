using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class LanguageSource
{
    public enum LANGUAGE { ENGLISH, GERMAN };
    public static LANGUAGE current_language;

    private static Dictionary<string, string[]> language_string_dict = new Dictionary<string, string[]>()
    {
        { "please point", new string[] {"Please point to the ", "Bitte pointen du "} },
        { "wrong by", new string[] {"Not quite. The arrow will now show the exact direction. That was off by degrees: ", "Nien Good! Das ist correct die clost: "} },
        { "correct to within", new string[] {"Good! That was correct to within degrees: ", "Good! Das ist correct die clost: "} },
        { "all objects recall", new string[] {"Please recall all the objects that you delivered.", "Bitte rembers Alldustuffs."} },
        { "all stores recall", new string[] {"Please recall all the stores that you delivered objects to.", "Bitte rembers Alldushops."} },
        { "end message", new string[] {"Congratulations, you finished deliverying objects!  The game is over.  Your score: ", "Kongratulashunst!!  Zie overs Givenallziethingst!  Zie points: "} },
        { "next day", new string[] {"Press X to proceed to the next delivery day.", "Pressen X die go die Proceedendeliveriday"} },
        { "first day", new string[] {"Press Y to continue to the first delivery day, \n Press N to replay instructional video.", "Pressen Y die go die Premieredeliveriday"} },
        { "play movie", new string[] {"Press any key to play movie.", "Pressenziekechst die Spielemovie..."} },
        { "recording confirmation", new string[] {"Did you hear the recording ? \n(Y = Continue / N = Try Again / C = Cancel).", "Didst zie du hear de Soundst ? \n(Y = Go / N = Againtrysprechen / C = Cancel)."} },
        { "playing", new string[] {"Playing...", "Spiele..."} },
        { "recording", new string[] {"Recording...", "Rekordingst..."} },
        { "after the beep", new string[] {"Press any key to record a sound after the beep.", "Pressenbuttenzierecordusoundt."} },
        { "running participant", new string[] {"Running a new session of Delivery Person. \n Press Y to continue, N to quit.",
                                         "Runsprechen eine Newgame du Deleveriemann.\n Pressen Y zie continuech, N zie applicationstoppen.",} },
        { "begin session", new string[] {"Begin session", "Sessionstarchts"} },
        { "please find prompt", new string[] {"please find the ", "bitte finden "} },
        { "bakery", new string[] {"bakery", "die Backerei"} },
        { "barber shop", new string[] {"barber shop", " den Friseur"} },
        { "bike shop", new string[] {"bike shop", "den Fahrradladen"} },
        { "cafe", new string[] {"cafe", "bitte finden "} },
        { "clothing store", new string[] {" clothing store", "das Kleidungeschaft"} },
        { "dentist", new string[] {"dentist", "den Zahnarzt"} },
        { "craft shop", new string[] {"craft shop", "den Bastelladen"} },
        { "grocery store", new string[] {"grocery store", "den Supermarkt"} },
        { "jewelry store", new string[] {"jewelry store", "das Juwelier Geschaft"} },
        { "florist", new string[] {"florist", "den Blumenladen"} },
        { "hardware store", new string[] {"hardware store", "den Baumarkt"} },
        { "gym", new string[] {"gym", "das Fitness Studio"} },
        { "pizzeria", new string[] {"pizzeria", "die Pizzeria"} },
        { "pet store", new string[] {"pet store", "die Tierhandlung"} },
        { "music store", new string[] {"music store", "das Musikgeschaft"} },
        { "pharmacy", new string[] {"pharmacy", "die Apotheke"} },
        { "toy store", new string[] {"toy store", "den Spielwarenladen"} },
    };

    public static string GetLanguageString(string string_name)
    {
        if (!language_string_dict.ContainsKey(string_name))
            throw new UnityException("I don't have a language string called: " + string_name);
        return language_string_dict[string_name][(int)current_language];
    }
}
