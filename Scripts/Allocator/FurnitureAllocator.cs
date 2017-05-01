using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace RoomEscape {
	public class FurnitureAllocator : Allocator {
		public List<Furniture> furnitures;
		float maxX, minX, maxZ, minZ;
		Vector2[] floor;
		float thickOfWall = 0.25f;

		public FurnitureAllocator (float width, float depth, float originX, float originZ, System.Random prng) : base (prng) {
			furnitures = new List<Furniture> ();
			floor = getPointsByValues (width - 2 * thickOfWall, depth - 2 * thickOfWall, originX, originZ);
			maxX = originX + width / 2 - thickOfWall;
			minX = originX - width / 2 + thickOfWall;
			maxZ = originZ + depth / 2 - thickOfWall;
			minZ = originZ - depth / 2 + thickOfWall;
		}

		public void AllocateDoor(int side, float x, float z) {
			if (side == 0)
				x++;
			else if (side == 1)
				z++;
			else if (side == 2)
				x--;
			else if (side == 3)
				z--;
			allocatedSpace.Add (getPointsByValues (2, 2, x, z));
		}

		// function for checking if an object is inside the room using parameters
		public bool IsInsideFloorByValues (float x, float z, float originX, float originZ) {
			Vector2[] temp = getPointsByValues (x, z, originX, originZ);
			return (floor [1].x >= temp [1].x && floor [1].y >= temp [1].y && floor [2].x <= temp [2].x && floor [2].y <= temp [2].y);
		}

		// function for checking if an object is inside the room using a GameObject
		public bool IsInsideFloorByObject (GameObject obj) {
			Vector2[] temp = getPointsByObject (obj);
//			Debug.Log ("floor [1].x:" + floor [1].x + " temp [1].x:" + temp [1].x + " floor [1].y:" + floor [1].y + " temp [1].y:" + temp [1].y + "floor [2].x:" + floor [2].x + " temp [2].x:" + temp [2].x + " floor [2].y:" + floor [2].y + " temp [2].y:" + temp [2].y);
			return (floor [1].x >= temp [1].x && floor [1].y >= temp [1].y && floor [2].x <= temp [2].x && floor [2].y <= temp [2].y);
		}

		// function for checking if enough furniture inside the room
		public bool IsEnoughFurniture (int max) {
			return (furnitures.Count < max) ? false : true;
		}

		// function for checking if no furniture inside the room
		public bool IsNoFurniture () {
			Debug.Log (Time.realtimeSinceStartup + " furniture count: " + furnitures.Count);
			return (furnitures.Count == 0) ? true : false;
		}

		// function for allocating furniture set in the room
		public bool AllocateFurnitureSet (GameObject obj, int againstWall) {
			float originX, originZ;
			int trial = 0;
			do {
				if (againstWall == 1) {
					int randSize = prng.Next(0,3);
					// int randSize = 1;
					if (randSize == 0) {
						obj.transform.Rotate(new Vector3 (0,270,0));
						originX = floor [1].x - obj.transform.lossyScale.z / 2;
						originZ = prng.Next ((int)minZ * 10, (int)maxZ * 10) / 10;
					} else if (randSize == 1) {
						obj.transform.Rotate(new Vector3 (0,90,0));
						originX = floor [2].x + obj.transform.lossyScale.z / 2;
						originZ = prng.Next ((int)minZ * 10, (int)maxZ * 10) / 10;
					} else {
						originX = prng.Next ((int)minX * 10, (int)maxX * 10) / 10;
						originZ = floor [2].y + obj.transform.lossyScale.z / 2;
					}
				} else {
					originX = prng.Next ((int)minX * 10, (int)maxX * 10) / 10;
					originZ = prng.Next ((int)minZ * 10, (int)maxZ * 10) / 10;
				}
				obj.transform.position = new Vector3 (originX, obj.transform.position.y, originZ);
				trial++;
			} while ((!IsInsideFloorByObject (obj) || IsCollidedByObject (obj)) && trial < 1);
			if (!IsCollidedByObject (obj) && IsInsideFloorByObject (obj)) {
				allocatedSpace.Add (getPointsByObject (obj));
				furnitures.Add (new Furniture ("Furniture" + (furnitures.Count + 1), originX, originZ, obj));
				return true;
			} else {
				return false;
			}
		}

		// function for allocating furniture in the room
		public bool AllocateFurniture (GameObject obj, int againstWall) {
			float originX, originZ;
			int trial = 0;
			do {
				if (againstWall == 1) {
					//int randSize = prng.Next(0,2);
					int randSize = 1;
					if (randSize == 0) {
						obj.transform.Rotate(new Vector3 (0,270,0));
						originX = floor [1].x - obj.transform.lossyScale.z / 2;
						originZ = prng.Next ((int)minZ * 10, (int)maxZ * 10) / 10;
					} else {
						originX = prng.Next ((int)minX * 10, (int)maxX * 10) / 10;
						originZ = floor [2].y + obj.transform.lossyScale.z / 2;
					}
				} else {
					originX = prng.Next ((int)minX * 10, (int)maxX * 10) / 10;
					originZ = prng.Next ((int)minZ * 10, (int)maxZ * 10) / 10;
				}
				obj.transform.position = new Vector3 (originX, obj.transform.position.y, originZ);
				trial++;
			} while ((!IsInsideFloorByObject (obj) || IsCollidedByObject (obj)) && trial < 1);
			if (!IsCollidedByObject (obj) && IsInsideFloorByObject (obj)) {
				allocatedSpace.Add (getPointsByObject (obj));
				furnitures.Add (new Furniture ("Furniture" + (furnitures.Count + 1), originX, originZ, obj));
				return true;
			} else {
				return false;
			}
		}
		public bool AllocateFurniture (GameObject obj) {
			float originX, originZ;
			originX = obj.transform.parent.position.x;
			originZ = obj.transform.parent.position.z;
			obj.transform.position = new Vector3 (originX, obj.transform.position.y, originZ);
			obj.transform.rotation = obj.transform.parent.rotation;
			// furnitures.Add (new Furniture ("Furniture" + (furnitures.Count + 1), originX, originZ, obj));
			return true;
		}
	}
}