using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace RoomEscape {

	public class SwitchIntObj : InteractiveObject {
		private bool isOpened;
		public SwitchIntObj (Location location, GameObject prefab, int keyNo) : base (location, prefab, keyNo) {
			isOpened = false;
		}

		public override void Click () {
			if (!isSolved && !isOpened) {
				obj.gameObject.transform.GetComponent<Interaction> ().Switching ();
				base.Click ();
				isOpened = true;
			} 
		}
	}
}