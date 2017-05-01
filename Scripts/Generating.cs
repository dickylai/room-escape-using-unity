using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Threading;

namespace RoomEscape {
	public class Generating : MonoBehaviour {

		public Text showText;
		private string text;
		private int dotCount;
		private Thread thread1;

		// Use this for initialization
		void Start () {
			dotCount = 0;
			thread1 = new Thread (updateText);
			thread1.Start ();
		}
		
		// Update is called once per frame
		void Update () {
			showText.text = text;
			StartCoroutine(LoadNewScene());
		}

		void updateText () {
			while (true) {
				text = "Game Generating";
				for (int i = 0; i < dotCount; i++)
					text += ".";
				dotCount = (dotCount + 1) % 4;
				System.Threading.Thread.Sleep(500);
			}
		}

		IEnumerator LoadNewScene() {
			yield return new WaitForSeconds(3);

			AsyncOperation async = Application.LoadLevelAsync("RoomEscape");

			while (!async.isDone) {
				yield return null;
			}
		}
	}
}
