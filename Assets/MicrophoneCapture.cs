using UnityEngine;
using System.Collections;
using Pitch;

// Adapting code from http://www.41post.com/4884/programming/unity-capturing-audio-from-a-microphone

[RequireComponent (typeof (AudioSource))]

public class MicrophoneCapture : MonoBehaviour {

	private bool micConnected = false;

	private bool recordingStarted = false;
	private bool userWantsRecording = false;
	private int previousPosition = 0;
	public float lastPitch;

	// The max and min frequencies for recording
	private int minFreq;
	private int maxFreq; // Used as our sampling rate!
	
	private PitchTracker pitchTracker;

	// How long to record before looping to overwrite it
	public int recordingWindow;

	// Number of samples at a time to send to the pitch tracker
	public int bufferLength;

	// Handle to the attached AudioSource
	private AudioSource goAudioSource;

	private PitchTracker.PitchDetectedHandler pitchDetectedHandler;

	// IEnumerator because we need to request user authorization, and come back to it.
	IEnumerator Start () {
		// Get authorization
		yield return Application.RequestUserAuthorization(UserAuthorization.Microphone);
		if (!Application.HasUserAuthorization(UserAuthorization.Microphone)) {
			micConnected = false;
			yield break;
		} 

		if (Microphone.devices.Length <= 0) {
			Debug.LogWarning ("No microphone connected!");
		} else {
			micConnected = true;

			// Getting default mic capabilities
			Microphone.GetDeviceCaps(null, out minFreq, out maxFreq);

			// If both zero, supports any frequency...
			if (minFreq == 0 && maxFreq == 0) {
				// So we make this many Hz the sampling rate
				maxFreq = 44100;
			}

			goAudioSource = this.GetComponent<AudioSource>();
		}

		pitchTracker = new PitchTracker ();
		pitchTracker.SampleRate = maxFreq;	
		pitchTracker.RecordPitchRecords = true;
		pitchTracker.PitchRecordHistorySize = 20;

		// yield break;
		pitchTracker.PitchDetected += new PitchTracker.PitchDetectedHandler (PitchDetected);
	}

	void OnGUI() {
		if (micConnected) {
			if (!Microphone.IsRecording (null)) {
				// Case 'Record' button pressed
				if (GUI.Button (new Rect (Screen.width / 2 - 100, Screen.height / 2 - 25, 200, 50), "Record")) {
					userWantsRecording = true;
				}
			} else { // Recording in progress!

				if (GUI.Button (new Rect (Screen.width / 2 - 100, Screen.height / 2 - 25, 200, 50), "Stop and Play!")) {
					userWantsRecording = false;
					Microphone.End (null);
					goAudioSource.Play ();
				}
				GUI.Label (new Rect (Screen.width / 2 - 100, Screen.height / 2 + 25, 200, 50), "Recording in progress...");
			}
			GUI.Label (new Rect (Screen.width / 2 - 100, Screen.height / 2 + 75, 200, 50), "Pitch: " + lastPitch);

		} else {
			// No mic
			GUI.Label (new Rect (Screen.width / 2 - 100, Screen.height / 2 + 25, 200, 50), "Mic not connected...");
		}

	}

	void FixedUpdate () {
		if (userWantsRecording && !Microphone.IsRecording (null)) { // User clicked to record -> start recording!
			goAudioSource.clip = Microphone.Start (null, true, recordingWindow, maxFreq);
			previousPosition = 0;
		} else if (Microphone.IsRecording (null)) { // Already recording

			// Every three frames, get the pitch
			int currentPosition = Microphone.GetPosition(null);

			// Hack for handling when we loop around. Ok or nah?
			if (currentPosition < previousPosition) {
				previousPosition = 0;
			}

			//if ((int) (currentPosition / (Time.deltaTime * maxFreq)) % 3 == 0) {
				float[] samples = new float[goAudioSource.clip.samples * goAudioSource.clip.channels];

				// Get all the data since the position when we last did this, with a bunch of 0s at the end
				goAudioSource.clip.GetData(samples, previousPosition);

				// Fill a new buffer with this data, omitting the trailing 0s
				bufferLength = currentPosition - previousPosition;

				float[] pitchTrackedBuffer = new float[bufferLength];
				System.Array.Copy(samples, 0, pitchTrackedBuffer, 0, bufferLength);

				// Have the pitch tracker process this additional data
				pitchTracker.ProcessBuffer (pitchTrackedBuffer);
			//}
			previousPosition = currentPosition;
		}

	}

	void PitchDetected(PitchTracker sender, PitchTracker.PitchRecord pitchRecord) {
		//lastPitch = pitchRecord.Pitch;

		// Average over past?
		IList latestPitches = sender.PitchRecords;
		int nonzeroPitches = 0;
		float nonzeroPitchSum = 0.0F;
		for (int i = 0; i < latestPitches.Count; i++) {
			PitchTracker.PitchRecord record = (PitchTracker.PitchRecord)latestPitches[i];
			if (record.Pitch != 0) {
				nonzeroPitches++;
				nonzeroPitchSum += record.Pitch;
			}
		}
		if (nonzeroPitches == 0) {
			lastPitch = 0;
		} else {
			lastPitch = nonzeroPitchSum / nonzeroPitches;
		}
	}
}
