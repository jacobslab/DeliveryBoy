using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MessageImageDisplayer : MonoBehaviour
{
    public GameObject[] practice_phase_messages;
    public GameObject[] final_recall_messages;
    public GameObject[] delivery_restart_messages;
    public GameObject[] store_images_presentation_messages;

    public GameObject please_find_the_blah;
    public UnityEngine.UI.Text please_find_the_blah_text;
    public GameObject please_find_the_blah_reminder;
    public UnityEngine.UI.Text please_find_the_blah_reminder_text;

    public IEnumerator DisplayLanguageMessage(GameObject[] language_messages)
    {
        yield return DisplayMessage(language_messages[(int)LanguageSource.current_language]);
    }

    private IEnumerator DisplayMessage (GameObject message)
    {
        message.SetActive(true);
        yield return null;
        while (!Input.GetButtonDown("x (continue)"))
            yield return null;
        message.SetActive(false);
    }

    public void SetReminderText(string store_name)
    {
        string prompt_string = LanguageSource.GetLanguageString("please find prompt") + LanguageSource.GetLanguageString(store_name);
        please_find_the_blah_reminder_text.text = prompt_string;
    }
}
