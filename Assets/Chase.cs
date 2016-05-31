using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
public class Chase : MonoBehaviour {

	public float speed = 3.0F;
	public float rotateSpeed = 3.0F;
	public Transform target;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void FixedUpdate() {
		CharacterController controller = GetComponent<CharacterController> ();
		Vector3 targetDir = target.position - transform.position;
		targetDir.y = 0;
		Vector3 forward = transform.TransformDirection (Vector3.forward);

		float rotateStep = rotateSpeed * Time.deltaTime;
		Vector3 newDir = Vector3.RotateTowards (forward, targetDir, rotateStep, 0.0F);
		transform.rotation = Quaternion.LookRotation (newDir);
		controller.SimpleMove (forward * speed);
	}
}
