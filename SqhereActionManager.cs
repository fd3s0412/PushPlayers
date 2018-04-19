using UnityEngine;
using System.Collections;

public class SqhereActionManager : MonoBehaviour {

	void FixedUpdate()
	{
		float x = Input.GetAxis ("Horizontal");
        Debug.Log("x : " + x);
        Debug.Log("Input.acceleration.x : " + Input.acceleration.x);
        Debug.Log("x == 0 : " + (x == 0));
        if (x == 0) x = Input.acceleration.x;

        float z = Input.GetAxis ("Vertical");
        Debug.Log("z : " + z);
        Debug.Log("Input.acceleration.y : " + Input.acceleration.y);
        Debug.Log("Input.acceleration.y : " + Input.acceleration.z);
        if (z == 0) z = Input.acceleration.y;

		Rigidbody rigidbody = GetComponent<Rigidbody>();

		// xとzに10をかけて押す力をアップ
		rigidbody.AddForce(x * 10, 0, z * 10);
	}
}
