using UnityEngine;
using System.Collections;

public class Sheeple : MonoBehaviour
{
	public float speed;
	// Use this for initialization
	void Start ()
	{
		this.GetComponent<Rigidbody> ().velocity = new Vector3 (this.speed, 0, 0);
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (this.transform.position.x > Screen.width - 100) {
			this.GetComponent<Rigidbody> ().velocity = new Vector3 (-this.speed, 0, 0);
			this.transform.localScale=new Vector3(-.5f,.5f,.5f);
		}
		if (this.transform.position.x < 100) {
			this.GetComponent<Rigidbody> ().velocity = new Vector3 (this.speed, 0, 0);
			this.transform.localScale=new Vector3(.5f,.5f,.5f);
		}
	}
}
