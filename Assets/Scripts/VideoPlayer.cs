using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class VideoPlayer : MonoBehaviour {

    Experiment exp { get { return Experiment.Instance; } }

    MovieTexture movie;
    AudioSource movieAudio;
    public MovieTexture hospitalLearningSessionVideo;
    public AudioClip hospitalLearningSessionAudio;
    public MovieTexture englishVideo;
    public AudioClip englishAudio;
    public MovieTexture germanVideo;
    public AudioClip germanAudio;
    public CanvasGroup group;
    public bool shouldPlay = false;
    void Awake() {
        group.alpha = 0.0f;
    }

    // Use this for initialization
    void Start() {
        RawImage rim = GetComponent<RawImage>();
        movieAudio = GetComponent<AudioSource>();
        if (rim != null) {
            if (rim.texture != null) {
				#if HOSPITAL
				rim.texture=hospitalLearningSessionVideo;
				movieAudio.clip=hospitalLearningSessionAudio;
				#else
#if GERMAN
                rim.texture=germanVideo;
                movieAudio.clip=germanAudio;
#else
                rim.texture = englishVideo;
                movieAudio.clip = englishAudio;
#endif
				#endif
                movie = (MovieTexture)rim.mainTexture;
			
			}

		}
        //	rim.color = new Color (0f, 0f, 0f, 0f);
    }

    bool isMoviePaused = false;
    void Update() {
        if (movie != null) {
            if (movie.isPlaying) {
                if (Input.GetAxis("Action Button") > 0.2f) { //skip movie!
                    Stop();
                }
                if (TrialController.isPaused) {
                    Pause();
                }
            }
            if (!TrialController.isPaused) {
                if (isMoviePaused) {
                    UnPause();
                }
            }
        }
        //else {
        //Debug.Log("No movie attached! Can't update.");
        //}
    }


	public IEnumerator Play(bool autoPlay) {
        group.alpha = 0.0f;
        if (movie != null) {
            Debug.Log("playing instruction video");
			if (!autoPlay)
				yield return StartCoroutine (exp.instructionsController.AskIfShouldPlay ());
			else
				shouldPlay = true;

            if (shouldPlay) {
                group.alpha = 1.0f;

                movie.Stop();
                movieAudio.Play();
                movie.Play();

                while (movie.isPlaying || isMoviePaused) {
                    yield return 0;
                }

                isMoviePaused = false;

                group.alpha = 0.0f;
            }
            yield return 0;
        }
        else {
            Debug.Log("No movie attached! Can't play.");
        }
    }

    public IEnumerator SetVideoTexture()
    {
        RawImage rim = GetComponent<RawImage>();
        AudioSource movieAudio = GetComponent<AudioSource>();

#if HOSPITAL
        if (ExperimentSettings.Instance.mySessionType == ExperimentSettings.SessionType.learningSession)
        {
#if GERMAN
            //put the german version of hospital learning session video here
#else
            //PLAY shorter video with only navigation
            rim.texture = hospitalLearningSessionVideo;
            movieAudio.clip = hospitalLearningSessionAudio;
#endif
        }
		else
		{
		#if GERMAN
		rim.texture=germanVideo;
		movieAudio.clip=germanAudio;
		#else
			rim.texture=englishVideo;
			movieAudio.clip=englishAudio;
		#endif
		}
#else
        // no need to change; as Start() has already set it properly
#endif

			movie=(MovieTexture)rim.texture;
            yield return null;
    }
	
	void Pause(){
		if(movie != null){
			movie.Pause();
			movieAudio.Pause ();
			isMoviePaused = true;
		} 
		else {
			Debug.Log("No movie attached! Can't pause.");
		}
	}
	
	void UnPause(){
		if(movie != null){
			movie.Play ();
			movieAudio.UnPause ();
			isMoviePaused = false;
		} 
		else {
			Debug.Log("No movie attached! Can't unpause.");
		}
	}
	
	void Stop(){
		if(movie != null){
			isMoviePaused = false;
			movie.Stop ();
		} 
		else {
			Debug.Log("No movie attached! Can't stop.");
		}
	}
	
}
