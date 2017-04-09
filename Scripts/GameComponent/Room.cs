using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace RoomEscape {
	// Room class defines a room including floor, ceiling and walls.
	// This class includes the creation of floor, ceiling and walls.
	// FurnitureAllocator class is used for determining how the furniture is placed.
	public class Room : GameComponent {

		private float thickOfWall = 0.25f;
		private List<GameObject> walls;
		public FurnitureAllocator fa;
		private Color wallColor, ceilingColor;
		private Texture floorTexture;

		// initialize a room
		public Room(string name, float width, float depth, float originX, float originZ, System.Random prng) {
			this.width = width;
			this.depth = depth;
			this.originX = originX;
			this.originZ = originZ;
			this.name = name;
			this.height = 3;

			obj = new GameObject ();
			obj.name = this.name;

			wallColor = new Color((float)(prng.Next (0, 101)/100.0),(float)(prng.Next (0, 101)/100.0),(float)(prng.Next (0, 101)/100.0));
			ceilingColor = new Color((float)(prng.Next (0, 101)/100.0),(float)(prng.Next (0, 101)/100.0),(float)(prng.Next (0, 101)/100.0));
			floorTexture = (Texture) Resources.LoadAll ("Materials/Floors")[prng.Next(0,Resources.LoadAll ("Materials/Floors").Length)];

			fa = new FurnitureAllocator (width, depth, originX, originZ, prng);

			//Create floor and ceiling
			makeFloor ();
			makeCeiling ();

			//Create walls
			walls = new List<GameObject> ();
			makeWall ("Wall West", originX - width / 2 + thickOfWall / 2, originZ, thickOfWall, depth);
			makeWall ("Wall South", originX, originZ - depth / 2 + thickOfWall / 2, width, thickOfWall);
			makeWall ("Wall East", originX + width / 2 - thickOfWall / 2, originZ, thickOfWall, depth);
			makeWall ("Wall North", originX, originZ + depth / 2 - thickOfWall / 2, width, thickOfWall);

			//Create light
			makeLightBulb();
		}

		// function for creating light bulb
		void makeLightBulb() {
			GameObject lightGameObject = new GameObject(name + " Light");
			Light lightComp = lightGameObject.AddComponent<Light> ();
			lightComp.color = Color.white;
			lightComp.intensity = 0.3f;
			lightComp.renderMode = LightRenderMode.ForcePixel;
			lightComp.shadows = LightShadows.Soft;
			lightGameObject.transform.position = new Vector3 (originX, 2.9f, originZ);
			lightGameObject.transform.SetParent(obj.transform);
			GameObject bulb = GameObject.CreatePrimitive (PrimitiveType.Sphere);
			bulb.transform.position = new Vector3 (originX, 3f, originZ);
			bulb.GetComponent<MeshRenderer> ().material = (Material)Resources.Load ("Materials/Transparent");
			bulb.transform.SetParent(obj.transform);
		}

		// function for creating plane for floor and ceiling
		void makeCeiling() {
			GameObject plane = GameObject.CreatePrimitive (PrimitiveType.Cube);
			plane.name = "Ceiling";
			plane.GetComponent<MeshRenderer> ().material.color = ceilingColor;
			plane.transform.position = new Vector3 (originX, height + 0.25f, originZ);
			plane.transform.localScale = new Vector3 (width, 0.5f, depth);
			Rigidbody Rigidbody = plane.AddComponent<Rigidbody> ();
			Rigidbody.isKinematic = true;
			plane.transform.SetParent(obj.transform);
		}

		void makeFloor() {
			GameObject plane = GameObject.CreatePrimitive (PrimitiveType.Cube);
			plane.name = "Floor";
			plane.GetComponent<MeshRenderer> ().material.mainTexture = floorTexture;
			plane.transform.position = new Vector3 (originX, -0.25f, originZ);
			plane.transform.localScale = new Vector3 (width, 0.5f, depth);
			Rigidbody Rigidbody = plane.AddComponent<Rigidbody> ();
			Rigidbody.isKinematic = true;
			plane.transform.SetParent(obj.transform);
		}

		// function for creating normal walls by calling makeWallHeightSpecific
		void makeWall (string name, float originX, float originZ, float x, float z) {
			makeWallHeightSpecific (name, originX, height / 2, originZ, x, height + 2 * thickOfWall, z);
		}

		// function for creating walls with different parameters
		void makeWallHeightSpecific (string name, float originX, float originY, float originZ, float x, float y, float z) {
			GameObject wall = GameObject.CreatePrimitive (PrimitiveType.Cube);
			wall.name = name;
			wall.GetComponent<MeshRenderer> ().material.color = wallColor;
			//wall.GetComponent<MeshRenderer> ().material.mainTexture = (Texture) Resources.Load("Materials/wall2");
			wall.transform.position = new Vector3 (originX, originY, originZ);
			wall.transform.localScale = new Vector3 (x, y, z);
			wall.transform.SetParent(obj.transform);
			walls.Add (wall);
		}

		// function for creating doorside walls by calling makeWallHeightSpecific
		public void MakeDoorWall (int wallNo, float doorLocation) {
			GameObject wall = walls [wallNo];
			float doorWidth = 1;
			float doorHeight = 2;
			float wallWidth, wallOrigin;
			if  (wall.transform.localScale.x == thickOfWall) {
				wallWidth = doorLocation - doorWidth / 2 - (wall.transform.position.z - depth / 2);
				wallOrigin = (wall.transform.position.z - depth / 2) + wallWidth / 2;
				makeWall (wall.transform.name, wall.transform.position.x, wallOrigin, thickOfWall, wallWidth);
				wallWidth = depth - doorWidth - wallWidth;
				wallOrigin = doorLocation + (doorWidth + wallWidth) / 2;
				makeWall (wall.transform.name, wall.transform.position.x, wallOrigin, thickOfWall, wallWidth);
				wall.transform.position = new Vector3 (wall.transform.position.x, (height - doorHeight) / 2 + doorHeight, doorLocation);
				wall.transform.localScale = new Vector3 (thickOfWall, height - doorHeight, doorWidth);
			} else {
				wallWidth = doorLocation - doorWidth / 2 - (wall.transform.position.x - width / 2);
				wallOrigin = (wall.transform.position.x - width / 2) + wallWidth / 2;
				makeWall (wall.transform.name, wallOrigin, wall.transform.position.z, wallWidth, thickOfWall);
				wallWidth = width - doorWidth - wallWidth;
				wallOrigin = doorLocation + (doorWidth + wallWidth) / 2;
				makeWall (wall.transform.name, wallOrigin, wall.transform.position.z, wallWidth, thickOfWall);
				wall.transform.position = new Vector3 (doorLocation, (height - doorHeight) / 2 + doorHeight, wall.transform.position.z);
				wall.transform.localScale = new Vector3 (doorWidth, height - doorHeight, thickOfWall);
			}
		}

		// function for getting the thick of wall
		public float GetThickOfWall () {
			return thickOfWall;
		}
	}
}