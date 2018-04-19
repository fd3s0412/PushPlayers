using UnityEngine;
using System.Collections;

public class CharacterActionManager : MonoBehaviour
{
    // このスクリプトで使う変数一覧
    private CharacterController charaCon;       // キャラクターコンポーネント用の変数
    private Animator animCon;  //  アニメーションするための変数
    public float idoSpeed = 5.0f;         // 移動速度（Public＝インスペクタで調整可能）
    public float kaitenSpeed = 1200.0f;   // プレイヤーの回転速度（Public＝インスペクタで調整可能）

    //private Vector3 movePower = Vector3.zero;    // キャラクター移動量（未使用）
    //private float jumpPower = 10.0f;        // キャラクター跳躍力（未使用）
    private const float gravityPower = 9.8f;         // キャラクター重力（未使用）


    // ■最初に1回だけ実行する処理
    void Start()
    {
        charaCon = GetComponent<CharacterController>(); // キャラクターコントローラーのコンポーネントを参照する
        animCon = GetComponent<Animator>(); // アニメーターのコンポーネントを参照する
    }


    // ■毎フレーム常に実行する処理
    void Update()
    {
        // ▼▼▼移動処理▼▼▼
        if (Input.GetAxis("Vertical") == 0 && Input.GetAxis("Horizontal") == 0)  //  テンキーや3Dスティックの入力（GetAxis）がゼロの時の動作
        {
            animCon.SetBool("Run", false);  //  Runモーションしない
        }

        else //  テンキーや3Dスティックの入力（GetAxis）がゼロではない時の動作
        {
            var cameraForward = Vector3.Scale(Camera.main.transform.forward, new Vector3(1, 0, 1)).normalized;  //  カメラが追従するための動作
            Vector3 direction = cameraForward * Input.GetAxis("Vertical") + Camera.main.transform.right * Input.GetAxis("Horizontal");  //  テンキーや3Dスティックの入力（GetAxis）があるとdirectionに値を返す
            animCon.SetBool("Run", true);  //  Runモーションする

            MukiWoKaeru(direction);  //  向きを変える動作の処理を実行する（後述）
            IdoSuru(direction);  //  移動する動作の処理を実行する（後述）
        }


        // ▼▼▼アクション処理▼▼▼
        animCon.SetBool("Action", Input.GetKey("x"));  //  ボタンを押したらアクションする

    }


    // ■向きを変える動作の処理
    void MukiWoKaeru(Vector3 mukitaiHoukou)
    {
        Quaternion q = Quaternion.LookRotation(mukitaiHoukou);          // 向きたい方角をQuaternion型に直す
        transform.rotation = Quaternion.RotateTowards(transform.rotation, q, kaitenSpeed * Time.deltaTime);   // 向きを q に向けてじわ～っと変化させる.
    }


    // ■移動する動作の処理
    void IdoSuru(Vector3 idosuruKyori)
    {
        charaCon.Move(idosuruKyori * Time.deltaTime * idoSpeed);   // プレイヤーの移動距離は時間×移動スピードの値
    }
}
