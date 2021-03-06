using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace RoomEscape {
	
	// Subclass of InteractiveObject for pick up object
	public class PickIntObj : InteractiveObject {

		protected string thumbnail;

		public PickIntObj (Location location, GameObject prefab, List<string> nextType, string thumbnail) : base (location, prefab, nextType) {
			this.thumbnail = thumbnail;
			state = 3;
		}

		public PickIntObj (GameObject prefab, List<string> nextType) : base (prefab, nextType) {}

		public override void Click () {
			obj.gameObject.transform.GetComponent<Interaction> ().Disappear ();
			base.Click ();
		}

		public void Choose () {
			if (state == 2) {
				state = 1;
			} else {
				state = 2;
			}
			isSolved = false;
		}

		public string GetThumbnail () {
			return thumbnail;
		}
	}

}