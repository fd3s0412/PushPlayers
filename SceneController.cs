using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneController : MonoBehaviour {

    public void ChangeHostMode()
    {
        // 現在の状態を反転させて保存する
        bool isHost = !PlayerPrefs.GetString("isHost", "False").Equals("True");
        PlayerPrefs.SetString("isHost", isHost.ToString());
        GameObject viewer = GameObject.Find("HostModeViewer");
        if (viewer)
        {
            Text obj = viewer.GetComponent<Text>();
            obj.text = "HostMode : " + isHost.ToString();
        }
    }

    public void SceneRestart() {
        int sceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(sceneIndex);
    }

    public void ChangeSceneStart()
    {
        SceneManager.LoadScene("Start");
    }

    public void ChangeSceneField()
    {
        SceneManager.LoadScene("Field");
    }
}
