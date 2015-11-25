using UnityEngine;
using System.Collections;

//a parent class for all log track classes
public abstract class LogTrack : MonoBehaviour {
	public Experiment exp { get { return Experiment.Instance; } }
	public Logger_Threading subjectLog { get { return Experiment.Instance.subjectLog; } }
	public Logger_Threading eegLog { get { return Experiment.Instance.eegLog; } }
	public string separator { get { return Logger_Threading.LogTextSeparator; } }

}
