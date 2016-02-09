using UnityEngine;
using System.Collections;

public class StoreControllerLogTrack : LogTrack {


	bool firstLog = false;

	//log on late update so that everything for that frame gets set first
	void LateUpdate () {
		//just log the environment info on the first frame
		if (ExperimentSettings.isLogging && !firstLog) {
			LogBuildingPositions(exp.storeController.stores);
			firstLog = true;
		}
	}

	void LogBuildingPositions(Store[] stores){
		for(int i = 0; i < stores.Length; i++){
			Store currStore = stores[i];
			subjectLog.Log (GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount(), "Store Info" + separator + currStore.name + separator +
			                "POSITION" + separator + currStore.transform.position.x + separator + currStore.transform.position.y + separator + currStore.transform.position.z + separator +
			                "ROTATION" + separator + currStore.transform.rotation.eulerAngles.x + separator + currStore.transform.rotation.eulerAngles.y + separator + currStore.transform.rotation.eulerAngles.z + separator +
			                "SCALE" + separator + currStore.transform.localScale.x + separator + currStore.transform.localScale.y + separator + currStore.transform.localScale.z);
		}
	}
		


}