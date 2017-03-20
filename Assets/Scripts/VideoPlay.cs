using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Video;
public class VideoPlay : MonoBehaviour {

    Experiment exp { get { return Experiment.Instance; } }
	public VideoClip hospitalLearningSessionVideo;
	public VideoClip englishVideo;
	public VideoClip germanVideo;
	private VideoClip currentClip;
	private VideoPlayer vidPlayer;
	public bool shouldPlay=false;
	private bool movieSkipped=false;
    void Awake() {

		vidPlayer = GetComponent<VideoPlayer> ();
    }

    // Use this for initialization
    void Start() {
       
    }

    bool isMoviePaused = false;
    void Update() {
		if (currentClip != null) {
			if (vidPlayer.isPlaying) {
                if (Input.GetAxis("Action Button") > 0.2f) { //skip movie!
//					vidPlayer.Stop();
					vidPlayer.enabled=false;
					movieSkipped = true;
//                    Stop();
                }
                if (TrialController.isPaused) {
					vidPlayer.playbackSpeed = 0f;
                }
            }
            if (!TrialController.isPaused) {
				vidPlayer.playbackSpeed = 1f;
            }
        }
        //else {
        //Debug.Log("No movie attached! Can't update.");
        //}
    }


	public IEnumerator Play(bool autoPlay) {
//        group.alpha = 0.0f;
		if (currentClip!=null) {
            Debug.Log("playing instruction video");
			if (!autoPlay)
				yield return StartCoroutine (exp.instructionsController.AskIfShouldPlay ());
			else
				shouldPlay = true;

            if (shouldPlay) {
//                group.alpha = 1.0f;
				Debug.Log("playing video now");
//				vidPlayer.Stop();
//                movieAudio.Play();
				Debug.Log("current clip is: " + vidPlayer.clip);
				vidPlayer.enabled = true;
				Debug.Log (vidPlayer.isPlaying);
				float timer = 0f;
				while (timer<currentClip.length && !movieSkipped) {
					timer += Time.deltaTime;
                    yield return 0;
                }
				movieSkipped = false;
				Debug.Log ("finished playing");
//                isMoviePaused = false;
//				vidPlayer.enabled=false;
//                group.alpha = 0.0f;
            }
            yield return 0;
        }
        else {
            Debug.Log("No movie attached! Can't play.");
        }
    }

    public IEnumerator SetVideoTexture()
    {
//        RawImage rim = GetComponent<RawImage>();
//        AudioSource movieAudio = GetComponent<AudioSource>();

#if HOSPITAL
        if (ExperimentSettings.Instance.mySessionType == ExperimentSettings.SessionType.learningSession)
        {
	#if GERMAN
            //put the german version of hospital learning session video here
	#else
            //PLAY shorter video with only navigation
			currentClip = hospitalLearningSessionVideo;
	#endif
        }
		else
		{
		#if GERMAN
		currentClip=germanVideo;
		#else
			currentClip=englishVideo;
		#endif
		}
#else
        // no need to change; as Start() has already set it properly
		#if GERMAN
		currentClip=germanVideo;
		#else
		currentClip=englishVideo;
		#endif
#endif
		if (currentClip != null)
			vidPlayer.clip = currentClip;
		yield return null;
    }
//	
//	void Pause(){
//		if(movie != null){
//			movie.Pause();
//			movieAudio.Pause ();
//			isMoviePaused = true;
//		} 
//		else {
//			Debug.Log("No movie attached! Can't pause.");
//		}
//	}
//	
//	void UnPause(){
//		if(movie != null){
//			movie.Play ();
//			movieAudio.UnPause ();
//			isMoviePaused = false;
//		} 
//		else {
//			Debug.Log("No movie attached! Can't unpause.");
//		}
//	}
//	
//	void Stop(){
//		if(movie != null){
//			isMoviePaused = false;
//			movie.Stop ();
//		} 
//		else {
//			Debug.Log("No movie attached! Can't stop.");
//		}
//	}
	
}
