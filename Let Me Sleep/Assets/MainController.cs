using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public delegate void ThresholdCrossedFunction();
public struct DecibelEvent
{
	public float ts, db;
}
[System.Serializable]
public struct Threshold{
	public float db;
	public ThresholdCrossedFunction callback;
}
public class MainController : MonoBehaviour
{
	TwilioMessaging twilio;
	SG_Email sendgrid;
	List<DecibelEvent> decibels;
	public float[] ios_thresholds, osx_thresholds;
	// Use this for initialization
	void Start ()
	{
		this.decibels = new List<DecibelEvent> ();
		this.twilio = this.GetComponent<TwilioMessaging> ();
		this.sendgrid = this.GetComponent<SG_Email> ();
		this.GetComponent<DecibelReader> ().setOnDecibel (this.OnDecibel);
	}

	public void OnDecibel (float dec)
	{
		DecibelEvent ev = new DecibelEvent ();
		ev.db = dec;
		ev.ts = Time.time;
	}

	// Update is called once per frame
	void Update ()
	{
		//float avg = 5;
		//double needed = 0.4026765184 * Mathf.Pow (avg, 4) - 26.25645611 * Mathf.Pow (avg, 3) + 334.2236632 * avg * avg - 1444.000294 * avg + 1975.630411;
		// y = 2.788495725 x4 - 112.5712224 x3 + 874.0103878 x2 - 2018.589941 x + 1033.231182
#if UNITY_EDITOR || UNITY_STANDALONE_OSX
		#elif UNITY_IOS
		#endif
	}
}
