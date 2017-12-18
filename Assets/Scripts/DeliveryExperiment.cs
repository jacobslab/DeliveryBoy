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
    private const int deliveries_per_trial = 13;
    private const float min_familiarization_isi = 0.4f;
    private const float max_familiarization_isi = 0.6f;
    private const float familiarization_presentation_length = 1.5f;
    private const float recall_text_display_length = 1f;
    private const float free_recall_length = 30f;
    private const float time_between_free_and_cued_recall = 2f;
    private const float cued_recall_time_per_store = 5f;
    private const float cued_recall_isi = 1f;

    public Camera regularCamera;
    public Camera familiarizationCamera;
    public Familiarizer familiarizer;
    public MessageImageDisplayer messageImageDisplayer;
    public RamulatorInterface ramulatorInterface;
    public PlayerMovement playerMovement;

    public ScriptedEventReporter scriptedEventReporter;
    public GameObject memoryWordCanvas;

    public Environment[] environments;

    private List<string> this_trial_presented_words = new List<string>();

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

        yield return DoIntroductionVideo();
        yield return DoSubjectSessionQuitPrompt(sessionNumber);
        yield return DoMicrophoneTest();

        memoryWordCanvas.SetActive(false);

        yield return DoFamiliarization();

        Environment environment = EnableEnvironment();

        for (int trial_number = 0; trial_number < 12; trial_number++)
        {
            if (useRamulator)
                ramulatorInterface.BeginNewTrial(trial_number);
            yield return DoDelivery(environment, trial_number);

            memoryWordCanvas.SetActive(true);
            yield return DoRecall(trial_number);

            SetRamulatorState("WAITING", true, new Dictionary<string, object>());
            yield return null;
            textDisplayer.DisplayText("proceed to next day prompt", "Press X to proceed to the next delivery day.");
            while (!Input.GetButton("q (secret)") && !Input.GetButton("x (continue)"))
                yield return null;
            SetRamulatorState("WAITING", false, new Dictionary<string, object>());
            textDisplayer.ClearText();
            if (Input.GetButton("q (secret)"))
                break;
            memoryWordCanvas.SetActive(false);
        }

        yield return DoFinalRecall();
    }

    private IEnumerator DoRecall(int trial_number)
    {
        SetRamulatorState("RETRIEVAL", true, new Dictionary<string, object>());
        DisplayTitle("Please recall objects from this delivery day.");

        highBeep.Play();
        scriptedEventReporter.ReportScriptedEvent("Sound played", new Dictionary<string, object>() { { "sound name", "high beep" }, { "sound duration", highBeep.clip.length.ToString() } });

        textDisplayer.DisplayText("display recall text", "*******");
        yield return new WaitForSeconds(recall_text_display_length);
        textDisplayer.ClearText();

        soundRecorder.StartRecording(Mathf.CeilToInt(free_recall_length));
        yield return new WaitForSeconds(free_recall_length);

        string output_directory = UnityEPL.GetDataPath();
        string wavFilePath = System.IO.Path.Combine(output_directory, trial_number.ToString());

        soundRecorder.StopRecording(wavFilePath);
        textDisplayer.ClearText();
        lowBeep.Play();
        scriptedEventReporter.ReportScriptedEvent("Sound played", new Dictionary<string, object>() { { "sound name", "low beep" }, { "sound duration", lowBeep.clip.length.ToString() } });

        ClearTitle();



        SetRamulatorState("RETRIEVAL", false, new Dictionary<string, object>());
    }

    private IEnumerator DoFinalRecall()
    {
        yield return null;
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
        List<StoreComponent> unvisitedStores = new List<StoreComponent>(environment.stores);
        for (int i = 0; i < deliveries_per_trial; i++)
        {
            int random_store_index = Random.Range(0, unvisitedStores.Count);
            StoreComponent nextStore = unvisitedStores[random_store_index];
            unvisitedStores.RemoveAt(random_store_index);
            playerMovement.Freeze();
            yield return messageImageDisplayer.DisplayFindTheBlahMessage(LanguageSource.GetLanguageString(nextStore.storeName));
            playerMovement.Unfreeze();
            while (!nextStore.PlayerInDeliveryZone())
                yield return null;

            if (i != deliveries_per_trial - 1)
            {
                playerMovement.Freeze();
                AudioClip deliveredItem = nextStore.PopItem();
                string deliveredItemName = deliveredItem.name;
                audioPlayback.clip = deliveredItem;
                audioPlayback.Play();
                AppendWordToLst(System.IO.Path.Combine(UnityEPL.GetDataPath(), trialNumber.ToString() + ".lst"), deliveredItemName);
                this_trial_presented_words.Add(deliveredItemName);
                SetRamulatorState("WORD", true, new Dictionary<string, object>() { { "word", deliveredItemName} });
                yield return new WaitForSeconds(deliveredItem.length);
                SetRamulatorState("WORD", false, new Dictionary<string, object>() { { "word", deliveredItemName } });
                playerMovement.Unfreeze();
            }
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

}