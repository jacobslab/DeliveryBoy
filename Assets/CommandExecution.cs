using UnityEngine;
using System.Collections;
using System.Threading;
using System.Diagnostics;
using UnityEngine.UI;
public class CommandExecution : MonoBehaviour {
  //  public Text trialText;
    private int testInt = 0;
	
	//SINGLETON
	private static CommandExecution _instance;
	
	public static CommandExecution Instance{
		get{
			return _instance;
		}
	}
	
	void Awake(){
		
		if (_instance != null) {
			UnityEngine.Debug.Log ("Instance already exists!");
			Destroy (transform.gameObject);
			return;
		}
		_instance = this;
	}

	// Use this for initialization
	void Start () {
      //  trialText.text = testInt.ToString();
    }
	
	// Update is called once per frame
	void Update () {
//        if(Input.GetKeyDown(KeyCode.A))
//        {
//            ExecuteCommand();
//        }
//        if(Input.GetKeyDown(KeyCode.D))
//        {
//            testInt++;
//            trialText.text = testInt.ToString();
//        }
	
	}
    public static void ExecuteCommand()
    {
        var thread = new Thread(delegate () { Command(); });
        thread.Start();
        UnityEngine.Debug.Log("Starting thread");
    }

    static void Command()
    {
//#if (UNITY_STANDALONE || UNITY_EDITOR)
//        var processInfo = new ProcessStartInfo("powershell.exe", @"notepad.exe");
//`#elif (UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX)
	//	var processInfo = new ProcessStartInfo("/Applications/Utilities/Terminal.app/Contents/MacOS/Terminal", @"shutdown -s now");
//#endif
		var processInfo = new ProcessStartInfo ("open","shutdown.app");
		//processInfo.FileName = "/Applications/Utilities/Terminal.app/Contents/MacOS/Terminal";
		//processInfo.Arguments="-c \" " + "shutdown -s now" + " \"";
        processInfo.CreateNoWindow = false;
		processInfo.RedirectStandardOutput = true;
        processInfo.UseShellExecute = false;

        var process = Process.Start(processInfo);

        process.WaitForExit();
        process.Close();
    }
}
