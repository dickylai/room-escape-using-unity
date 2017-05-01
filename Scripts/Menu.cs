using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

namespace RoomEscape {
	public class Menu : MonoBehaviour {
		static string seed = "";
		static int difficulty = 0;

		public void SetSeed(string input) {
			seed = input;
		} 

		public void SetDifficulty(int input) {
			difficulty = input;
		}

		public void GoToGame() {
			SceneManager.LoadScene ("Generating");
		}

		public string GetSeed () {
			return seed;
		}

		public int GetDifficulty () {
			return difficulty;
		}
	}
}