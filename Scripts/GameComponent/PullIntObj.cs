using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace RoomEscape {
	
	// Subclass of InteractiveObject for pick up object
	public class PullIntObj : InteractiveObject {
		private bool isOpened;
		public PullIntObj (Location location, GameObject prefab, int keyNo) : base (location, prefab, keyNo) {
			isOpened = false;
		}

		public override void Click () {
			if (!isSolved && !isOpened) {
				obj.gameObject.transform.GetComponent<Interaction> ().Pulling (0.4f);
				base.Click ();
				isOpened = true;
			} 
//			else if (isSolved && isOpened) {
//				obj.gameObject.transform.GetComponent<Interaction> ().Pulling (-0.4f);
//				isOpened = false;
//			}
		}
	}
	
}