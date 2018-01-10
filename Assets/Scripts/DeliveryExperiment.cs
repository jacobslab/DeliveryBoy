using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct Environment
{
    public GameObject parent;
    public StoreComponent[] stores;
}

public class DeliveryExperiment : CoroutineExperiment
{
    public delegate void StateChange(string stateName, bool on);
    public static StateChange OnStateChange;

    private static int sessionNumber = -1;
    private static bool useRamulator;

    private const string dboy_version = "v4.0";
    private const string recall_text = "*******";
    private const int deliveries_per_trial = 13;
    private const float min_familiarization_isi = 0.4f;
    private const float max_familiarization_isi = 0.6f;
    private const float familiarization_presentation_length = 1.5f;
    private const float recall_text_display_length = 1f;
    private const float free_recall_length = 30f;
    private const float store_recall_length = 90f;
    private const float final_recall_length = 300f;
    private const float time_between_different_recall_phases = 2f;
    private const float cued_recall_time_per_store = 5f;
    private const float cued_recall_isi = 1f;
    private const float arrow_correction_time = 3f;

    public Camera regularCamera;
    public Camera familiarizationCamera;
    public Familiarizer familiarizer;
    public MessageImageDisplayer messageImageDisplayer;
    public RamulatorInterface ramulatorInterface;
    public PlayerMovement playerMovement;
    public GameObject pointer;
    public ParticleSystem pointerParticleSystem;
    public GameObject pointerMessage;
    public UnityEngine.UI.Text pointerText;

    public float pointerRotationSpeed = 10f;

    public ScriptedEventReporter scriptedEventReporter;
    public GameObject memoryWordCanvas;

    public Environment[] environments;

    private List<StoreComponent> this_trial_presented_stores = new List<StoreComponent>();
    private List<string> all_presented_objects = new List<string>();

    public static void ConfigureExperiment(bool newUseRamulator, int newSessionNumber, string participantCode)
    {
        useRamulator = newUseRamulator;
        sessionNumber = newSessionNumber;
    }

	void Start ()
    {
        StartCoroutine(ExperimentCoroutine());
	}
	
	private IEnumerator ExperimentCoroutine()
    {
        if (sessionNumber == -1)
        {
            throw new UnityException("Please call ConfigureExperiment before beginning the experiment.");
        }

        //write versions to logfile
        Dictionary<string, object> versionsData = new Dictionary<string, object>();
        versionsData.Add("UnityEPL version", Application.version);
        versionsData.Add("Experiment version", dboy_version);
        versionsData.Add("Logfile version", "1");
        scriptedEventReporter.ReportScriptedEvent("versions", versionsData, 0);

        if (useRamulator)
            yield return ramulatorInterface.BeginNewSession(sessionNumber);

        yield return DoIntroductionVideo(LanguageSource.GetLanguageString("play movie"), LanguageSource.GetLanguageString("first day"));
        yield return DoSubjectSessionQuitPrompt(sessionNumber,
                                                LanguageSource.GetLanguageString("running participant"));
        yield return DoMicrophoneTest(LanguageSource.GetLanguageString("after the beep"),
                                      LanguageSource.GetLanguageString("recording"),
                                      LanguageSource.GetLanguageString("playing"),
                                      LanguageSource.GetLanguageString("recording confirmation"));

        memoryWordCanvas.SetActive(false);

        yield return DoFamiliarization();

        Environment environment = EnableEnvironment();

        int trial_number = 0;
        for (trial_number = 0; trial_number < 12; trial_number++)
        {
            if (useRamulator)
                ramulatorInterface.BeginNewTrial(trial_number);
            yield return DoDelivery(environment, trial_number);

            memoryWordCanvas.SetActive(true);
            regularCamera.enabled = false;
            familiarizationCamera.enabled = true;
            yield return DoRecall(trial_number);

            SetRamulatorState("WAITING", true, new Dictionary<string, object>());
            yield return null;
            textDisplayer.DisplayText("proceed to next day prompt", LanguageSource.GetLanguageString("next day"));

            while (!Input.GetButton("q (secret)") && !Input.GetButton("x (continue)"))
                yield return null;
            SetRamulatorState("WAITING", false, new Dictionary<string, object>());
            regularCamera.enabled = true;
            familiarizationCamera.enabled = false;
            textDisplayer.ClearText();
            if (Input.GetButton("q (secret)"))
                break;
            memoryWordCanvas.SetActive(false);
        }

        yield return DoFinalRecall(environment);

        int delivered_objects = trial_number == 12 ? (trial_number) * 12 : (trial_number + 1) * 12;
        textDisplayer.DisplayText("end text", LanguageSource.GetLanguageString("end message") + delivered_objects.ToString() );
    }

