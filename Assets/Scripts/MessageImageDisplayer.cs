using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MessageImageDisplayer : MonoBehaviour
{
    public GameObject[] practice_phase_messages;
    public GameObject[] final_recall_messages;
    public GameObject[] delivery_restart_messages;
    public GameObject[] sotre_images_presentation_messages;

    public void DisplayLanguageMessage(GameObject[] language_messages)
    {
        StartCoroutine(DisplayMessage(language_messages[(int)LanguageSource.current_language]));
    }

    private IEnumerator DisplayMessage (GameObject message)
    {
        message.SetActive(true);
        while (!Input.GetButtonDown("x"))
            yield return null;
        message.SetActive(false);
    }
}
