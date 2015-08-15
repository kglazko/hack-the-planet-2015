using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DecibelReader : MonoBehaviour
{
	public float low_disturbance_threshold;
	public float serious_disturbance_threshold;
	public Text txt;
	public Transform transf;
	public float[] data;
	public AudioSource source;
	public float pull_frequency;//time between pulls
	public int pull_duration, sampling_frequency;
	public int qSamples;
	public float rmsValue, dbValue, refValue, usedValue;
	// Use this for initialization
	void Start ()
	{
		this.qSamples = Mathf.RoundToInt(this.pull_duration * this.sampling_frequency);
		this.data = new float[qSamples];
		Debug.Log ("Using microphone " + Microphone.devices [0]);
		Invoke ("Record", pull_frequency);

		//this.GetComponent<TwilioMessaging> ().SendSMS ();
		//this.GetComponent<SG_Email>().SendSendgridEmailWebAPI ();
		//this.GetComponent<SG_Email> ().SendSendgridEmailSMTP ();
	}

	void GetVolume ()
	{
		//this.source.GetOutputData (this.data, this.source.clip.channels); // fill array with samples
		//this.source.GetOutputData (this.data, 0); // fill array with samples
		this.source.clip.GetData (this.data, 0); // fill array with samples
		int i;
		float sum = 0;
		for (i=0; i < qSamples; i++) {
			sum += this.data [i] * this.data [i]; // sum squared samples
		}
		rmsValue = Mathf.Sqrt (sum / qSamples); // rms = square root of average
		dbValue = 20 * Mathf.Log10 (rmsValue / refValue); // calculate dB
		if (dbValue < -160)
			dbValue = -160; // clamp it to -160dB min
	}

	public void Record ()
	{
		source.clip = Microphone.Start (Microphone.devices [0], false, pull_duration, 44100);
		Invoke ("Analyze", pull_duration);
	}

	public void Analyze ()
	{
		this.source.GetOutputData (this.data, 1);
		Microphone.End (Microphone.devices [0]);
		source.clip.GetData (data, 0);
		Debug.Log ("Grabbed new data with " + source.clip.channels + " channel");
		this.GetVolume ();
		this.usedValue = 1000 * this.rmsValue;
		this.transf.localScale = new Vector3 (1, usedValue, 1);
		this.txt.text = usedValue.ToString ("n3");
		if (this.usedValue > this.serious_disturbance_threshold) {
			this.txt.color = Color.red;
		} else	if (this.usedValue > this.low_disturbance_threshold) {
			this.txt.color = Color.yellow;
		} else {
			this.txt.color = Color.white;
		}
		this.Record ();
	}
	// Update is called once per frame
	void Update ()
	{

	}
}
