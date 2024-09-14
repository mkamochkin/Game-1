using UnityEngine;

public class FaceCamera : MonoBehaviour
{
    private Camera mainCamera;
    
    public float smoothSpeed = 3.0f;

    void Start()
    {
        mainCamera = Camera.main;
    }

    void LateUpdate()
    {
        Quaternion targetRotation = Quaternion.LookRotation(transform.position - mainCamera.transform.position, mainCamera.transform.rotation * Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, smoothSpeed * Time.deltaTime);
    }
}
