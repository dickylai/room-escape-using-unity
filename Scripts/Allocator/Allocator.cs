using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace RoomEscape {
	/// <summary>
	/// The following allocator classes are for implementing the allocation of objects and relations and linkages of objects.
	/// The allocators include room allocator, door allocator, furniture allocator and interactive object allocator.
	/// These allocators are encapsulated in the main program (i.e. RoomGenerator) and called by it when necessary.
	/// </summary>
	// This is an abstract class providing different allocators performing allocating objects (i.e. rooms, furnitures)
	public abstract class Allocator {
		protected System.Random prng;

		protected List<Vector2[]> allocatedSpace = new List<Vector2[]> ();

		public Allocator (System.Random prng) { this.prng = prng; }

		public void AddObjectByObject (GameObject obj) {
			allocatedSpace.Add (getPointsByObject (obj));
		}

		public void AddObjectByValues (float width, float depth, float originX, float originZ) {
			allocatedSpace.Add (getPointsByValues (width, depth, originX, originZ));
		}

		// function for getting the vertices by values
		protected Vector2[] getPointsByValues (float x, float z, float originX, float originZ) {
			Vector2[] temp = new Vector2[4];
			temp [0].x = originX - x / 2f;
			temp [0].y = originZ + z / 2f;
			temp [1].x = originX + x / 2f;
			temp [1].y = originZ + z / 2f;
			temp [2].x = originX - x / 2f;
			temp [2].y = originZ - z / 2f;
			temp [3].x = originX + x / 2f;
			temp [3].y = originZ - z / 2f;
			return temp;
		}

		// function for getting the vertices by object
		protected Vector2[] getPointsByObject (GameObject obj) {
			Vector2[] temp = new Vector2[4];
			for (int i = 0; i < temp.Length; i++) {
				temp [i] = getXZPoint (obj, i);
			}
			return temp;
		}

		// function called by getPoints for a specific vertex
		// different modes for different vertices 
		//  0 _____________________ 1
		//   |                     |
		//   |                     |
		//   |_____________________|
		//  2                       3
		protected Vector2 getXZPoint (GameObject obj, int mode) {
			float vertX, vertZ;
			float x = obj.transform.lossyScale.x;
			float z = obj.transform.lossyScale.z;
			float originX = obj.transform.position.x;
			float originZ = obj.transform.position.z;
			if (mode == 0) {
				vertX = originX - x / 2;
				vertZ = originZ + z / 2;
			} else if (mode == 1) {
				vertX = originX + x / 2;
				vertZ = originZ + z / 2;
			} else if (mode == 2) {
				vertX = originX - x / 2;
				vertZ = originZ - z / 2;
			} else {
				vertX = originX + x / 2;
				vertZ = originZ - z / 2;
			}
			return new Vector2 (vertX, vertZ);
		}

		public bool IsCollidedByValues (float x, float z, float originX, float originZ) {
			Vector2[] test = getPointsByValues (x, z, originX, originZ);
			if (allocatedSpace.Count == 0)
				return false;
			foreach (Vector2[] points in allocatedSpace) {
				if (intersectShape (points, test))
					return true;
			}
			return false;
		}

		public bool IsCollidedByObject (GameObject obj) {
			Vector2[] test = getPointsByObject (obj);
			if (allocatedSpace.Count == 0)
				return false;
			foreach (Vector2[] points in allocatedSpace) {
				if (intersectShape (points, test))
					return true;
			}
			return false;
		}

		protected bool intersectShape (Vector2[] a, Vector2[] b) {
			return ( ! (a[2].y >= b[1].y || a[1].y <= b[2].y || a[1].x <= b[2].x || a[2].x >= b[1].x)  );
		}
	}
}