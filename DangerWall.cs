using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class DangerWall : MonoBehaviour
{
    public GameObject loserLabel;

    // オブジェクトと接触した時に呼ばれるコールバック
    void OnCollisionEnter(Collision hit)
    {
        GameObject hitObj = hit.gameObject;
        // 接触したオブジェクトのタグが"Player"の場合
        if (hitObj.CompareTag("Player"))
        {
            //hit.gameObject.SetActive(false);
            hitObj.transform.position = new Vector3(0, 4, 0);
            Rigidbody rigidbody = hitObj.GetComponent<Rigidbody>();
            rigidbody.velocity = Vector3.zero;
            rigidbody.angularVelocity = Vector3.zero;
            //loserLabel.SetActive(true);
            // 現在のシーン番号を取得
            //int sceneIndex = SceneManager.GetActiveScene().buildIndex;

            // 現在のシーンを再読込する
            //SceneManager.LoadScene(sceneIndex);
        }
    }
}