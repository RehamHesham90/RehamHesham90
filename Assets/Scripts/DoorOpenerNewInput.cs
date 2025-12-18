using UnityEngine;
using UnityEngine.InputSystem; // New Input System

public class DoorOpenerNewInput : MonoBehaviour
{
    //public HingeJoint hinge;                    // link this in Inspector (the HingeJoint on Door)
    //public float openAngle = 90f;
    // public float openSpeed = 200f;
    Animator doorOpener;
    public InputActionReference interactAction; // drag the "Interact" action (from PlayerInputActions) here
    public bool isOpen = false;
    [HideInInspector] public bool playerNearby = false;

    JointSpring spring;
    

    void Start()
    {
        doorOpener = GetComponent<Animator>();
        // if (hinge == null) hinge = GetComponent<HingeJoint>();
        // spring = hinge.spring;
    }

    void OnEnable()
    {
        if (interactAction != null) interactAction.action.Enable();
        if (interactAction != null) interactAction.action.performed += OnInteract;
    }

    void OnDisable()
    {
        if (interactAction != null) interactAction.action.performed -= OnInteract;
        if (interactAction != null) interactAction.action.Disable();
    }

    void OnInteract(InputAction.CallbackContext ctx)
    {
        print("E Pressed" + playerNearby);
        if (!playerNearby) return;
        ToggleDoor();
    }

    public void ToggleDoor()
    {
        isOpen = !isOpen;
        doorOpener.SetBool("Open", isOpen);

        //spring.targetPosition = isOpen ? openAngle : 0f;
        //spring.spring = openSpeed;
        //spring.damper = 10f;
        //hinge.spring = spring;
        //hinge.useSpring = true;
    }

    // optional helper for DoorTrigger to set proximity:
    public void SetPlayerNearby(bool val)
    {
        playerNearby = val;
    }
}
