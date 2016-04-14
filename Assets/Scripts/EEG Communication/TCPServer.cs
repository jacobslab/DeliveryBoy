//TCP communication based off of http://www.codeproject.com/Articles/10649/An-Introduction-to-Socket-Programming-in-NET-using

using UnityEngine;
using System.Collections;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.IO;

using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;

using System.Threading;

using LitJson;

public class TCPServer : MonoBehaviour {
	Experiment exp { get { return Experiment.Instance; } }


	ThreadedServer myServer;
	public bool isConnected { get { return GetIsConnected(); } }
	public bool canStartGame { get { return GetCanStartGame(); } }



	//int QUEUE_SIZE = 20;  //Blocks if the queue is full


	//SINGLETON
	private static TCPServer _instance;
	
	public static TCPServer Instance{
		get{
			return _instance;
		}
	}
	
	void Awake(){

		if (_instance != null) {
			UnityEngine.Debug.Log("Instance already exists!");
			Destroy(transform.gameObject);
			return;
		}
		_instance = this;

	}

	void Start(){
		if(Config.isSystem2){
			RunServer ();
		}
	}

	//test clock alignment, every x seconds
	IEnumerator AlignClocks(){
		yield return new WaitForSeconds(TCP_Config.numSecondsBeforeAlignment);
		while(true){
			myServer.RequestClockAlignment();
			yield return new WaitForSeconds(TCP_Config.ClockAlignInterval);
		}
	}

	/*//test encoding phase, every x seconds
	IEnumerator SendPhase(bool value){
		yield return new WaitForSeconds(TCP_Config.numSecondsBeforeAlignment);
		while(true){
			myServer.SendStateEvent(GameClock.SystemTime_Milliseconds, "ENCODING", value);
			yield return new WaitForSeconds(10.0f);
		}
	}*/

	void RunServer () {
		myServer = new ThreadedServer ();
		myServer.Start ();
	}

	bool startedAlignClocks = false;
	void Update(){
		if (myServer != null) {
			if (isConnected && !startedAlignClocks) {
				startedAlignClocks = true;
				StartCoroutine (AlignClocks ());
				myServer.SendInitMessages ();
			}
		}
		//DEBUGGING
		/*
		if (Input.GetKeyDown (KeyCode.A)) {
			myServer.isServerConnected = true;
		}
		if (Input.GetKeyDown (KeyCode.S)) {
			myServer.canStartGame = true;
		}
		*/
	}

	public void Log(long time, TCP_Config.EventType eventType){
		exp.eegLog.Log(time, exp.eegLog.GetFrameCount(), eventType.ToString());
	}

	public void SetState(TCP_Config.DefineStates state, bool isEnabled){
		if (myServer != null) {
			if (myServer.isServerConnected) {
				myServer.SendStateEvent (GameClock.SystemTime_Milliseconds, state.ToString (), isEnabled);
				UnityEngine.Debug.Log("SET THE STATE FOR BIO-M FILE: " + state.ToString() + isEnabled.ToString());
			}
		}
	}

	public void SendTrialNum(int trialNum){
		if (myServer != null) {
			if(myServer.isServerConnected){
				myServer.SendSimpleJSONEvent(GameClock.SystemTime_Milliseconds, TCP_Config.EventType.TRIAL, trialNum);
			}
		}
	}

	bool GetIsConnected(){
		return myServer.isServerConnected;
	}

	bool GetCanStartGame(){
		return myServer.canStartGame;
	}

	void OnApplicationQuit(){
		if(myServer != null){
			myServer.End ();
			UnityEngine.Debug.Log ("Ended server.");
		}
	}


}








//THREADED SERVER
public class ThreadedServer : ThreadedJob{
	public bool isRunning = false;

	public bool isServerConnected = false;
	public bool isSynced = false;
	public bool canStartGame = false;
	Stopwatch clockAlignmentStopwatch;
	int numClockAlignmentTries = 0;
	const int timeBetweenClockAlignmentTriesMS = 500;//500; //half a second
	const int maxNumClockAlignmentTries = 120; //for a total of 60 seconds of attempted alignment




	public string messagesToSend = "";
	string incompleteMessage = "";

	Socket s;
	TcpListener myList;

	int socketTimeoutMS = 5; //TODO: what should I set this to?
		
	public ThreadedServer(){
		
	}
	
	protected override void ThreadFunction()
	{
		isRunning = true;
		// Do your threaded task. DON'T use the Unity API here
		while (isRunning) {
			if(!isServerConnected){
				InitControlPC();
			}
			TalkToClient();
		}
		CleanupConnections();
	}
	
