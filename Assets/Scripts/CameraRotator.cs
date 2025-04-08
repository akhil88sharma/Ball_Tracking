using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class CameraRotator : MonoBehaviour
{
    public Transform target;
    public Camera mainCamera;
    [Range(0.1f, 5f)]
    [Tooltip("How sensitive the mouse drag to camera rotation")]
    public float mouseRotateSpeed = 0.8f;
    [Range(0.01f, 100)]
    [Tooltip("How sensitive the touch drag to camera rotation")]
    public float touchRotateSpeed = 17.5f;
    [Tooltip("Smaller positive value means smoother rotation, 1 means no smooth apply")]
    public float slerpValue = 0.25f;
    public enum RotateMethod { Mouse, Touch };
    [Tooltip("How do you like to rotate the camera")]
    public RotateMethod rotateMethod = RotateMethod.Touch;


    private Vector2 swipeDirection; //swipe delta vector2
    private Quaternion cameraRot; // store the quaternion after the slerp operation
    private Touch touch;
    private float offset;

    private float minXRotAngle = -90; //min angle around x axis
    private float maxXRotAngle = 5; // max angle around x axis

    //Mouse rotation related
    private float rotX; // around x
    private float rotY; // around y
    
    //For camera zoom
    [SerializeField]
    private float zoomSpeed = 10f;
    private Camera zoomCamera;
    private void Awake()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }


    }
    // Start is called before the first frame update
    void Start()
    {
        offset = Vector3.Distance(mainCamera.transform.position, target.position);
        zoomCamera = Camera.main;

        cameraRot = mainCamera.transform.rotation; // Initialize with current camera rotation
        rotX = mainCamera.transform.eulerAngles.x;
        rotY = mainCamera.transform.eulerAngles.y;
    }

    // Update is called once per frame
    void Update()
    {
        if (rotateMethod == RotateMethod.Mouse)
        {
            if(Input.GetMouseButtonDown(0))
            {
                cameraRot = mainCamera.transform.rotation;
                rotX = mainCamera.transform.eulerAngles.x;
                rotY = mainCamera.transform.eulerAngles.y;
            }

            if (Input.GetMouseButton(0))
            {
                rotX += -Input.GetAxis("Mouse Y") * mouseRotateSpeed; 
                rotY += Input.GetAxis("Mouse X") * mouseRotateSpeed;
            }

            rotX = Mathf.Clamp(rotX, minXRotAngle, maxXRotAngle);
        }
        else if (rotateMethod == RotateMethod.Touch)
        {
            if (Input.touchCount == 1)
            {
                touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began)
                {
                    //Debug.Log("Touch Began");

                }
                else if (touch.phase == TouchPhase.Moved)
                {
                    swipeDirection += touch.deltaPosition * Time.deltaTime * touchRotateSpeed;
                }
                else if (touch.phase == TouchPhase.Ended)
                {
                    //Debug.Log("Touch Ended");
                }
            }
            if (Input.touchCount == 2) // Detect pinch
            {
                Touch touch1 = Input.GetTouch(0);
                Touch touch2 = Input.GetTouch(1);

                // Calculate previous and current distances between the touches
                float prevDistance = (touch1.position - touch1.deltaPosition - (touch2.position - touch2.deltaPosition)).magnitude;
                float currDistance = (touch1.position - touch2.position).magnitude;

                // Calculate the zoom factor
                float zoomAmount = (currDistance - prevDistance) * 0.01f * zoomSpeed;

                // Apply zoom with clamping
                if (zoomCamera.orthographic)
                {
                    zoomCamera.orthographicSize = Mathf.Clamp(zoomCamera.orthographicSize - zoomAmount, 1f, 20f);
                }
                else
                {
                    zoomCamera.fieldOfView = Mathf.Clamp(zoomCamera.fieldOfView - zoomAmount, 20f, 80f);
                }
            }

            if (swipeDirection.y < minXRotAngle)
            {
                swipeDirection.y = minXRotAngle;
            }
            else if (swipeDirection.y > maxXRotAngle)
            {
                swipeDirection.y = maxXRotAngle;
            }
        }

        if (zoomCamera.orthographic)
        {
            zoomCamera.orthographicSize -= Input.GetAxis("Mouse ScrollWheel") * zoomSpeed;
        }
        else
        {
            zoomCamera.fieldOfView -= Input.GetAxis("Mouse ScrollWheel") * zoomSpeed;
        }

    }

    private void LateUpdate()
    {

        Vector3 dir = new Vector3(0, 0, -offset); //assign value to the distance between the maincamera and the target

        Quaternion newQ; // value equal to the delta change of our mouse or touch position
        if (rotateMethod == RotateMethod.Mouse)
        {
            newQ = Quaternion.Euler(rotX, rotY, 0); //We are setting the rotation around X, Y, Z axis respectively
        }
        else
        {
            newQ = Quaternion.Euler(-swipeDirection.y, swipeDirection.x, 0);
        }
        cameraRot = Quaternion.Slerp(cameraRot, newQ, slerpValue);  //let cameraRot value gradually reach newQ which corresponds to our touch
        mainCamera.transform.position = target.position + cameraRot * dir;
        mainCamera.transform.LookAt(target.position);

    }

    /*public void SetCamPos()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
        mainCamera.transform.position = new Vector3(0, 0, -offset);
    }*/
}
