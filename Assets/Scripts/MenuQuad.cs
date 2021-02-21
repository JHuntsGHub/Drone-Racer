using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 *  MenuQuad simply makes the Quad in the menu bob up and down.
 */
public class MenuQuad : MonoBehaviour {

    private Vector3 startPos, endPos;
    private float speed;

	// Use this for initialization
	void Start () {
        startPos = transform.position;
        endPos = new Vector3(startPos.x, startPos.y - 15, startPos.z);
        speed = 1.2f;
    }
	
	// All the Update() method does is make the Quad bob up and down.
	void Update () {
        transform.Translate(0, speed * Time.deltaTime, 0);

        if (transform.position.y > startPos.y || transform.position.y < endPos.y)
            speed *= -1;
	}
}
