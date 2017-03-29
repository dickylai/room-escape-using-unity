using UnityEngine;
using System.Collections;

public class Interaction : MonoBehaviour {
	// Use this for initialization
	bool rotate;
	float angle, smooth;

	bool pull;
	float pullTarget;

	bool disappear;

	void Start () {
		rotate = false;
		angle = 0;
		smooth = 2.0f;

		pull = false;

		disappear = false;
	}
	
	// Update is called once per frame
	void Update () {
		if (rotate) {
			Quaternion target = Quaternion.Euler (0, angle, 0);
			transform.localRotation = Quaternion.Slerp(transform.localRotation, target,
				Time.deltaTime * smooth);
			if (transform.localRotation.y == angle)
				rotate = false;
		}

		if (pull) {
			Vector3 target = new Vector3 (transform.position.x, transform.position.y, pullTarget);
			transform.position = Vector3.Slerp (transform.position, target, Time.deltaTime * smooth);
			if (transform.position == target)
				pull = false;
		}

		if (disappear) {
			this.gameObject.SetActive (false);
			disappear = false;
		}
	}

	public void Rotation (float r) {
		this.angle = transform.eulerAngles.y + r;
		rotate = true;
	}

	public void Pulling (float d) {
		this.pullTarget = transform.position.z + d;
		pull = true;

	}

	public void Disappear () {
		disappear = true;
	}
}