    private IEnumerator DoRecall(int trial_number)
    {
        SetRamulatorState("RETRIEVAL", true, new Dictionary<string, object>());
        DisplayTitle("Please recall objects from this delivery day.");

        highBeep.Play();
        scriptedEventReporter.ReportScriptedEvent("Sound played", new Dictionary<string, object>() { { "sound name", "high beep" }, { "sound duration", highBeep.clip.length.ToString() } });

        textDisplayer.DisplayText("display recall text", recall_text);
        yield return SkippableWait(recall_text_display_length);
        textDisplayer.ClearText();

        soundRecorder.StartRecording(Mathf.CeilToInt(free_recall_length));
        yield return SkippableWait(free_recall_length);

        string output_directory = UnityEPL.GetDataPath();
        string wavFilePath = System.IO.Path.Combine(output_directory, trial_number.ToString());

        soundRecorder.StopRecording(wavFilePath);
        textDisplayer.ClearText();
        lowBeep.Play();
        scriptedEventReporter.ReportScriptedEvent("Sound played", new Dictionary<string, object>() { { "sound name", "low beep" }, { "sound duration", lowBeep.clip.length.ToString() } });

        ClearTitle();



        this_trial_presented_stores.Shuffle();
        yield return SkippableWait(time_between_different_recall_phases);
        DisplayTitle("Which object did you deliver to this store?");
        foreach (StoreComponent cueStore in this_trial_presented_stores)
        {
            highBeep.Play();
            scriptedEventReporter.ReportScriptedEvent("Sound played", new Dictionary<string, object>() { { "sound name", "high beep" }, { "sound duration", highBeep.clip.length.ToString() } });

            textDisplayer.DisplayText("display recall text", recall_text);
            yield return SkippableWait(recall_text_display_length);
            textDisplayer.ClearText();

            cueStore.familiarization_object.SetActive(true);
            soundRecorder.StartRecording(Mathf.CeilToInt(cued_recall_time_per_store));
            yield return SkippableWait(cued_recall_time_per_store);
            cueStore.familiarization_object.SetActive(false);

            string output_file_name = trial_number.ToString() + "-" + cueStore.storeName;
            wavFilePath = System.IO.Path.Combine(output_directory, output_file_name);
            string lstFilepath = System.IO.Path.Combine(output_directory, output_file_name) + ".lst";
            soundRecorder.StopRecording(wavFilePath);
            AppendWordToLst(lstFilepath, cueStore.GetLastPoppedItemName());

            textDisplayer.ClearText();
            lowBeep.Play();
            scriptedEventReporter.ReportScriptedEvent("Sound played", new Dictionary<string, object>() { { "sound name", "low beep" }, { "sound duration", lowBeep.clip.length.ToString() } });
            yield return SkippableWait(cued_recall_isi);
        }
        ClearTitle();
        SetRamulatorState("RETRIEVAL", false, new Dictionary<string, object>());


    }

