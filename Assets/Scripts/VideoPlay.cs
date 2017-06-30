using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Video;
public class VideoPlay : MonoBehaviour {

    Experiment exp { get { return Experiment.Instance; } }
	public VideoClip hospitalLearningSessionVideo;
	public VideoClip germanHospitalLearningSessionVideo;
	public VideoClip englishVideo;
	public VideoClip germanVideo;
	private VideoClip currentClip;
	private VideoPlayer videoPlayer;
	public AudioClip hospitalEnglishAudio;
	public AudioClip englishAudio;
	public AudioClip hospitalGermanAudio;
	public AudioClip germanAudio;
	private AudioClip currentAudio;
	public bool shouldPlay=false;
	private bool movieSkipped=false;
	private AudioSource audioSource;
	public RawImage image;
    void Awake() {

    }

    // Use this for initialization
    void Start() {
       
    }

    bool isMoviePaused = false;
    void Update() {
		if (currentClip != null && videoPlayer!=null) {
			if (videoPlayer.isPlaying) {
                if (Input.GetAxis("Action Button") > 0.2f) { //skip movie!
					videoPlayer.Stop();
					audioSource.Stop ();
					videoPlayer.enabled=false;
					movieSkipped = true;
                }
                if (TrialController.isPaused) {
					videoPlayer.playbackSpeed = 0f;
					audioSource.Pause ();
                }
            }
            if (!TrialController.isPaused) {
				videoPlayer.playbackSpeed = 1f;
				audioSource.UnPause ();
            }
        }
        else {
//        Debug.Log("No movie attached! Can't update.");
        }
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
				exp.instructionsController.VideoInstructions.transform.GetChild (0).gameObject.GetComponent<RawImage> ().enabled = true;
					//Add VideoPlayer to the GameObject
					videoPlayer = gameObject.AddComponent<VideoPlayer>();

					//Add AudioSource
					audioSource = gameObject.AddComponent<AudioSource>();

					//Disable Play on Awake for both Video and Audio
					videoPlayer.playOnAwake = false;
					audioSource.playOnAwake = false;

					//We want to play from video clip not from url
					videoPlayer.source = VideoSource.VideoClip;

				audioSource.clip = currentAudio;
					//Set Audio Output to AudioSource
				videoPlayer.audioOutputMode = VideoAudioOutputMode.None;

					//Assign the Audio from Video to AudioSource to be played
//					videoPlayer.EnableAudioTrack(0, true);
//					videoPlayer.SetTargetAudioSource(0, audioSource);

					//Set video To Play then prepare Audio to prevent Buffering
				videoPlayer.clip = currentClip;
					videoPlayer.Prepare();

					//Wait until video is prepared
					while (!videoPlayer.isPrepared)
					{
						Debug.Log("Preparing Video");
						yield return null;
					}

					Debug.Log("Done Preparing Video");
//				videoPlayer.renderMode = VideoRenderMode.CameraNearPlane;
//				videoPlayer.targetCamera = Camera.main;
					//Assign the Texture from Video to RawImage to be displayed
					image.texture = videoPlayer.texture;

					//Play Video
					videoPlayer.Play();

					//Play Sound
					audioSource.Play();

					Debug.Log("Playing Video");
					while (videoPlayer.isPlaying)
					{
						Debug.LogWarning("Video Time: " + Mathf.FloorToInt((float)videoPlayer.time));
						yield return null;
					}

					Debug.Log("Done Playing Video");
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
		currentClip = germanHospitalLearningSessionVideo;
			currentAudio=hospitalGermanAudio;
	#else
            //PLAY shorter video with only navigation
			currentClip = hospitalLearningSessionVideo;
			currentAudio=hospitalEnglishAudio;
	#endif
        }
		else
		{
		#if GERMAN
		currentClip=germanVideo;
			currentAudio=germanAudio;
		#else
			currentClip=englishVideo;
			currentAudio=englishAudio;
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
