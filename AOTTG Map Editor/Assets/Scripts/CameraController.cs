﻿using UnityEngine;

public class CameraController : MonoBehaviour
{
    //The three speeds the camera can move at
    [SerializeField]
    private float fastSpeed = 300f;
    [SerializeField]
    private float normalSpeed = 100f;
    [SerializeField]
    private float slowSpeed = 30f;
    //The speed the camera rotates at
    [SerializeField]
    private float rotateSpeed = 100f;

    //Disable the fog on distant objects
    void OnPreRender()
    {
        RenderSettings.fog = false;
    }

    //If the editor is in fly mode, translate and rotate the camera
    void LateUpdate()
    {
        if(EditorManager.currentMode == EditorMode.Fly)
        {
            translateCamera();
            rotateCamera();
        }
    }

    private void translateCamera()
    {
        //The speed the camera should move at
        float currentSpeed = normalSpeed;

        //Set the speed based on if shift, control, or neither is pressed
        if (Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.LeftControl))
            currentSpeed = slowSpeed;
        else if (Input.GetKey(KeyCode.LeftControl) && !Input.GetKey(KeyCode.LeftShift))
            currentSpeed = fastSpeed;

        //Get the amount to translate on the x and z axis
        float xDisplacement = Input.GetAxisRaw("Horizontal") * currentSpeed * Time.deltaTime;
        float zDisplacement = Input.GetAxisRaw("Vertical") * currentSpeed * Time.deltaTime;

        //Get the amount to translate on the y axis
        float yDisplacement = 0;

        //If only the left mouse button is pressed, move the camera down
        if (Input.GetButton("Fire1") && !Input.GetButton("Fire2"))
            yDisplacement = -currentSpeed * Time.deltaTime;
        //If only the right mouse button is pressed, move the camera up
        else if (Input.GetButton("Fire2") && !Input.GetButton("Fire1"))
            yDisplacement = currentSpeed * Time.deltaTime;

        //Translate the camera on the x and z axes in self space
        transform.Translate(xDisplacement, 0, zDisplacement, Space.Self);
        //Translate the camera on the y axis in world space
        transform.Translate(0, yDisplacement, 0, Space.World);
    }

    private void rotateCamera()
    {
        //Find how much the camera should be rotated on the x and y axes, then add the current rotations to them
        float xRotation = (Input.GetAxis("Mouse Y") * -rotateSpeed * Time.deltaTime) + transform.rotation.eulerAngles.x;
        float yRotation = (Input.GetAxis("Mouse X") * rotateSpeed * Time.deltaTime) + transform.rotation.eulerAngles.y;

        //Restrict the camera angle so it doesn't flip
        if (xRotation > 90 && xRotation < 180)
            xRotation = 90;
        if (xRotation < 270 && xRotation > 180)
            xRotation = 270;

        //Set the new rotation of the camera
        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
    }
}