using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InputMic : MonoBehaviour
{

    public static float MicLoudness;
    public Dropdown micDrops;
    private float maxLoud = 0f;
    private string _device;
    public bl_ProgressBar volProg;
    private bool cannotHear = true;
    public Text spokenWord;
    public string[] wordList;
    private int currentWord = 0;
    public Image marker;
    public Text beginExperimentText;
    private List<string> micList = new List<string>();
    //mic initialization
	public CanvasGroup micCanvasGroup;
	void Awake()
	{
		micCanvasGroup.alpha = 0f;
	}
    void Start()
    {
        for (int i = 0; i < Microphone.devices.Length; i++)
        {
            Debug.Log(Microphone.devices[i].ToString());
            micList.Add(Microphone.devices[i].ToString());
        }
        beginExperimentText.enabled = false;
        micDrops.AddOptions(micList);
    }
    void InitMic()
    {

        if (_device == null) _device = Microphone.devices[micDrops.value];
        _clipRecord = Microphone.Start(_device, true, 999, 44100);
    }
    IEnumerator RotateWords()
    {
        spokenWord.text = wordList[0];
        float timer = 0f;
        while (cannotHear)
        {
            timer += Time.deltaTime;
            if (timer > 5f)
            {
                spokenWord.color = Color.red;
                spokenWord.text = "Sorry, I cannot hear you! \n Please adjust microphone";
                yield return new WaitForSeconds(1f);
                spokenWord.color = Color.white;
                timer = 0f;
                currentWord++;
                spokenWord.text = wordList[currentWord];
            }
            if (MicLoudness > 0.5f)
            {
                cannotHear = false;
                marker.color = Color.green;
            }
            yield return 0;
        }
        yield return null;
    }

    public IEnumerator RunMicTest()
    {
		micCanvasGroup.alpha = 1f;
        yield return StartCoroutine("RotateWords");
        yield return new WaitForSeconds(1.5f);
		micCanvasGroup.alpha = 0f;
        yield return null;

    }

    void StopMicrophone()
    {
        Microphone.End(_device);
    }


    AudioClip _clipRecord = new AudioClip();
    int _sampleWindow = 128;

    //get data from microphone into audioclip
    float LevelMax()
    {
        float levelMax = 0;
        float[] waveData = new float[_sampleWindow];
        int micPosition = Microphone.GetPosition(null) - (_sampleWindow + 1); // null means the first microphone
        if (micPosition < 0) return 0;
        _clipRecord.GetData(waveData, micPosition);
        // Getting a peak on the last 128 samples
        for (int i = 0; i < _sampleWindow; i++)
        {
            float wavePeak = waveData[i] * waveData[i];
            if (levelMax < wavePeak)
            {
                levelMax = wavePeak;
            }
        }
        return levelMax;
    }



    void Update()
    {
        // levelMax equals to the highest normalized value power 2, a small number because < 1
        // pass the value to a static var so we can access it from anywhere
        MicLoudness = LevelMax();

        if (maxLoud < MicLoudness)
            maxLoud = MicLoudness;
        if (cannotHear)
            volProg.Value = MicLoudness;
        else {
            beginExperimentText.enabled = true;
            spokenWord.color = Color.green;
            spokenWord.text = "I heard you say " + wordList[currentWord];
        }
    }

    bool _isInitialized;
    // start mic when scene starts
    void OnEnable()
    {
        InitMic();
        _isInitialized = true;
    }

    //stop mic when loading a new level or quit application
    void OnDisable()
    {
        StopMicrophone();
    }

    void OnDestroy()
    {
        StopMicrophone();
    }


    // make sure the mic gets started & stopped when application gets focused
    void OnApplicationFocus(bool focus)
    {
        if (focus)
        {
            //Debug.Log("Focus");

            if (!_isInitialized)
            {
                //Debug.Log("Init Mic");
                InitMic();
                _isInitialized = true;
            }
        }
        if (!focus)
        {
            //Debug.Log("Pause");
            StopMicrophone();
            //Debug.Log("Stop Mic");
            _isInitialized = false;

        }
    }
}