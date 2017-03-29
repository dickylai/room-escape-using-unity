using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace RoomEscape {
	// Abstract class is for the game components including rooms, doors, furnitures, locations and interactive objects.
	// It provides some basic properties and functions.
	public abstract class GameComponent {

		protected string name;
		protected float width, height, depth, originX, originY, originZ;
		protected GameObject obj;

		public string GetName () { return name; }
		public float GetWidth () { return width; }
		public float GetHeight () { return height; }
		public float GetDepth () { return depth; }
		public float GetOriginX () { return originX; }
		public float GetOriginY () { return originY; }
		public float GetOriginZ () { return originZ; }
		public GameObject GetGameObject () { 
			if (obj)
				return obj; 
			else 
				return null; }
	}
}