    private IEnumerator DoFinalRecall(Environment environment)
    {
        SetRamulatorState("RETRIEVAL", true, new Dictionary<string, object>());
        regularCamera.enabled = false;
        familiarizationCamera.enabled = true;

        DisplayTitle(LanguageSource.GetLanguageString("all stores recall"));

        highBeep.Play();
        scriptedEventReporter.ReportScriptedEvent("Sound played", new Dictionary<string, object>() { { "sound name", "high beep" }, { "sound duration", highBeep.clip.length.ToString() } });
        textDisplayer.DisplayText("display recall text", recall_text);
        yield return SkippableWait(recall_text_display_length);
        textDisplayer.ClearText();
        soundRecorder.StartRecording(Mathf.CeilToInt(store_recall_length));
        yield return SkippableWait(store_recall_length);
        string output_directory = UnityEPL.GetDataPath();
        string output_file_name = "store recall";
        string wavFilePath = System.IO.Path.Combine(output_directory, output_file_name);
        string lstFilepath = System.IO.Path.Combine(output_directory, output_file_name) + ".lst";
        soundRecorder.StopRecording(wavFilePath);
        foreach (StoreComponent store in environment.stores)
            AppendWordToLst(lstFilepath, store.storeName);
        textDisplayer.ClearText();
        lowBeep.Play();
        scriptedEventReporter.ReportScriptedEvent("Sound played", new Dictionary<string, object>() { { "sound name", "low beep" }, { "sound duration", lowBeep.clip.length.ToString() } });

        ClearTitle();

        yield return SkippableWait(time_between_different_recall_phases);

        DisplayTitle("all objects recall");

        highBeep.Play();
        scriptedEventReporter.ReportScriptedEvent("Sound played", new Dictionary<string, object>() { { "sound name", "high beep" }, { "sound duration", highBeep.clip.length.ToString() } });
        textDisplayer.DisplayText("display recall text", recall_text);
        yield return SkippableWait(recall_text_display_length);
        textDisplayer.ClearText();
        soundRecorder.StartRecording(Mathf.CeilToInt(final_recall_length));
        yield return SkippableWait(final_recall_length);
        output_file_name = "final recall";
        wavFilePath = System.IO.Path.Combine(output_directory, output_file_name);
        lstFilepath = System.IO.Path.Combine(output_directory, output_file_name) + ".lst";
        soundRecorder.StopRecording(wavFilePath);
        foreach (string deliveredObject in all_presented_objects)
            AppendWordToLst(lstFilepath, deliveredObject);
        textDisplayer.ClearText();
        lowBeep.Play();
        scriptedEventReporter.ReportScriptedEvent("Sound played", new Dictionary<string, object>() { { "sound name", "low beep" }, { "sound duration", lowBeep.clip.length.ToString() } });

        ClearTitle();
        SetRamulatorState("RETRIEVAL", false, new Dictionary<string, object>());
    }

    private IEnumerator DoFamiliarization()
    {
        regularCamera.enabled = false;
        familiarizationCamera.enabled = true;

        yield return messageImageDisplayer.DisplayLanguageMessage(messageImageDisplayer.store_images_presentation_messages);
        yield return familiarizer.DoFamiliarization(min_familiarization_isi, max_familiarization_isi, familiarization_presentation_length);

        regularCamera.enabled = true;
        familiarizationCamera.enabled = false;
    }

    private IEnumerator DoDelivery(Environment environment, int trialNumber)
    {
        messageImageDisplayer.please_find_the_blah_reminder.SetActive(true);

        this_trial_presented_stores = new List<StoreComponent>();
        List<StoreComponent> unvisitedStores = new List<StoreComponent>(environment.stores);
        for (int i = 0; i < deliveries_per_trial; i++)
        {
            StoreComponent nextStore = null;
            int random_store_index = -1;
            int tries = 0;
            do
            {
                Debug.Log(tries.ToString());
                tries++;
                random_store_index = Random.Range(0, unvisitedStores.Count);
                nextStore = unvisitedStores[random_store_index];
            }
            while (nextStore.IsVisible() && tries < 17);
            unvisitedStores.RemoveAt(random_store_index);


            playerMovement.Freeze();
            yield return messageImageDisplayer.DisplayFindTheBlahMessage(LanguageSource.GetLanguageString(nextStore.storeName));
            messageImageDisplayer.please_find_the_blah_reminder.SetActive(false);
            yield return DoPointingTask(nextStore);
            messageImageDisplayer.please_find_the_blah_reminder.SetActive(true);
            playerMovement.Unfreeze();

            while (!nextStore.PlayerInDeliveryPosition())
            {
                Debug.Log(nextStore.IsVisible());
                yield return null;
            }
            
            if (i != deliveries_per_trial - 1)
            {
                playerMovement.Freeze();
                AudioClip deliveredItem = nextStore.PopItem();
                string deliveredItemName = deliveredItem.name;
                audioPlayback.clip = deliveredItem;
                audioPlayback.Play();
                AppendWordToLst(System.IO.Path.Combine(UnityEPL.GetDataPath(), trialNumber.ToString() + ".lst"), deliveredItemName);
                this_trial_presented_stores.Add(nextStore);
                all_presented_objects.Add(deliveredItemName);
                SetRamulatorState("WORD", true, new Dictionary<string, object>() { { "word", deliveredItemName} });
                yield return SkippableWait(deliveredItem.length);
                SetRamulatorState("WORD", false, new Dictionary<string, object>() { { "word", deliveredItemName } });
                playerMovement.Unfreeze();
            }
        }

        messageImageDisplayer.please_find_the_blah_reminder.SetActive(false);
    }

