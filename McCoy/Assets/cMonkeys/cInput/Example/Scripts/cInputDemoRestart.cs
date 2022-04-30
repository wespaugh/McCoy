using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class cInputDemoRestart : MonoBehaviour {

	public Text resetText;

	void Start() {
		resetText.enabled = false;
	}

	void OnMouseDown() {
		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
	}
}
