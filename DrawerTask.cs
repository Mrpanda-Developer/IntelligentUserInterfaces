using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.UI;

// You can change this file
public class DrawerTask : MonoBehaviour
{
    [Header(" DO NOT CHANGE ANY PARAMETERS HERE ")]
    [Header("Setup")]
    public string taskName = "Drawers";
    public FileType expectedType = FileType.Green;
    public XRSocketInteractor[] sockets; // drag from inspector
    public int requiredCount = 4; // should normally be equal to sockets.Length

    [Header("Metrics (persist across resets)")]
    [SerializeField] private int totalInserts;   // every time something is placed in any socket
    [SerializeField] private int wrongInserts;   // placed item != expectedType
    public float ErrorRate => totalInserts > 0 ? (float)wrongInserts / totalInserts : 0f; //note: not used in final version

    public bool IsComplete { get; private set; }

    // Alternative interaction: press a button to open or close the drawer.
    private ConfigurableJoint drawerJoint;
    private Rigidbody drawerBody;
    private XRGrabInteractable drawerHandle;
    private GameRunController runController;

    private Transform referenceFrame;
    private Vector3 closedPosition;
    private Vector3 openingDirection;
    private Vector3 buttonPosition;
    private float openingDistance;

    private GameObject buttonCanvas;
    private Button toggleButton;
    private TextMeshProUGUI buttonLabel;

    private bool motorRunning;
    private float targetDistance;
    private float motorStopTime;

    void Start()
    {
        runController = FindFirstObjectByType<GameRunController>();

        // Find the sliding part of this drawer.
        foreach (var joint in GetComponentsInChildren<ConfigurableJoint>(true))
        {
            if (joint.name == "Drawer_itself")
            {
                drawerJoint = joint;
                break;
            }
        }

        if (drawerJoint == null)
        {
            Debug.LogWarning(
                $"{name}: Could not find Drawer_itself with a ConfigurableJoint.",
                this);
            return;
        }

        drawerBody = drawerJoint.GetComponent<Rigidbody>();
        drawerHandle =
            drawerJoint.GetComponentInChildren<XRGrabInteractable>(true);

        if (drawerBody == null || drawerHandle == null)
        {
            Debug.LogWarning(
                $"{name}: Could not find the drawer Rigidbody or grab handle.",
                this);
            return;
        }

        // The inspected lab prefab slides along its joint's X axis.
        if (drawerJoint.xMotion != ConfigurableJointMotion.Limited)
        {
            Debug.LogWarning(
                $"{name}: Expected a drawer joint with limited X movement.",
                this);
            return;
        }

        referenceFrame = drawerJoint.connectedBody != null
            ? drawerJoint.connectedBody.transform
            : drawerJoint.transform.parent;

        Vector3 axis = drawerJoint.transform
            .TransformDirection(drawerJoint.axis).normalized;

        Vector3 towardHandle =
            drawerHandle.transform.position - drawerBody.position;

        if (Vector3.Dot(axis, towardHandle) < 0f)
            axis = -axis;

        closedPosition = ToReferencePoint(drawerBody.position);
        openingDirection = referenceFrame != null
            ? referenceFrame.InverseTransformDirection(axis)
            : axis;

        openingDistance =
            Mathf.Min(0.25f, drawerJoint.linearLimit.limit * 0.8f);

        if (openingDistance <= 0.001f)
            return;

        buttonPosition = ToReferencePoint(
            drawerHandle.transform.position +
            Vector3.up * 0.14f +
            axis * 0.08f);

        CreateButton();
    }

    Vector3 ToReferencePoint(Vector3 worldPosition)
    {
        return referenceFrame != null
            ? referenceFrame.InverseTransformPoint(worldPosition)
            : worldPosition;
    }

    Vector3 FromReferencePoint(Vector3 localPosition)
    {
        return referenceFrame != null
            ? referenceFrame.TransformPoint(localPosition)
            : localPosition;
    }

