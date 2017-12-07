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

    public void DisplayLanguageMessage(GameObject[] language_messages)
    {
        StartCoroutine(DisplayMessage(language_messages[(int)LanguageSource.current_language]));
    }

    private IEnumerator DisplayMessage (GameObject message)
    {
        message.SetActive(true);
        while (!Input.GetButtonDown("x (continue)"))
            yield return null;
        message.SetActive(false);
    }

    public void DisplayFindTheBlahMessage(string store_name)
    {
        please_find_the_blah_text.text = LanguageSource.GetLanguageString("please find prompt") + " " + store_name;
        StartCoroutine(DisplayMessage(please_find_the_blah));
    }
}
