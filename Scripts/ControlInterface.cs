using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace RoomEscape {
	public class ControlInterface : MonoBehaviour {

		public Texture2D crosshairImage;
		public Image panel;
		private List<Image> thumbnails;
		private List<PickIntObj> objs;
		private RoomGenerator roomGenerator;

		// Use this for initialization
		void Start () {
			roomGenerator = GetComponent<RoomGenerator> ();
			thumbnails = new List<Image> ();
			objs = new List<PickIntObj> ();
			thumbnails.AddRange (panel.GetComponentsInChildren<Image> ());
		}

		// Update is called once per frame
		void Update () {
			if ( Input.GetMouseButtonDown (0)){ 
				RaycastHit hit; 
				Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition); 
				if (Physics.Raycast (ray, out hit, 1.8f)) {
					Debug.Log (hit.transform.tag);
					if (hit.transform.CompareTag ("staticObj") || hit.transform.CompareTag ("nonStaticObjPull") || hit.transform.CompareTag ("nonStaticObjRotate") || hit.transform.CompareTag ("door")) {
						InteractiveObject interactiveObject = roomGenerator.ia.GetInteractiveObject (hit.transform.gameObject);
						if (interactiveObject != null) {
							roomGenerator.ia.GetLink (interactiveObject, getSelectedObj ());
							updateInventory ();
						}
					}
				}
			}
			for (int key = 0; key < 10; key++) {
				if (Input.GetKeyDown ("" + ((key + 1) % 10))) {
					if (objs.Count > key) {
						objs [key].Choose ();
						if (objs [key].GetState () == 1)
							thumbnails [key + 1].color = Color.white;
						else
							thumbnails [key + 1].color = Color.gray;
						break;
					}
				}
			}
		}

		PickIntObj getSelectedObj () {
			foreach (PickIntObj obj in objs) {
				if (obj.GetState () == 1)
					return obj;				
			}
			return null;
		}

		void updateInventory() {
			objs = new List<PickIntObj> ();
			foreach (InteractiveObject item in roomGenerator.ia.interactiveObjects) {
				if (item.GetType ().Name == "PickIntObj" && (item.GetState () == 2 || item.GetState () == 1))
					objs.Add ((PickIntObj)item);
			}
			for (int i = 1; i < 11; i++) {
				thumbnails [i].color = Color.gray;
			}
			thumbnails [objs.Count + 1].overrideSprite = null;
			for (int i = 0; i < objs.Count; i++) {
				thumbnails [i + 1].overrideSprite = Resources.Load<Sprite> (objs [i].GetThumbnail ());
			}
		}

		// generate the crosshair at the middle of the screen
		void OnGUI()
		{
			float xMin = (Screen.width / 2) - (crosshairImage.width / 4);
			float yMin = (Screen.height / 2) - (crosshairImage.height / 4);
			GUI.DrawTexture(new Rect(xMin, yMin, crosshairImage.width / 2, crosshairImage.height / 2), crosshairImage);
		}
	}
}