using UnityEngine;
using System.Collections;
using System.Threading;
using System.Diagnostics;
using UnityEngine.UI;
public class CommandExecution : MonoBehaviour {
    public Text trialText;
    private int testInt = 0;
	// Use this for initialization
	void Start () {
        trialText.text = testInt.ToString();
    }
	
	// Update is called once per frame
	void Update () {

        if(Input.GetKeyDown(KeyCode.A))
        {
            ExecuteCommand();
        }
        if(Input.GetKeyDown(KeyCode.D))
        {
            testInt++;
            trialText.text = testInt.ToString();
        }
	
	}
    public static void ExecuteCommand()
    {
        var thread = new Thread(delegate () { Command(); });
        thread.Start();
        UnityEngine.Debug.Log("Starting thread");
    }

    static void Command()
    {
        var processInfo = new ProcessStartInfo("powershell.exe", @"notepad.exe");
        processInfo.CreateNoWindow = false;
        processInfo.UseShellExecute = false;

        var process = Process.Start(processInfo);

        process.WaitForExit();
        process.Close();
    }
}
