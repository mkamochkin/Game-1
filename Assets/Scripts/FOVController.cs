using UnityEngine;

public class FOVController : MonoBehaviour
{
    [SerializeField]
    private PlayerController playerController;
    [SerializeField]
    private float walkingFOV = 70.0f;
    [SerializeField]
    private float sprintingFOV = 95.0f;
    [SerializeField]
    private float fovSpeed;
    float targetFOV;
    
    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
    }

    void Update()
    {
        if (playerController.Sprinting() > 1.0f || playerController.Grappling() || playerController.isWallRunning == true)
        {
            targetFOV = sprintingFOV;
        }
        else
        {
            targetFOV = walkingFOV;
        }
        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, Time.deltaTime * fovSpeed);
    }
}