    private IEnumerator DoPointingTask(StoreComponent nextStore)
    {
        pointer.SetActive(true);
        pointerMessage.SetActive(true);
        pointerText.text = LanguageSource.GetLanguageString("please point") + LanguageSource.GetLanguageString(nextStore.storeName) + ".";
        yield return null;
        while (!Input.GetButtonDown("x (continue)"))
        {
            pointer.transform.eulerAngles = pointer.transform.eulerAngles + new Vector3(0, Input.GetAxis("Horizontal") * Time.deltaTime * pointerRotationSpeed, 0);
            yield return null;
        }

        float pointerError = PointerError(nextStore.gameObject);
        if (pointerError < Mathf.PI / 12)
        {
            pointerParticleSystem.Play();
            pointerText.text = LanguageSource.GetLanguageString("correct to within") + Mathf.RoundToInt(pointerError * Mathf.Rad2Deg).ToString();
        }
        else
        {
            pointerText.text = LanguageSource.GetLanguageString("wrong by") + Mathf.RoundToInt(pointerError * Mathf.Rad2Deg).ToString();
        }
        yield return null;
        yield return PointArrowToStore(nextStore.gameObject);
        while (!Input.GetButtonDown("x (continue)"))
        {
            yield return null;
        }
        pointerParticleSystem.Stop();
        pointer.SetActive(false);
        pointerMessage.SetActive(false);
    }

    private float PointerError(GameObject toStore)
    {
        Vector3 lookDirection = toStore.transform.position - pointer.transform.position;
        float correctYRotation = Quaternion.LookRotation(lookDirection).eulerAngles.y;
        float actualYRotation = pointer.transform.eulerAngles.y;
        float offByRads = Mathf.Abs(correctYRotation - actualYRotation) * Mathf.Deg2Rad;
        if (offByRads > Mathf.PI)
            offByRads = Mathf.PI * 2 - offByRads;
        return offByRads;
    }

    private IEnumerator PointArrowToStore(GameObject pointToStore)
    {
        float rotationSpeed = 1f;
        float startTime = Time.time;
        Vector3 lookDirection = pointToStore.transform.position - pointer.transform.position;
        while (Time.time < startTime + arrow_correction_time)
        {
            pointer.transform.rotation = Quaternion.Slerp(pointer.transform.rotation, Quaternion.LookRotation(lookDirection), Time.deltaTime * rotationSpeed);
            yield return null;
        }
    }

    private void AppendWordToLst(string lstFilePath, string word)
    {
        System.IO.FileInfo lstFile = new System.IO.FileInfo(lstFilePath);
        bool firstLine = !lstFile.Exists;
        if (firstLine)
            lstFile.Directory.Create();
        lstFile.Directory.Create();
        using (System.IO.StreamWriter w = System.IO.File.AppendText(lstFilePath))
        {
            if (!firstLine)
                w.Write(System.Environment.NewLine);
            w.Write(word);
        }
    }

    private Environment EnableEnvironment()
    {
        System.Random reliable_random = new System.Random(UnityEPL.GetParticipants()[0].GetHashCode());
        Environment environment = environments[reliable_random.Next(environments.Length)];
        environment.parent.SetActive(true);
        return environment;
    }

    //WAITING, INSTRUCT, COUNTDOWN, ENCODING, WORD, DISTRACT, RETRIEVAL
    protected override void SetRamulatorState(string stateName, bool state, Dictionary<string, object> extraData)
    {
        if (OnStateChange != null)
            OnStateChange(stateName, state);
        if (useRamulator)
            ramulatorInterface.SetState(stateName, state, extraData);
    }

    private IEnumerator SkippableWait(float waitTime)
    {
        float startTime = Time.time;
        while (Time.time < startTime + waitTime)
        {
            if (Input.GetButtonDown("q (secret)"))
                break;
            yield return null;
        }
    }

}

public static class IListExtensions
{
    /// <summary>
    /// Shuffles the element order of the specified list.
    /// </summary>
    public static void Shuffle<T>(this IList<T> ts)
    {
        var count = ts.Count;
        var last = count - 1;
        for (var i = 0; i < last; ++i)
        {
            var r = UnityEngine.Random.Range(i, count);
            var tmp = ts[i];
            ts[i] = ts[r];
            ts[r] = tmp;
        }
    }
}
