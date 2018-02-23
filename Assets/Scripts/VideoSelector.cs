using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VideoSelector : MonoBehaviour
{
    public UnityEngine.Video.VideoPlayer videoPlayer;
    public UnityEngine.Video.VideoClip englishClip;
    public UnityEngine.Video.VideoClip germanClip;

    void OnEnable()
    {
        if (LanguageSource.current_language == LanguageSource.LANGUAGE.GERMAN)
            videoPlayer.clip = germanClip;
        else
            videoPlayer.clip = englishClip;

        videoPlayer.Play();
    }

}
