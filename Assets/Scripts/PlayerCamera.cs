using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    // Serialize fields to be set in the editor
    [SerializeField] private Transform player;
    [SerializeField] private float sensitivity = 1;
    [SerializeField] private Vector3 localEulers;
    [SerializeField] private float grappleRaycastDistance = 100.0f;

    // Variables for camera rotation constraints
    private float maxUpAngle = 89;
    private float maxDownAngle = 89;

    // Public variables for the angles
    public float horizontalAngle;
    public float verticalAngle;

    void Start()
    {
        // Lock and hide the cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // Get mouse input
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        // Rotate the player horizontally based on mouse input
        if (Mathf.Abs(mouseX) > 0.01f)
        {
            player.transform.RotateAround(player.transform.position, player.transform.up, mouseX * sensitivity / 5 * Time.deltaTime);
        }

        // Adjust the localEulers based on vertical mouse input
        if (Mathf.Abs(mouseY) > 0.01f)
        {
            localEulers += new Vector3(mouseY * sensitivity / 5 * Time.deltaTime * -1, 0, 0);
        }

        // Clamp the vertical rotation and apply the rotation to the camera
        localEulers = new Vector3(Mathf.Clamp(localEulers.x, -maxUpAngle, maxDownAngle), localEulers.y, localEulers.z);
        transform.localRotation = Quaternion.Euler(localEulers);

        // Calculate the horizontal and vertical angles
        PlayerController playerScript = player.GetComponent<PlayerController>();
        horizontalAngle = -player.eulerAngles.y;
        verticalAngle = -localEulers.x;

        // Pass the vertical angle to the player script
        playerScript.verticalLookAngle = verticalAngle;
        playerScript.horizontalLookAngle = horizontalAngle;


        // Check for grapple targets
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, grappleRaycastDistance) && hit.transform.CompareTag("Grappleable"))
        {
            player.GetComponent<PlayerController>().SetGrappleTarget(hit.transform);
        }
        else
        {
            player.GetComponent<PlayerController>().SetGrappleTarget(null);
        }
    }
}