    Vector3 OpeningAxis()
    {
        return referenceFrame != null
            ? referenceFrame.TransformDirection(openingDirection).normalized
            : openingDirection.normalized;
    }

    float CurrentOpening()
    {
        return Vector3.Dot(
            drawerBody.position - FromReferencePoint(closedPosition),
            OpeningAxis());
    }

    void CreateButton()
    {
        buttonCanvas = new GameObject(
            "Drawer Open Close Button",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(GraphicRaycaster),
            typeof(TrackedDeviceGraphicRaycaster));

        buttonCanvas.layer = 5; 

        var canvas = buttonCanvas.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = Camera.main;

        var canvasRect = buttonCanvas.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(220f, 70f);
        canvasRect.localScale = Vector3.one * 0.001f;

        var buttonObject = new GameObject(
            "Open Close",
            typeof(RectTransform),
            typeof(Image),
            typeof(Button));

        buttonObject.layer = 5;
        buttonObject.transform.SetParent(buttonCanvas.transform, false);

        var buttonRect = buttonObject.GetComponent<RectTransform>();
        buttonRect.anchorMin = Vector2.zero;
        buttonRect.anchorMax = Vector2.one;
        buttonRect.offsetMin = Vector2.zero;
        buttonRect.offsetMax = Vector2.zero;

        var background = buttonObject.GetComponent<Image>();
        background.color = new Color(0.08f, 0.35f, 0.65f, 1f);

        toggleButton = buttonObject.GetComponent<Button>();
        toggleButton.targetGraphic = background;
        toggleButton.interactable = false;
        toggleButton.onClick.AddListener(ToggleDrawer);

        var labelObject = new GameObject(
            "Label",
            typeof(RectTransform),
            typeof(TextMeshProUGUI));

        labelObject.layer = 5;
        labelObject.transform.SetParent(buttonObject.transform, false);

        var labelRect = labelObject.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = new Vector2(8f, 4f);
        labelRect.offsetMax = new Vector2(-8f, -4f);

        buttonLabel = labelObject.GetComponent<TextMeshProUGUI>();
        buttonLabel.text = "Press Start first";
        buttonLabel.fontSize = 23f;
        buttonLabel.alignment = TextAlignmentOptions.Center;
        buttonLabel.color = Color.white;
        buttonLabel.raycastTarget = false;
    }

    void Update()
    {
        if (toggleButton == null)
            return;

        bool started = runController != null && runController.Started;
        bool held = drawerHandle != null && drawerHandle.isSelected;

        // Manual grabbing always takes priority over automatic movement.
        if (!started || held)
            motorRunning = false;

        toggleButton.interactable =
            started && !held && !motorRunning && !drawerBody.isKinematic;

        if (!started)
            buttonLabel.text = "Press Start first";
        else if (held)
            buttonLabel.text = "Release handle";
        else if (motorRunning)
            buttonLabel.text = targetDistance > 0f
                ? "Opening..."
                : "Closing...";
        else
            buttonLabel.text = CurrentOpening() > openingDistance * 0.5f
                ? "Close drawer"
                : "Open drawer";
    }

    void LateUpdate()
    {
        if (buttonCanvas == null)
            return;

        buttonCanvas.transform.position =
            FromReferencePoint(buttonPosition);

        var camera = Camera.main;
        if (camera != null)
        {
            Vector3 forward =
                buttonCanvas.transform.position - camera.transform.position;

            if (forward.sqrMagnitude > 0.0001f)
            {
                buttonCanvas.transform.rotation =
                    Quaternion.LookRotation(forward, Vector3.up);
            }
        }
    }

