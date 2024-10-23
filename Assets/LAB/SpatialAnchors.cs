using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static OVRInput;
using TMPro;

public class SpatialAnchors : MonoBehaviour
{
    //Specify controller to create Spatial Anchors
    [SerializeField] private Controller controller;
    private int count = 0;
    // Spatial Anchor Prefab
    public GameObject anchorPrefab;
    private Canvas canvas;
    private TextMeshProUGUI idText;
    private TextMeshProUGUI positionText;

    private const float Speed = 0.4f;
    
    // Calibration Interop
    public TCP Server;
    private bool HasCalibrated = false;

    // Update is called once per frame
    void Update()
    {
        // Create Anchor when user press the index trigger on specified controller
        if(OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, controller))
        {
            CreateSpatialAnchorForController();
        }

        // Manage self position for adjustments
        Vector2 sideAxis = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, Controller.RTouch);
        Vector2 topAxis = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, Controller.LTouch);
        Vector3 translation = new Vector3(sideAxis.x, topAxis.y, sideAxis.y) * Speed * Time.deltaTime;

        //this.transform
        var parent = this.transform.Find("UnityCreated");
        foreach (Transform child in parent)
        {
            var targetTransform = child.Find("transform");
            targetTransform.Translate(translation, Space.World);
        }
        
        if (HasCalibrated == false && OVRInput.Get(OVRInput.Button.One))
        {
            Server.ShouldSendCalibrate = true;
            Debug.LogWarning("CONTROLLER: Sent calibrate request");
            HasCalibrated = true;
        }
    }
    
    public void CreateSpatialAnchorForController()
    {
        // Create anchor at Controller Position and Rotation
        GameObject anchor = Instantiate(anchorPrefab, OVRInput.GetLocalControllerPosition(controller), OVRInput.GetLocalControllerRotation(controller));
        
        canvas = anchor.GetComponentInChildren<Canvas>();
        
        // Show anchor id
        idText = canvas.gameObject.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        idText.text = "#: " + count.ToString();

        // Show anchor position
        positionText = canvas.gameObject.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
        positionText.text = anchor.transform.GetChild(0).GetChild(0).position.ToString();

        // Make the anchor become a Meta Quest Spatial Anchor
        anchor.AddComponent<OVRSpatialAnchor>();

        // Add it to manager
        var parentTransform = this.transform.Find("UnityCreated");
        // Debug.LogWarning($"CONTROLLER: parentTransform is null {parentTransform == null}");
        anchor.transform.SetParent(parentTransform);

        // Increase Id by 1
        count += 1;
    }
    
    public GameObject CreateSpatialAnchorForRealsense(Vector3 position, int id)
    {
        // Create anchor at Controller Position and Rotation
        GameObject anchor = Instantiate(anchorPrefab, position, Quaternion.identity);
        anchor.name = id.ToString();
        
        canvas = anchor.GetComponentInChildren<Canvas>();
        
        // Show anchor id
        idText = canvas.gameObject.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        idText.text = $"ID: {id}";

        // Show anchor position
        positionText = canvas.gameObject.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
        positionText.text = anchor.transform.GetChild(0).GetChild(0).position.ToString();

        // Make the anchor become a Meta Quest Spatial Anchor
        anchor.AddComponent<OVRSpatialAnchor>();

        // Add it to manager
        var parentTransform = this.transform.Find("RealsenseCreated");
        // Debug.LogWarning($"CONTROLLER: parentTransform is null {parentTransform == null}");
        anchor.transform.SetParent(parentTransform);

        return anchor;
    }
}
