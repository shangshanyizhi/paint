using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraContral : MonoBehaviour {
    Vector3 clickPos;
    Vector3 dragPos;
    Quaternion startRotation;

	// Use this for initialization
	void Start () {
		
	}
    private void OnMouseDown()
    {
        clickPos =Input.mousePosition;
        startRotation = transform.rotation;
    }
    private void OnMouseDrag()
    {
        dragPos = Input.mousePosition;
        //transform.rotation =startRotation;
        //Quaternion.EulerRotation()
        transform.Rotate(-Vector3.up * (dragPos.x - clickPos.x),Space.World);
        transform.Rotate(Vector3.right * (dragPos.y - clickPos.y), Space.World);
        clickPos = Input.mousePosition;
    }
    // Update is called once per frame
    void Update () {
        //if (Input.GetMouseButtonDown(0))
        //{

        //}
       // Debug.Log(transform.rotation);
	}
}
