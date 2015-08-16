using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public delegate void ThresholdCrossedFunction ();

public struct DecibelEvent
{
	public float ts, db;
}

[System.Serializable]
public struct ThresholdMessage
{
	public string[] messages;
	public int start;

	public string GetAndIncr {
		get {
			string msg = messages [start];
			start = 1 + start % messages.Length;
			return msg;
		}
	}
}

public class MainController : MonoBehaviour
{
	TwilioMessaging twilio;
	SG_Email sendgrid;
	List<DecibelEvent> decibels;
	public InputField[] sms_numbers;
	public InputField[] email_addresses;
	public float[] threshold_needed_min, threshold_needed_max;
	public float[] ios_thresholds, osx_thresholds;
	public ThresholdMessage[] messages;
	public float delay_between_messages;
	bool started;
	int numViolations;
	bool canSMS;
	// Use this for initialization
	void Start ()
	{
		this.started = false;
		this.numViolations = 0;
		this.canSMS = true;
		this.decibels = new List<DecibelEvent> ();
		this.twilio = this.GetComponent<TwilioMessaging> ();
		this.sendgrid = this.GetComponent<SG_Email> ();
		this.GetComponent<DecibelReader> ().setOnDecibel (this.OnDecibel);
		this.loadSaved ();
	}

	void loadSaved ()
	{
		for (int i = 0; i != this.sms_numbers.Length; i++) {
			if (PlayerPrefs.HasKey (SMS_KEY + i)) {
				this.sms_numbers [i].text = PlayerPrefs.GetString (SMS_KEY + i);
			}
		}
		for (int i = 0; i != this.email_addresses.Length; i++) {
			if (PlayerPrefs.HasKey (EMAIL_KEY + i)) {
				this.email_addresses [i].text = PlayerPrefs.GetString (EMAIL_KEY + i);
			}
		}
	}

	void onDelayOver ()
	{
		this.canSMS = true;
	}

	public void OnDecibel (float dec)
	{
		if (!this.started) {
			return;
		}
		DecibelEvent ev = new DecibelEvent ();
		ev.db = dec;
		ev.ts = Time.time;
		this.decibels.Add (ev);
		#if UNITY_EDITOR || UNITY_STANDALONE_OSX
			int complain = this.TryComplain (this.osx_thresholds);
		#elif UNITY_IOS
		int complain = this.TryComplain(this.ios_thresholds);
		#endif
		if (complain != -1) {
			this.Complain (complain);
		}
	}

	void Complain (int index)
	{
		//Twilio
		string msg = this.messages [index].GetAndIncr;
		Debug.Log ("Send message " + msg);
		this.twilio.body = msg;
		string[] str = new string[this.sms_numbers.Length];
		for (int i = 0; i != this.sms_numbers.Length; ++i) {
			str [i] = this.sms_numbers [i].text;
		}
		this.twilio.SendSMSBatch (str);

		//Sendgrid
		++this.numViolations;
		if (this.numViolations > 4) {
			str = new string[this.email_addresses.Length];
			for (int i = 0; i != this.email_addresses.Length; ++i) {
				str [i] = this.email_addresses [i].text;
			}
			this.sendgrid.SendSendgridEmailWebBatch (str);
		}

		this.canSMS = false;
		Invoke ("onDelayOver", delay_between_messages);
		if (this.decibels.Count > 3 * this.threshold_needed_max [this.threshold_needed_max.Length - 1]) {
			Debug.Log ("Rebuild decibles history");
			List<DecibelEvent> newdb = new List<DecibelEvent> ();
			for (int i = (int)(this.threshold_needed_max[this.threshold_needed_max.Length-1]); i != 0; --i) {
				newdb.Add (this.decibels [this.decibels.Count - i]);
			}
		}
	}

	public float avgnow;

	int TryComplain (float[] thresholds)
	{
		if (!canSMS) {
			return -1;
		}
		float sum = 0;
		float end_ts = this.decibels [this.decibels.Count - 1].ts;
		for (int i = this.decibels.Count-1; i != -1; i--) {
			float elapsed = end_ts - this.decibels [i].ts;
			sum += this.decibels [i].db;
			float avg = sum / (this.decibels.Count - i);
			avgnow = avg;
			if (elapsed > this.threshold_needed_max [this.threshold_needed_min.Length - 1] 
				&& avg < thresholds [thresholds.Length - 1]) {
				Debug.Log ("It's been " + elapsed + " and still no complain, breaking!");
				break;
			}
			for (int ii = 0; ii != this.threshold_needed_min.Length; ii++) {
				if (this.threshold_needed_max [ii] < elapsed) {
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
				return ii;
			}
		}
		return -1;
	}

	public void OnStartStop ()
	{
		Debug.Log (this.started ? "Stopping" : "Starting");
		this.started = !this.started;
		GameObject[] objs = GameObject.FindGameObjectsWithTag ("sheep");
		foreach (GameObject obj in objs) {
			obj.GetComponent<Image> ().enabled = this.started;
		}
		this.decibels = new List<DecibelEvent> ();
	}

	public const string EMAIL_KEY = "saved_email";
	public const string SMS_KEY = "saved_phone";

	public void OnEmailUpdate (int pos)
	{
		Debug.Log (this.email_addresses [pos].text);
		if (this.email_addresses [pos].text == "") {
			//Debug.Log ("delete");
			PlayerPrefs.DeleteKey (EMAIL_KEY + pos);
			return;
		}
		PlayerPrefs.SetString (EMAIL_KEY + pos, this.email_addresses [pos].text);
	}

	public void OnTextUpdate (int pos)
	{
		if (this.sms_numbers [pos].text == "") {
			PlayerPrefs.DeleteKey (SMS_KEY + pos);
			return;
		}
		PlayerPrefs.SetString (SMS_KEY + pos, this.sms_numbers [pos].text);
	}
}