    public void ToggleDrawer()
    {
        if (!isActiveAndEnabled ||
            drawerBody == null ||
            drawerBody.isKinematic ||
            runController == null ||
            !runController.Started ||
            drawerHandle.isSelected ||
            motorRunning)
        {
            return;
        }

        // Read the actual position, so this also works after manual dragging.
        targetDistance = CurrentOpening() > openingDistance * 0.5f
            ? 0f
            : openingDistance;

        motorRunning = true;
        motorStopTime = Time.time + 3f;
        drawerBody.WakeUp();
    }

    void FixedUpdate()
    {
        if (!motorRunning || drawerBody == null)
            return;

        if (drawerBody.isKinematic ||
            drawerHandle.isSelected ||
            Time.time >= motorStopTime)
        {
            motorRunning = false;
            return;
        }

        Vector3 axis = OpeningAxis();
        float error = targetDistance - CurrentOpening();

        Vector3 frameVelocity = drawerJoint.connectedBody != null
            ? drawerJoint.connectedBody.GetPointVelocity(drawerBody.position)
            : Vector3.zero;

        float speed = Vector3.Dot(
            drawerBody.linearVelocity - frameVelocity, axis);

        if (Mathf.Abs(error) < 0.01f && Mathf.Abs(speed) < 0.03f)
        {
            motorRunning = false;
            return;
        }

        // Apply a damped force along the existing joint.
        // Collisions and the original joint constraints remain active.
        float acceleration = Mathf.Clamp(
            error * 60f - speed * 14f, -8f, 8f);

        drawerBody.AddForce(
            axis * acceleration, ForceMode.Acceleration);
    }

    void OnDestroy()
    {
        if (buttonCanvas != null)
            Destroy(buttonCanvas);
    }

    // DO NOT CHANGE THIS METHOD
    void OnEnable()
    {
        foreach (var s in sockets)
        {
            if (!s) continue;
            s.selectEntered.AddListener(OnSocketEntered);
            s.selectExited.AddListener(OnSocketExited);
        }

        Recompute();
    }

    // DO NOT CHANGE THIS METHOD
    void OnDisable()
    {
        foreach (var s in sockets)
        {
            if (!s) continue;
            s.selectEntered.RemoveListener(OnSocketEntered);
            s.selectExited.RemoveListener(OnSocketExited);
        }
    }

    //called when something entered a socket
    void OnSocketEntered(SelectEnterEventArgs args)
    {
        // Count attempt
        totalInserts++;

        // we get the the selected item and check whether its what we expected
        var selected = args.interactableObject?.transform;
        if (selected)
        {
            var fi = selected.GetComponent<FileItem>();
            if (!fi || fi.fileType != expectedType)
                wrongInserts++;
        }

        Recompute();
    }

    // called when something exited a socket
    void OnSocketExited(SelectExitEventArgs _) => Recompute();

    // this method is called when a new item has entered or exited a socket
    void Recompute()
    {
        int matched = 0;

        // for each socket, check if we currently have a correct item in it
        foreach (var s in sockets)
        {
            // always check whether the socket is initialized
            if (s == null) continue;
            var selected = s.firstInteractableSelected;
            if (selected == null) continue;

            // check if the item in the socket is of the expected filetype
            var fi = selected.transform.GetComponent<FileItem>();
            if (fi && fi.fileType == expectedType)
                matched++;
        }

        bool nowComplete = matched >= requiredCount;

        // If the task was not yet complete, print message
        if (nowComplete && !IsComplete)
        {
            IsComplete = true;
            Debug.Log($"{taskName}: COMPLETED ({matched}/{requiredCount}) | errorRate={ErrorRate:P1}");
        }
        // If task was complete before, and not anymore
        else if (!nowComplete && IsComplete)
        {
            IsComplete = false;
            Debug.Log($"{taskName}: no longer complete ({matched}/{requiredCount})");
        }
    }

    public void ResetState()
    {
        IsComplete = false;
        Recompute(); // will recompute from empty sockets after we clear them in the controller
    }


    public (int total, int wrong, float rate) GetErrorMetrics()
        => (totalInserts, wrongInserts, ErrorRate);
}