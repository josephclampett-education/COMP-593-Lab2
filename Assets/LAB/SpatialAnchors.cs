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

    private const float Speed = 1;

    // Update is called once per frame
    void Update()
    {
        // Create Anchor when user press the index trigger on specified controller
        if(OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, controller))
        {
            CreateSpatialAnchor();
        }

        // Manage self position for adjustments
        Vector2 axis = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, controller);
        this.transform.Translate(new Vector3(axis.x, 0, axis.y) * Speed * Time.deltaTime, Space.World);
    }
    
    public void CreateSpatialAnchor()
    {
        // Create anchor at Controller Position and Rotation
        GameObject anchor = Instantiate(anchorPrefab, OVRInput.GetLocalControllerPosition(controller)
                                            , OVRInput.GetLocalControllerRotation(controller));
        
        canvas = anchor.GetComponentInChildren<Canvas>();
        
        // Show anchor id
        idText = canvas.gameObject.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        idText.text = "ID: " + count.ToString();

        // Show anchor position
        positionText = canvas.gameObject.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
        positionText.text = anchor.transform.GetChild(0).GetChild(0).position.ToString();

        // Make the anchor become a Meta Quest Spatial Anchor
        anchor.AddComponent<OVRSpatialAnchor>();

        // Add it to manager
        anchor.transform.SetParent(this.transform);

        // Increase Id by 1
        count += 1;
    }
}
