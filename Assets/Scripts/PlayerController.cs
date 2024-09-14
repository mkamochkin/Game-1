
using System;
using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    // Basic movement variables
    [SerializeField]
    private float moveSpeed = 5.0f; // Base speed
    [SerializeField]
    private float maxVelocityChange = 10.0f; // Max acceleration
    float moveHorizontal;
    float moveForward;
    float currentSpeedMultiplier;

    // Jumping variables
    [SerializeField]
    private float jumpForce = 5.0f; // Jump force
    [SerializeField]
    private float sprintMultiplier = 2.0f; // Speed multiplier when sprinting
    [SerializeField]
    private float speedBasedOnGroundedMultiplier; // If on ground it's 1x, if in the air it's 0.5x
    float distanceToGround;
    private bool isGrounded;

    // Grapple variables
    [SerializeField]
    private float grappleRange; // Grapple range
    [SerializeField]   
    private float grappleForce = 10.0f; // Grapple force
    [SerializeField]
    private GameObject ropePrefab;
    private GameObject currentRope = null;
    private Transform grappleTarget; // Grapple target
    private float ropeLength = 0;
    [SerializeField]
    private float ropeThrowSpeed = 50;

    // Sliding variables
    private bool sliding;

    // Wallrun variables
    [SerializeField]
    private float wallRunTime; // Max wallrun time
    [SerializeField]
    private float wallRunAngleThreshold; // Maximum allowed angle between player movement direction and wall's normal
    [SerializeField]
    private float wallRunMaxAngle; // Maximum angle from horizontal allowed for vertical wallrun movement
    [SerializeField]
    private float minWallAngle; // Minimum angle from the ground required for a surface to be considered a wall
    public bool isTouchingWallrunnable = false; // If wall's angle is greater than minWallAngle
    public bool isWallRunning = false;
    [SerializeField]
    public float wallRunTimer = 3f; // Timer increments when wallrunning
    [SerializeField]
    private float maxLookAwayAngleForWallrun;
    private Vector3 wallRunNormal = Vector3.zero; // Normal Vector of wall to check angle
    //private bool isTouchingWallDuringRun = false;
    private bool canStartWallRun = true;
    [SerializeField]
    private float wallRunCooldown = 2.0f;
    float angleDifference;

    [SerializeField]
    private bool holdSprint; // Toggleable boolean for hold sprint or toggle sprint
    private bool isSprinting = false;
    private bool sprintKeyPressed = false;

    public float verticalLookAngle; // Vertical look angle based off horizon in degrees, up is positive
    public float horizontalLookAngle; // Horizonal look angle

    private Rigidbody rb; // Player's RigidBody

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
    }

    void Update()
    {
        // Jumping
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.velocity = new Vector3(rb.velocity.x, jumpForce, rb.velocity.z);
        }
    }

    void FixedUpdate()
    {
        // Adjust speed based on whether the player is grounded
        speedBasedOnGroundedMultiplier = isGrounded ? 1.0f : 0.05f;

        // Calculate movement
        moveHorizontal = Input.GetAxis("Horizontal");
        moveForward = Input.GetAxis("Forwards");
        currentSpeedMultiplier = Sprinting();
        //Vector3 targetVelocity = (transform.forward * moveForward + transform.right * moveHorizontal) * moveSpeed * currentSpeedMultiplier;

        // Apply acceleration
        if (!sliding)
        {
            Vector3 targetVelocity = (transform.forward * moveForward + transform.right * moveHorizontal) * moveSpeed * currentSpeedMultiplier;
            Vector3 velocityChange = targetVelocity - rb.velocity;
            velocityChange.y = 0;
            velocityChange = Vector3.ClampMagnitude(velocityChange, maxVelocityChange);
            rb.AddForce(velocityChange * speedBasedOnGroundedMultiplier, ForceMode.VelocityChange);
        }
        else if (sliding)
        {
            Vector3 targetVelocity = (transform.forward * moveForward + transform.right * moveHorizontal) * moveSpeed * currentSpeedMultiplier;
            Vector3 velocityChange = targetVelocity - rb.velocity;
            velocityChange.y = 0;
            velocityChange = Vector3.ClampMagnitude(velocityChange, maxVelocityChange);
            rb.AddForce(velocityChange * speedBasedOnGroundedMultiplier, ForceMode.VelocityChange);
        }
        

        // Grappling
        if (Grappling() && ropeLength >= (grappleTarget.position - transform.position).magnitude)
        {
            Vector3 grappleDirection = (grappleTarget.position - transform.position).normalized;
            rb.AddForce(grappleDirection * grappleForce, ForceMode.Acceleration);
            currentRope.transform.position = transform.position;
            currentRope.transform.up = grappleDirection;
            currentRope.transform.localScale = new Vector3(currentRope.transform.localScale.x, (grappleTarget.position - transform.position).magnitude, currentRope.transform.localScale.z);
            ropeLength = (grappleTarget.position - transform.position).magnitude;
        }
        else if (Grappling())
        {
            Vector3 grappleDirection = (grappleTarget.position - transform.position).normalized;
            ropeLength += Time.deltaTime * ropeThrowSpeed;
            currentRope.transform.position = transform.position;
            currentRope.transform.up = grappleDirection;
            currentRope.transform.localScale = new Vector3(currentRope.transform.localScale.x, ropeLength, currentRope.transform.localScale.z);
        }

        // Call wallrun method
        WallRun();
        Debug.DrawRay(transform.position, wallRunNormal);
        Sliding();
    }

    public void UpdateIsGrounded(bool grounded)
    {
        isGrounded = grounded;
    }

    private void Sliding()
    {
        //Debug.Log("sliding: " + sliding);
        if (isSprinting && Input.GetButton("LCtrl"))
        {
            sliding = true;
            
            Debug.Log("sliding: " + sliding);
        }
        
        else sliding = false;

        //set issprinting to false after sliding is done
        //isSprinting = false;
        Debug.Log("sliding: " + sliding);
    }
    
    
    // Sprinting
    public float Sprinting()
    {
        // If hold sprint
        if (holdSprint)
        {
            if (Input.GetButton("LShift") && Input.GetAxis("Forwards") > 0)
            {
                isSprinting = true;
                return sprintMultiplier;
            }
            isSprinting = false;
            return 1.0f;
        }
        // If toggle sprint
        else if (!holdSprint)
        {
            if (Input.GetButtonUp("LShift"))
            {
                sprintKeyPressed = true;
            }
            if (Input.GetAxis("Forwards") > 0)
            {
                if (sprintKeyPressed && !isSprinting)
                {
                    isSprinting = true;
                    sprintKeyPressed = false;
                }
                if (Input.GetButton("LShift"))
                {
                    isSprinting = true;
                }

                if (isSprinting)
                {
                    return sprintMultiplier;
                }
                else
                {
                    return 1.0f;

                }
            }
            else
            {
                isSprinting = false;
                sprintKeyPressed = false;
                return 1.0f;
            }
        }

        else
            return 0;

    }
    
    private void WallRun()
    {
        //Debug.Log(wallRunTimer);
        //Debug.Log("Horizontal angle: " + horizontalLookAngle);

        //Debug.DrawRay(transform.position, transform.forward, Color.red);
        if (isTouchingWallrunnable && isSprinting && Vector3.Angle(transform.forward, wallRunNormal) < wallRunAngleThreshold && !isGrounded && canStartWallRun)
        {
            Debug.Log("Starting angle difference: " + Vector3.Angle(transform.forward, wallRunNormal));
            //Debug.Log("Starting wall run with normal: " + wallRunNormal); // Add this line
            isWallRunning = true;
            rb.useGravity = false;
            //wallRunTimer = wallRunTime;
            canStartWallRun = false;
        }

        if (isWallRunning)
        {

            //Debug.Log("is wallrunning with wallrun normal " + wallRunNormal);
            //Debug.Log("Angle difference: " + Mathf.Abs(Vector3.Angle(wallRunNormal, Quaternion.Euler(, horizontalLookAngle, 0) * Vector3.forward)));

            // Calculate the player's forward direction based on horizontalLookAngle
            Vector3 playerForwardDirection = transform.forward;

            // Calculate the direction parallel to the wall
            Vector3 directionParallelToWall = Vector3.ProjectOnPlane(playerForwardDirection, wallRunNormal);
            //Debug.DrawLine(transform.position, directionParallelToWall.normalized*3, Color.red);
            // Calculate the angle difference between the player's forward direction and the direction parallel to the wall
            angleDifference = Vector3.Angle(playerForwardDirection, directionParallelToWall);

            // Print the angle difference
            //Debug.Log("Angle difference: " + angleDifference);


            wallRunTimer -= Time.deltaTime;
            // Calculate the direction parallel to the wall
            Vector3 wallParallelDirection = Vector3.ProjectOnPlane(transform.forward, wallRunNormal).normalized;
            // Calculate the direction based on verticalLookAngle, clamped by wallRunMaxAngle
            Vector3 crossDirection = Vector3.Cross(wallRunNormal, wallParallelDirection);
            if (Vector3.Dot(crossDirection, transform.up) < 0) // Make sure the cross product direction is upwards
            {
                crossDirection = -crossDirection;
            }
            Vector3 verticalDirection = Mathf.Clamp(verticalLookAngle / wallRunMaxAngle, -1f, 1f) * crossDirection.normalized;
            // Combine the horizontal and vertical directions
            Vector3 wallRunDirection = (wallParallelDirection + verticalDirection).normalized;

            // Remove any movement away from the wall
            wallRunDirection = Vector3.ProjectOnPlane(wallRunDirection, wallRunNormal).normalized;

            // Set the player's velocity
            rb.velocity = wallRunDirection * moveSpeed * currentSpeedMultiplier;


            
            //Debug.Log(angleDifference);
            
        }
        //Jumped off wallrun
        if ((Input.GetButton("Jump") || (!Input.GetButton("LShift") && holdSprint) || (Input.GetAxis("Forwards") <= 0 && !holdSprint || !isTouchingWallrunnable)) && isWallRunning)
        {
            Debug.Log("Jumped off wallrun");
            wallRunCooldown = 1f;
            EndWallRun();
        }
        //Fell off wallrun

        if ((angleDifference > maxLookAwayAngleForWallrun ||  wallRunTimer <= 0) && isWallRunning)
        {
            Debug.Log("Fell off wallrun");
            EndWallRun();
        }
    }



    // End wall run
    private void EndWallRun()
    {
        isWallRunning = false;
        rb.useGravity = true;
        canStartWallRun = false;
        // (Hopefully) push player away and up from wall
        Vector3 wallrunPushOff = new Vector3 (-wallRunNormal.x, jumpForce, -wallRunNormal.z);
        //Debug.DrawRay(transform.position, wallrunPushOff);
        Debug.DrawRay(transform.position, wallRunNormal);
        rb.AddForce(new Vector3(-wallRunNormal.x, jumpForce, -wallRunNormal.z), ForceMode.VelocityChange);

        StartCoroutine(WallRunCooldown());
        ResetWallRunVariables();
    }

    private void ResetWallRunVariables()
    {
        wallRunNormal = Vector3.zero;
        isTouchingWallrunnable = false;
        wallRunTimer = wallRunTime;
        isWallRunning = false;
        wallRunCooldown = 2.0f;
    }

    // Check for wallrunnable surface on collision
    void OnCollisionStay(Collision collision)
    {
        //if (collision.gameObject.tag != "Ground") Debug.Log("stay happened");
        // If the player is not currently wall running
        if (isWallRunning && collision.gameObject.tag != "Ground")
        {
            //Debug.Log("Stay is currently running with wallrun");
            // Iterate through all the contact points between the colliders
            foreach (ContactPoint contact in collision.contacts)
            {
                // If the angle between the contact normal (surface normal of the collider) and the (Vector3.up) is greater than the minimum wall angle
                if (Vector3.Angle(contact.normal, Vector3.up) > minWallAngle)
                {
                    // Store the surface normal of the current contact point for later use in wall running calculations
                    wallRunNormal = contact.normal;
                    // Exit the OnCollisionStay function
                    isTouchingWallrunnable = true;
                    //Debug.Log("wallrun normal was set to " + wallRunNormal + " in on collision stay");
                    return;
                }
            }
        }
        // If no suitable contact points were found, set isTouchingWallrunnable to false
        if (!isWallRunning)
        {
            if (canStartWallRun && collision.gameObject.tag != "Ground") isTouchingWallrunnable = true;
            else isTouchingWallrunnable = false;
        }
    }

    // Set grapple target from PlayerCamera
    public void SetGrappleTarget(Transform target)
    {
        grappleTarget = target;
    }

    // If grappling check
    public bool Grappling()
    {
        bool grapple = (grappleTarget != null) && Input.GetAxis("RClick") > 0;

        if (currentRope == null && grapple)
        {
            ropeLength = 0;
            currentRope = Instantiate(ropePrefab);

        }
        else if(currentRope != null && !grapple)
        {
            Destroy(currentRope);
        }

        
        return grapple;
    }

    // Pause coroutine
    IEnumerator PauseForTime(float timePauseSeconds)
    {
        yield return new WaitForSeconds(timePauseSeconds);
    }

    private IEnumerator WallRunCooldown()
    {
        yield return new WaitForSeconds(wallRunCooldown);
        canStartWallRun = true;
    }

    void OnTriggerEnter(Collider other)
    {
        //Debug.Log("Trigger enter");
        if (other.gameObject.CompareTag("Ground"))
        {
            UpdateIsGrounded(true);
        }
    }

    // OnTriggerExit method to check if the trigger collider is no longer touching the ground
    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Ground"))
        {
            UpdateIsGrounded(false);
        }
    }

}
