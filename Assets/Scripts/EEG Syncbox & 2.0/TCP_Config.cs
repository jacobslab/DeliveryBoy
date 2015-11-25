using UnityEngine;
using System.Collections;

public class TCP_Config : MonoBehaviour {

	public static float numSecondsBeforeAlignment = 30.0f;

	public static string HostIPAddress = "192.168.137.200"; //"169.254.50.2" for Mac Pro Desktop.
	public static int ConnectionPort = 8888; //8001 for Mac Pro Desktop communication


	public static char MSG_START = '[';
	public static char MSG_SEPARATOR = '~';
	public static char MSG_END = ']';

	public static string ExpName = "TH1";
	public static string SubjectName = ExperimentSettings.currentSubject.name;

	public enum EventType {
		SUBJECTID,
		EXPNAME,
		VERSION,
		INFO,
		CONTROL,
		SESSION,
		PRACTICE,
		TRIAL,
		PHASE,
		DISPLAYON,
		DISPLAYOFF,
		HEARTBEAT,
		ALIGNCLOCK,
		ABORT,
		SYNC,
		SYNCED,
		EXIT
	}

}
