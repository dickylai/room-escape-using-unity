using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour {
	
	[Range(0,1)]
	public float forwardSpeed;
	[Range(0,1)]
	public float backwardSpeed;
	[Range(0,1)]
	public float sideSpeed;
	[Range(0,10)]
	public float mouseSensitivity;
	[Range(0,5)]
	public float smoothing;
	[Range(0,2)]
	public float interactDistance;

	public GameObject camera;
	private GameObject item;

	private Vector2 mouseLook;
	private Vector2 smoothV;

	void Start () {
		Cursor.lockState = CursorLockMode.Locked;
	}

	void Update () {
		Vector2 mouseDrag = new Vector2 (Input.GetAxisRaw ("Mouse X"), Input.GetAxisRaw ("Mouse Y"));
		mouseDrag = Vector2.Scale (mouseDrag, new Vector2 (mouseSensitivity * smoothing, mouseSensitivity * smoothing));
		smoothV.x = Mathf.Lerp (smoothV.x, mouseDrag.x, 1f / smoothing);
		smoothV.y = Mathf.Lerp (smoothV.y, mouseDrag.y, 1f / smoothing);

		mouseLook.x += smoothV.x;
		if ((mouseLook.y + smoothV.y <= 70) && (mouseLook.y + smoothV.y >= -70))
			mouseLook.y += smoothV.y;

		transform.localRotation = Quaternion.AngleAxis (mouseLook.x, transform.up);
		camera.transform.localRotation = Quaternion.AngleAxis (-mouseLook.y, Vector3.right);

		if (Input.GetKeyDown ("escape")) {
			Cursor.lockState = CursorLockMode.None;
		}
	}
		
	void FixedUpdate () {
		float moveVertical = (Input.GetAxis ("Vertical") > 0) ? Input.GetAxis ("Vertical") * forwardSpeed : Input.GetAxis ("Vertical") * backwardSpeed;
		float moveHorizontal = Input.GetAxis ("Horizontal") * sideSpeed;

		transform.Translate(moveHorizontal, 0, moveVertical);
	}

}
