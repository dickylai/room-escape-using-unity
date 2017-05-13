using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace RoomEscape {
	
	// Subclass of InteractiveObject for pick up object
	public class InterIntObj : PickIntObj {

		public InterIntObj (Location location, GameObject prefab, List<string> nextType, string thumbnail) : base (prefab, nextType) {
			prefab.SetActive (false);
			this.thumbnail = thumbnail;
			state = 3;
		}

		public override void Click () {
			// not appear in the environment
			base.Click ();
		}
	}

}