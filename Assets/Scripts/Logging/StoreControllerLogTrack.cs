using UnityEngine;
using System.Collections;

public class StoreControllerLogTrack : LogTrack {


	bool firstLog = false;

	//log on late update so that everything for that frame gets set first
	void LateUpdate () {
		//just log the environment info on the first frame
		if (ExperimentSettings.isLogging && !firstLog) {
			LogBuildingCenterPositions(exp.storeController.stores);
			firstLog = true;
		}
	}

	void LogBuildingCenterPositions(Store[] stores){
		for(int i = 0; i < stores.Length; i++){
			Store currStore = stores[i];
			Transform currStoreCenter = currStore.StoreCenterTransform;
			subjectLog.Log (GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount(), "Store Center Info" + separator + currStore.name + separator +
			                "POSITION" + separator + currStoreCenter.position.x + separator + currStoreCenter.position.y + separator + currStoreCenter.position.z + separator +
			                "ROTATION" + separator + currStoreCenter.rotation.eulerAngles.x + separator + currStoreCenter.rotation.eulerAngles.y + separator + currStoreCenter.rotation.eulerAngles.z + separator +
			                "SCALE" + separator + currStoreCenter.localScale.x + separator + currStoreCenter.localScale.y + separator + currStoreCenter.localScale.z);
		}
	}
		


}