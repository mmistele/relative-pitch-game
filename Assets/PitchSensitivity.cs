using UnityEngine;
using System.Collections;

public class PitchSensitivity : MonoBehaviour {

	public MicrophoneCapture micCapture;
	public float targetPitch;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		float pitchProduced = micCapture.lastPitch;
		Color c = GetComponent<Renderer> ().material.color;
		float alpha = Mathf.Clamp(1.0F - 1.0F/(0.5F * Mathf.Abs (targetPitch - pitchProduced)), 0, 1);
		Debug.Log (alpha);
		GetComponent<Renderer> ().material.color = new Color (c.r, c.g, c.b, alpha);
	}
}
