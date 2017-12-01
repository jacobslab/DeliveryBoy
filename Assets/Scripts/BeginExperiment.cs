using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BeginExperiment : MonoBehaviour
{
    public UnityEngine.UI.Toggle useRamulatorToggle;

    private const string scene_name = "MainGame";

    public void DoBeginExperiment()
    {
        DeliveryExperiment.ConfigureExperiment(useRamulatorToggle.isOn, 1);
        SceneManager.LoadScene(scene_name);
    }
}