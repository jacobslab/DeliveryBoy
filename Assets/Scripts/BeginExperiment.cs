using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BeginExperiment : MonoBehaviour
{
    public UnityEngine.GameObject greyedOutButton;
    public UnityEngine.GameObject beginExperimentButton;
    public UnityEngine.UI.InputField participantCodeInput;
    public UnityEngine.UI.Toggle useRamulatorToggle;
    public UnityEngine.UI.Text beginButtonText;

    private const string scene_name = "MainGame";

    private void Update()
    {
        if (IsValidParticipantName(participantCodeInput.text))
        {
            UnityEPL.ClearParticipants();
            UnityEPL.AddParticipant(participantCodeInput.text);
            UnityEPL.SetExperimentName("Delivery Boy");
            beginExperimentButton.SetActive(true);
            greyedOutButton.SetActive(false);
            int nextSessionNumber = NextSessionNumber();
            UnityEPL.SetSessionNumber(NextSessionNumber());
            beginButtonText.text = LanguageSource.GetLanguageString("begin session") + " " + nextSessionNumber.ToString();
        }
        else
        {
            greyedOutButton.SetActive(true);
            beginExperimentButton.SetActive(false);
        }
    }

    public void DoBeginExperiment()
    {
        if (!IsValidParticipantName(participantCodeInput.text))
            throw new UnityException("You are trying to start the experiment with an invalid participant name!");

        DeliveryExperiment.ConfigureExperiment(useRamulatorToggle.isOn, NextSessionNumber(), participantCodeInput.text);
        SceneManager.LoadScene(scene_name);
    }

    private int NextSessionNumber()
    {
        string dataPath = UnityEPL.GetParticipantFolder();
		System.IO.Directory.CreateDirectory (dataPath);
        string[] sessionFolders = System.IO.Directory.GetDirectories(dataPath);
        int mostRecentSessionNumber = -1;
        foreach (string folder in sessionFolders)
        {
            int thisSessionNumber = -1;
            if (int.TryParse(folder.Substring(folder.LastIndexOf('_')+1), out thisSessionNumber) && thisSessionNumber > mostRecentSessionNumber)
                mostRecentSessionNumber = thisSessionNumber;
        }
        return mostRecentSessionNumber + 1;
    }

    private bool IsValidParticipantName(string name)
    {
        bool isTest = name.Equals("TEST");
        if (isTest)
            return true;
        if (name.Length != 6)
            return false;
        bool isValidRAMName = name[0].Equals('R') && name[1].Equals('1') && char.IsDigit(name[2]) && char.IsDigit(name[3]) && char.IsDigit(name[4]) && char.IsUpper(name[5]);
        bool isValidSCALPName = char.IsUpper(name[0]) && char.IsUpper(name[1]) && char.IsUpper(name[2]) && char.IsDigit(name[3]) && char.IsDigit(name[4]) && char.IsDigit(name[5]);
        return isValidRAMName || isValidSCALPName;
    }
}