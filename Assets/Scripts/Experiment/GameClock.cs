using UnityEngine;
using System.Collections;
using System;

public class GameClock : MonoBehaviour {

	public long GameTime_Milliseconds { get { return GetGameTime(); } }
	public static long SystemTime_Milliseconds { get { return GetSystemClockMilliseconds (); } }
	public static long SystemTime_Microseconds { get { return GetSystemClockMicroseconds (); } }
	
	public static string SystemTime_MillisecondsString { get { return FormatTime (SystemTime_Milliseconds); } }
	public static string SystemTime_MicrosecondsString { get { return FormatTime (SystemTime_Microseconds); } }
	
	protected long microseconds = 1;
	long initialSystemClockMilliseconds;
	
	void Awake(){
		initialSystemClockMilliseconds = GetSystemClockMilliseconds();
	}
	
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	
	long GetGameTime(){
		return GetSystemClockMilliseconds () - initialSystemClockMilliseconds;
	}
	
	static long GetSystemClockMilliseconds(){
		//long ticks = DateTime.Now.Ticks;
		//Debug.Log (DateTime.Now.Ticks);
		//Debug.Log (DateTime.Now);
		
		//long seconds = tick / TimeSpan.TicksPerSecond;
		long milliseconds = 0;// = ticks / TimeSpan.TicksPerMillisecond;
		
		
		
		
		
		DateTime e = DateTime.UtcNow;//new DateTime(2011, 12, 31, 0, 0, 0, DateTimeKind.Utc);
		DateTime s = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
		TimeSpan t = (e - s);
		milliseconds = (long)t.TotalMilliseconds;
		
		return milliseconds;
	}
	
	static long GetSystemClockMicroseconds(){
		//Convenience method to return the system time.
		
		//long ticks = DateTime.Now.Ticks;
		
		long microseconds;// = ticks / (TimeSpan.TicksPerMillisecond / 1000);
		
		microseconds = GetSystemClockMilliseconds () * 1000; //NOTE: TECHNICALLY JUST MILLISECONDS, NOT ACCURATE MICROSECONDS.
		
		return microseconds;
	}
	
	static string GetFormattedMillisecondsAsString(){
		return FormatTime(SystemTime_Milliseconds);
	}
	
	static string GetFormattedMicrosecondsAsString(){
		return FormatTime(SystemTime_Microseconds);
	}
	
	public static string FormatTime(long time){
		return time.ToString().PadLeft(20, '0');
	}


}