	void TalkToClient(){
		try {
			if(!isSynced){
				if(numClockAlignmentTries < maxNumClockAlignmentTries){
					CheckClockAlignment();
				}
				else{
					//TODO: what to do if the clocked never synced?!
				}
			}

			//SEND HEARTBEAT
			SendHeartbeatPolled();

			CheckForMessages();

			SendMessages();

			UnityEngine.Debug.Log("MAIN LOOP EXECUTED");

			
		}
		catch (Exception e) {
			UnityEngine.Debug.Log("Connection Error....." + e.StackTrace);
		}  
	}

	void InitControlPC(){

		//connect
		OpenConnections();

		isServerConnected = true;
	}

	public void SendInitMessages(){
		//define event
		SendDefineEvent (GameClock.SystemTime_Milliseconds, TCP_Config.EventType.DEFINE, TCP_Config.GetDefineList ());
		
		//send name of this experiment
		SendSimpleJSONEvent(GameClock.SystemTime_Milliseconds, TCP_Config.EventType.EXPNAME, TCP_Config.ExpName);
		
		//send exp version
		SendSimpleJSONEvent(GameClock.SystemTime_Milliseconds, TCP_Config.EventType.VERSION, Config.VersionNumber);

		//send exp session
		SendSessionEvent (GameClock.SystemTime_Milliseconds, TCP_Config.EventType.SESSION, Experiment.sessionID, TCP_Config.sessionType);
		SendSimpleJSONEvent(GameClock.SystemTime_Milliseconds, TCP_Config.EventType.VERSION, Config.VersionNumber);
		
		//send subject ID
		SendSimpleJSONEvent(GameClock.SystemTime_Milliseconds, TCP_Config.EventType.SUBJECTID, TCP_Config.SubjectName);
		
		//align clocks //SHOULD THIS BE FINISHED BEFORE WE START SENDING HEARTBEATS? -- NO
		RequestClockAlignment();
		
		//start heartbeat
		StartHeartbeatPoll();
		
		//wait for "STARTED" message to be received
	}

	void OpenConnections(){
		IPAddress ipAd = IPAddress.Parse(TCP_Config.HostIPAddress);

		// use local m/c IP address, and 
		// use the same in the client
		
		/* Initializes the Listener */
		myList = new TcpListener(ipAd,TCP_Config.ConnectionPort);
		
		/* Start Listening at the specified port */        
		myList.Start();
		
		UnityEngine.Debug.Log("The server is running at port" + TCP_Config.ConnectionPort + "...");    
		UnityEngine.Debug.Log("The local End point is  :" + myList.LocalEndpoint );
		UnityEngine.Debug.Log("Waiting for a connection.....");
		
		s = myList.AcceptSocket();
		isServerConnected = true;

		//THIS IS VERY IMPORTANT.
		//WITHOUT THIS, SOCKET WILL HANG ON THINGS LIKE RECEIVING MESSAGES IF THERE ARE NO NEW MESSAGES.
			//...because socket.Receive() is a blocking call.
		s.ReceiveTimeout = socketTimeoutMS;

		UnityEngine.Debug.Log("CONNECTED!");
	}
	
	void CleanupConnections(){
		/* clean up */            
		s.Close();
		myList.Stop();
		isServerConnected = false;
	}

	void CloseServer(){
		try{
			isServerConnected = false;
		}
		catch (Exception e) {
			UnityEngine.Debug.Log("Close Server Error....." + e.StackTrace);
		}  
	}

	//CLOCK ALIGNMENT!
	/*
        Task computer starts the process by sending "ALIGNCLOCK' request.
        Control PC will send a sequence of SYNC messages which are echoed back to it
        When it is complete, the Control PC will send a SYNCED message, which indicates 
        it has completed the clock alignment and it is safe for task computer to proceed 
        to the next step.
		*/
	public void RequestClockAlignment(){

		clockAlignmentStopwatch = new Stopwatch();

		isSynced = false;

		SendSimpleJSONEvent(GameClock.SystemTime_Milliseconds, TCP_Config.EventType.ALIGNCLOCK, "");
		//SendSimpleJSONEvent(0, TCP_Config.EventType.ALIGNCLOCK, "0", ""); //JUST FOR DEBUGGING
		UnityEngine.Debug.Log("REQUESTING ALIGN CLOCK");
        
		clockAlignmentStopwatch.Start();
		numClockAlignmentTries = 0;

	}

	//after x seconds have passed, check if the clocks are aligned yet
	int CheckClockAlignment(){
		if(clockAlignmentStopwatch.ElapsedMilliseconds >= timeBetweenClockAlignmentTriesMS){
			if(isSynced){
				UnityEngine.Debug.Log("Sync Complete");
				clockAlignmentStopwatch.Reset();
				return 0;
			}
			else{ //if not synced yet, wait another .5 seconds
				numClockAlignmentTries++;
				clockAlignmentStopwatch.Reset();
				clockAlignmentStopwatch.Start();
				return -1;
			}
		}
		return -1;
	}





