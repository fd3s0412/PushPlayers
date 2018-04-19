using UnityEngine;
using System.Collections;

public class CharacterFreezeRotation : MonoBehaviour {

	// Use this for initialization
	void Start () {
        Rigidbody rigidbody = GetComponent<Rigidbody>();
        // 位置固定、回転の X と Y を固定。
        rigidbody.constraints =
        RigidbodyConstraints.FreezeRotationX |
        RigidbodyConstraints.FreezeRotationZ |
        RigidbodyConstraints.FreezePositionY;
    }
}
