using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField]
    private float move_sensitivity = 0.1f;

    [SerializeField]
    private float zoom_sensitivity = 0.05f;

	// Update is called once per frame
	void Update ()
    {
		if(Input.GetKey(KeyCode.W))
        {
            transform.Translate(0, move_sensitivity, 0);
        }
        if (Input.GetKey(KeyCode.A))
        {
            transform.Translate(-move_sensitivity, 0, 0);
        }
        if (Input.GetKey(KeyCode.S))
        {
            transform.Translate(0, -move_sensitivity, 0);
        }
        if (Input.GetKey(KeyCode.D))
        {
            transform.Translate(move_sensitivity, 0, 0);
        }

        if(Input.GetAxis("Mouse ScrollWheel") > 0)
        {
            GetComponent<Camera>().orthographicSize *= (1 - zoom_sensitivity);
        }
        else if (Input.GetAxis("Mouse ScrollWheel") < 0)
        {
            GetComponent<Camera>().orthographicSize *= (1 + zoom_sensitivity);
        }
    }
}
