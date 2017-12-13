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

    public MessageImageDisplayer messageImageDisplayer;
    public RamulatorInterface ramulatorInterface;
    public PlayerMovement playerMovement;

    public ScriptedEventReporter scriptedEventReporter;
    public GameObject memoryWordCanvas;

    public Environment[] environments;

    private List<string> this_trial_presented_words = new List<string>();

    public static void ConfigureExperiment(bool newUseRamulator, int newSessionNumber, string participantCode)
    {
        UnityEPL.AddParticipant(participantCode);
        UnityEPL.SetSessionNumber(newSessionNumber);
        UnityEPL.SetExperimentName("Delivery Boy");
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

        Environment environment = EnableEnvironment();

        yield return DoDelivery(environment, 0);
    }

    private IEnumerator DoDelivery(Environment environment, int trialNumber)
    {
        List<StoreComponent> unvisitedStores = new List<StoreComponent>(environment.stores);
        for (int i = 0; i < deliveries_per_trial; i++)
        {
            int random_store_index = Random.Range(0, unvisitedStores.Count);
            StoreComponent nextStore = unvisitedStores[random_store_index];
            unvisitedStores.RemoveAt(random_store_index);
            messageImageDisplayer.DisplayFindTheBlahMessage(LanguageSource.GetLanguageString(nextStore.storeName));
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
                yield return new WaitForSeconds(deliveredItem.length);
                playerMovement.Unfreeze();
            }
        }
    }

    private void AppendWordToLst(string lstFilePath, string word)
    {
        System.IO.FileInfo lstFile = new System.IO.FileInfo(lstFilePath);
        bool firstLine = lstFile.Exists;
        if (firstLine)
            lstFile.Directory.Create();
        lstFile.Directory.Create();
        using (var stream = System.IO.File.OpenWrite(lstFilePath))
        {
            using (System.IO.StreamWriter writer = new System.IO.StreamWriter(stream))
            {
                if (!firstLine)
                    writer.Write("\n");
                writer.Write(word);
            }
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