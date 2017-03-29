using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace RoomEscape {
	// InteractiveObject class defines an interactive object
	public class InteractiveObject : GameComponent {
		protected bool isSolved;
		protected bool hasNext;
		protected int state;
		protected Vector3 location;
		protected List<string> nextType;

		public InteractiveObject (Location location, GameObject prefab, List<string> nextType) {
			isSolved = false;
			hasNext = false;
			state = 1;
			obj = prefab;
			name = prefab.name;
			this.originX = location.GetOriginX ();
			this.originY = location.GetOriginY ();
			this.originZ = location.GetOriginZ ();
			this.nextType = nextType;
			obj.transform.position = new Vector3(originX, originY, originZ);
			obj.transform.SetParent (location.GetGameObject ().transform.parent);
		}

		public InteractiveObject (Location location, GameObject prefab) {
			isSolved = false;
			hasNext = false;
			state = 1;
			obj = prefab;
			name = prefab.name;
			this.originX = location.GetOriginX ();
			this.originY = location.GetOriginY ();
			this.originZ = location.GetOriginZ ();
			nextType = new List<string> ();
			obj.transform.position = new Vector3(originX, originY, originZ);
			obj.transform.SetParent (location.GetGameObject ().transform.parent);
		}

		public InteractiveObject () {}

		public Transform GetObjParent () {
			return obj.transform.parent;
		}

		public Vector3 GetObjPosition () {
			return obj.transform.position;
		}

		public void SetObjParent (Transform tran) {
			obj.transform.SetParent (tran);
		}

		public void SetObjPosition (Vector3 vec) {
			obj.transform.position = vec;
		}

		public Vector3 GetLocation () {
			return location;
		}

		public virtual void Click () {
			changeState (state - 1);
			if (state == 0) isSolved = true;
		}

		public virtual bool IsSolved () {
			return isSolved;
		}

		public string GetNextType (int seed) {
			if (nextType.Count == 0 || hasNext == true)
				return "";
			else {
				hasNext = true;
				return nextType [seed % nextType.Count];
			}
		}

		public bool HasNext() {
			return hasNext;
		}

		protected virtual void changeState (int no) {
			state = no;
		}

		public int GetState() {
			return state;
		}
	}

}