	//MESSAGE SENDING AND RECEIVING

	//send all "messages to send"
	void SendMessages(){
		if(messagesToSend != ""){
			string messagesToSendCopy = messagesToSend;
			UnityEngine.Debug.Log("SENDING MESSAGE: " + messagesToSendCopy);
			SendMessage(messagesToSendCopy);
			if(messagesToSend == messagesToSendCopy){
				messagesToSend = "";
			}
			else{
				UnityEngine.Debug.Log("CLEARED SENT PART OF MESSAGES TO SEND");
				messagesToSend = messagesToSend.Substring(messagesToSendCopy.Length);
			}
		}
	}

	//send a single message. don't call this on it's own.
	//should use other methods (EchoMessage, SendEvent, etc.) to add messages to "messagesToSend"
	void SendMessage(string message){
		try{
			ASCIIEncoding asen=new ASCIIEncoding();
			s.Send(asen.GetBytes(message));
			UnityEngine.Debug.Log("\nSent Message: " + message);
		}
		catch (Exception e) {
			UnityEngine.Debug.Log("Send Message Error....." + e.StackTrace);
		}
	}

	void EchoMessage(string message){
		messagesToSend += ("ECHO: " + message);
	}

	public string SendSimpleJSONEvent(long systemTime, TCP_Config.EventType eventType, string eventData){
		
		string jsonEventString = JsonMessageController.FormatSimpleJSONEvent (systemTime, eventType.ToString(), eventData);
		
		UnityEngine.Debug.Log (jsonEventString);
		
		messagesToSend += jsonEventString;
		
		return jsonEventString;
	}

	public string SendSimpleJSONEvent(long systemTime, TCP_Config.EventType eventType, long eventData){
		
		string jsonEventString = JsonMessageController.FormatSimpleJSONEvent (systemTime, eventType.ToString(), eventData);
		
		UnityEngine.Debug.Log (jsonEventString);
		
		messagesToSend += jsonEventString;
		
		return jsonEventString;
	}
	
	public string SendSessionEvent(long systemTime, TCP_Config.EventType eventType, int sessionNum, TCP_Config.SessionType sessionType){
		
		string jsonEventString = JsonMessageController.FormatJSONSessionEvent (systemTime, sessionNum, sessionType.ToString());
		
		UnityEngine.Debug.Log (jsonEventString);
		
		messagesToSend += jsonEventString;

		return jsonEventString;
	}
	
	public string SendDefineEvent(long systemTime, TCP_Config.EventType eventType, List<string> stateList){
		
		string jsonEventString = JsonMessageController.FormatJSONDefineEvent (systemTime, stateList);
		
		UnityEngine.Debug.Log (jsonEventString);
		
		messagesToSend += jsonEventString;
		
		return jsonEventString;
	}
	
	public string SendStateEvent(long systemTime, string stateName, bool value){
		
		string jsonEventString = JsonMessageController.FormatJSONStateEvent (systemTime, stateName, value);
		
		UnityEngine.Debug.Log (jsonEventString);
		
		messagesToSend += jsonEventString;
		
		return jsonEventString;
	}

	void CheckForMessages(){
		String message = ReceiveMessageBuffer();
		
		ProcessJSONMessageBuffer(message);
	}

	String ReceiveMessageBuffer(){
		String messageBuffer = "";
		try{

			byte[] b=new byte[1000];

			int k=s.Receive(b);
			UnityEngine.Debug.Log("Recieved something!");
			if(k > 0){

				for (int i=0; i<k; i++) {
					messageBuffer += Convert.ToChar(b[i]);
				}
			}
			UnityEngine.Debug.Log (messageBuffer);
		}

		catch (Exception e) {
			UnityEngine.Debug.Log("Receive Message Error....." + e.StackTrace);
		}

		return messageBuffer;
	}

	//CURRENTLY ASSUMING MESSAGES AREN'T GETTING SPLIT IN HALF.
	public void ProcessJSONMessageBuffer(string messageBuffer){
		
		if (messageBuffer != "") {
			
			char[] individualCharacters = messageBuffer.ToCharArray();
			
			int numOpenCharacter = 0;
			int numCloseCharacter = 0;
			string message = "";
			for(int i = 0; i <individualCharacters.Length; i++){
				if(incompleteMessage != ""){
					numOpenCharacter = incompleteMessage.Split(TCP_Config.MSG_START).Length - 1;
					numCloseCharacter = incompleteMessage.Split(TCP_Config.MSG_END).Length - 1;
				}
				
				if(individualCharacters[i] == TCP_Config.MSG_START){
					numOpenCharacter++;
				}
				else if(individualCharacters[i] == TCP_Config.MSG_END && numOpenCharacter > numCloseCharacter){ //close character should never come before open character(s)
					numCloseCharacter++;
				}
				
				message += individualCharacters[i].ToString();
				
				if(numOpenCharacter == numCloseCharacter && numOpenCharacter > 0){ //END OF MESSAGE!
					UnityEngine.Debug.Log("DECODE MESSAGE: " + message);
					DecodeJSONMessage(message);
					
					//reset variables
					message = "";
					
					numOpenCharacter = 0;
					numCloseCharacter = 0;
				}
				//if we're on the last character and num open != num close, we have an incomplete message!
				else if (i == individualCharacters.Length - 1 && numOpenCharacter > 0){
					incompleteMessage = message;
					UnityEngine.Debug.Log("INCOMPLETE MESSAGE: " + incompleteMessage);
				}
				
			}
		}
	}


