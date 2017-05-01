﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace RoomEscape {
	public class ControlInterface : MonoBehaviour {

		public Texture2D crosshairImage;
		public Image panel;

		public Text seedText;
		public Text levelText;

		private List<Image> thumbnails;
		private List<PickIntObj> objs;
		private RoomGenerator roomGenerator;
		private Menu menu;

		// Use this for initialization
		void Start () {
			roomGenerator = GetComponent<RoomGenerator> ();
			menu = GetComponent<Menu> ();
			thumbnails = new List<Image> ();
			objs = new List<PickIntObj> ();
			thumbnails.AddRange (panel.GetComponentsInChildren<Image> ());

			seedText.text = " Seed: \"" + menu.GetSeed () + "\"";
			levelText.text = " Level: ";
			if (menu.GetDifficulty () == 0) levelText.text += "Easy";
			else if (menu.GetDifficulty () == 1) levelText.text += "Normal";
			else levelText.text += "Hard";
		}

		// Update is called once per frame
		void Update () {
			if ( Input.GetMouseButtonDown (0)){ 
				RaycastHit hit; 
				Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition); 
				if (Physics.Raycast (ray, out hit, 1.8f)) {
					Debug.Log (hit.transform.tag);
					if (hit.transform.CompareTag ("staticObj") || hit.transform.CompareTag ("nonStaticObjPull") || hit.transform.CompareTag ("nonStaticObjRotate") || hit.transform.CompareTag ("nonStaticObjSwitch") || hit.transform.CompareTag ("door")) {
						Debug.Log (Time.timeSinceLevelLoad + hit.transform.tag + " hihi");
						InteractiveObject interactiveObject = roomGenerator.ia.GetInteractiveObject (hit.transform.gameObject);
						if (interactiveObject != null) {
							Debug.Log (Time.timeSinceLevelLoad + " hihi2");
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

			if (Input.GetKeyDown ("k")) {
				Cursor.lockState = CursorLockMode.None;
				Cursor.visible = true;
				SceneManager.LoadScene ("Menu");
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