using UnityEngine;
using System.Collections;

public class Interaction : MonoBehaviour {
	// Use this for initialization
	bool rotate;
	float angle, smooth;

	bool pull;
	float pullTarget;

	bool disappear;

	bool switching;

	void Start () {
		rotate = false;
		angle = 0;
		smooth = 2.0f;

		pull = false;

		disappear = false;

		switching = false;
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
			Vector3 target = (transform.eulerAngles.y % 180 == 0) ? new Vector3 (transform.position.x, transform.position.y, pullTarget) : new Vector3 (pullTarget, transform.position.y, transform.position.z);
			transform.position = Vector3.Slerp (transform.position, target, Time.deltaTime * smooth);
			if (transform.position == target) {
				pull = false;
			}
		}

		if (disappear) {
			this.gameObject.SetActive (false);
			disappear = false;
		}

		if (switching) {
			this.transform.GetChild (0).gameObject.SetActive (!this.transform.GetChild (0).gameObject.activeSelf);
			this.transform.GetChild (1).gameObject.SetActive (!this.transform.GetChild (1).gameObject.activeSelf);
			if (this.transform.name == "box")
				this.transform.GetChild (5).gameObject.SetActive (false);
			switching = false;
		}
	}

	public void Rotation (float r) {
		this.angle = transform.localEulerAngles.y + r;
		rotate = true;
	}

	public void Pulling (float d) {
		if (transform.eulerAngles.y == 0)
			this.pullTarget = transform.position.z + d;
		else if (transform.eulerAngles.y == 90)
			this.pullTarget = transform.position.x + d;
		else if (transform.eulerAngles.y == 180)
			this.pullTarget = transform.position.z - d;
		else if (transform.eulerAngles.y == 270)
			this.pullTarget = transform.position.x - d;
		pull = true;

	}

	public void Disappear () {
		disappear = true;
	}

	public void Switching () {
		switching = true;
	}
}
