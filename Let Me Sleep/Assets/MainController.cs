using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public delegate void ThresholdCrossedFunction ();

public struct DecibelEvent
{
	public float ts, db;
}

public class MainController : MonoBehaviour
{
	TwilioMessaging twilio;
	SG_Email sendgrid;
	List<DecibelEvent> decibels;
	public float[] threshold_needed_min, threshold_needed_max;
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
		this.decibels.Add (ev);
		#if UNITY_EDITOR || UNITY_STANDALONE_OSX
		this.TryComplain (this.osx_thresholds);
		#elif UNITY_IOS
		this.tryComplain(this.ios_thresholds);
		#endif
	}

	public void TryComplain (float[] thresholds)
	{
		float sum = 0;
		float end_ts = this.decibels [this.decibels.Count - 1].ts;
		for (int i = this.decibels.Count-1; i != -1; i--) {
			float elapsed = end_ts - this.decibels [i].ts;
			sum += this.decibels [i].db;
			float avg = sum / (this.decibels.Count - i);
			if (elapsed > this.threshold_needed_max [this.threshold_needed_min.Length - 1] 
				&& avg < thresholds [thresholds.Length - 1]) {
				Debug.Log ("It's been " + elapsed + " and still no complain, breaking!");
				break;
			}
			for (int ii = 0; ii != this.threshold_needed_min.Length; ii++) {
				if(this.threshold_needed_max[ii] < elapsed){
					//Debug.Log ("Skipping " + ii + " since elapsed time is already greater than " + this.threshold_needed_max[ii]);
					continue;
				}
				if (this.threshold_needed_min [ii] > elapsed) {
					//Debug.Log ("Can't complain at level " + ii + " because elapsed time " + elapsed + " is below threshold " + this.threshold_needed_min [ii]);
					continue;
				}
				if (thresholds [ii] >= avg) {
					//Debug.Log ("Can't complain at level " + ii + " because avg " + avg + " is less than " + thresholds [ii]);
					continue;
				}
				Debug.Log ("Complain at level " + ii + " because avg = " + avg + " over time " + elapsed);
				return;
			}
		}
	}
}