	public void DecodeJSONMessage(string jsonMessage){

		string dataContent = "";
		int dataContentInt = 0;
		string typeContent = "";

		JsonData messageData = JsonMapper.ToObject(jsonMessage);

		typeContent = (string)messageData ["type"];

		switch ( typeContent ){
		case "SUBJECTID":
			//do nothing
			break;

		case "EXPNAME":
			//do nothing
			break;

		case "VERSION":
			//do nothing
			break;

		case "MESSAGE":
			dataContent = (string)messageData["data"];
			if(dataContent == "STARTED"){
				canStartGame = true;
			}
			break;

		case "SESSION":
			break;

		case "TRIAL":
			dataContentInt = (int)messageData["data"];
			break;

		case "DEFINE":
			break;

		case "STATE":
			break;

		case "HEARTBEAT":
			//do nothing
			break;

		case "ALIGNCLOCK":
			//do nothing
			break;

		case "ABORT":
			//TODO: show message
			Application.Quit();
			break;
			
		case "SYNC":
			//Sync received from Control PC
			//Echo SYNC back to Control PC with high precision time so that clocks can be aligned
			SendSimpleJSONEvent(GameClock.SystemTime_Milliseconds, TCP_Config.EventType.SYNC, GameClock.SystemTime_Microseconds);
			break;
			
		case "SYNCED":
			//Control PC is done with clock alignment
			isSynced = true;
			//now align the neuroport if we've received the start message
			if(canStartGame){
				SendSimpleJSONEvent (GameClock.SystemTime_Milliseconds, TCP_Config.EventType.SYNCNP, "");
			}
			break;
			
		case "EXIT":
			//Control PC is exiting. If heartbeat is active, this is a premature abort.
			
			/*
					if self.isHeartbeat and self.abortCallback:
	                    self.disconnect()
	                    self.abortCallback(self.clock)
					*/
			
			if(isHeartbeat){
				//TODO: do this. am I supposed to check for a premature abort? does it matter? or just end it?
				End ();
			}
			//TODO: show message
			Application.Quit();
			break;
			
		default:
			break;
		}

	}


	//HEARTBEAT
	bool isHeartbeat = false;
	bool hasSentFirstHeartbeat = false;
	long firstBeat = 0;
	long nextBeat = 0;
	long lastBeat = 0;
	long intervalMS = 1000;
	long delta = 0; //is this ever used?

	void StartHeartbeatPoll(){
		isHeartbeat = true;
		hasSentFirstHeartbeat = false;
	}

	void StopHeartbeatPoll(){
		isHeartbeat = false;
	}

	void SendHeartbeatPolled(){
		//Send continuous heartbeat events every 'intervalMillis'
		//The computation assures that the average interval between heartbeats will be intervalMillis rather...
		//...than intervalMillis + some amount of computational overhead because it is relative to a fixed t0.

		if(hasSentFirstHeartbeat){
			long t1 = GameClock.SystemTime_Milliseconds;
			if ((t1 - firstBeat) > nextBeat ){
				UnityEngine.Debug.Log("HI HEARTBEAT");
				nextBeat = nextBeat + intervalMS;
				delta = t1 - lastBeat;
				lastBeat = t1;
				SendSimpleJSONEvent(lastBeat, TCP_Config.EventType.HEARTBEAT, intervalMS.ToString());
			}
		}
		else {
			UnityEngine.Debug.Log("HI FIRST HEARTBEAT");
			firstBeat = GameClock.SystemTime_Milliseconds;
			lastBeat = firstBeat;
			nextBeat = intervalMS;
			SendSimpleJSONEvent(lastBeat, TCP_Config.EventType.HEARTBEAT, intervalMS.ToString());
			hasSentFirstHeartbeat = true;
		}
	}



	//FINISHING/ENDING THE THREAD
	protected override void OnFinished()
	{
		// This is executed by the Unity main thread when the job is finished

	}

	public void End(){
		if (isServerConnected) {
			CloseServer ();
		}
		isRunning = false;
	}
}
