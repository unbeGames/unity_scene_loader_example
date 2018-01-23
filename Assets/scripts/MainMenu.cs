using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Controller;

namespace UI {
	public class MainMenu : MonoBehaviour {
		ApplicationController app;

		void Start () {
			app = ApplicationController.app;
		}

		public void Level1() {
			app.Level1();
		}

		public void Level2() {
			app.Level2();
		}

		public void Back(){
			app.ToStartup();
		}

		public void Quit(){
			app.Quit();
		}
	}
}
