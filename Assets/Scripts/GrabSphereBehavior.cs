using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class GrabSphereBehavior : MonoBehaviour
{
    private static GameObject cylinderPrefab;


    public SteamVR_Action_Boolean GrabCylinder;
    public SteamVR_Action_Boolean PluckCylinder;
    public SteamVR_Action_Boolean CreateCylinder;
    public SteamVR_Action_Boolean ChangeColor;

    public SteamVR_Action_Vibration Haptics;

    public SteamVR_Input_Sources handType;

    public Color defaultColor;

    private CylinderBehavior grabbedObject;
    private PluckSphereBehavior pluckedObject;

    private new Renderer renderer;
    private Collider collider;
    private CylinderBehavior selection;



    // Start is called before the first frame update
    void Start()
    {
        if (cylinderPrefab == null)
            cylinderPrefab = Resources.Load("Prefabs/Cylinder") as GameObject;

        renderer = GetComponent<Renderer>();
        collider = GetComponent<Collider>();

        GrabCylinder.AddOnStateDownListener(GrabDown, handType);
        GrabCylinder.AddOnStateUpListener(GrabUp, handType);

        PluckCylinder.AddOnStateDownListener(PluckDown, handType);
        PluckCylinder.AddOnStateUpListener(PluckUp, handType);

        CreateCylinder.AddOnStateDownListener(OnCreateDestroyCylinder, handType);
        ChangeColor.AddOnStateDownListener(OnChangeColor, handType);
    }

    public void GrabDown(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        if (pluckedObject != null)
            return;

        if (selection != null)
        {
            selection.Grab(transform);
            SetGrabbing(selection);
        }
    }

    public void GrabUp(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        if (grabbedObject != null)
        {
            grabbedObject.Release(transform);
        }

        SetGrabbing(null);
    }

    private void SetGrabbing(CylinderBehavior grabbed)
    {
        grabbedObject = grabbed;
        renderer.enabled = grabbedObject == null;
    }

    private void SetPlucking(PluckSphereBehavior plucked)
    {
        pluckedObject = plucked;
        renderer.enabled = pluckedObject == null;
    }

    public void PluckDown(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        if (grabbedObject != null)
            return;

        if (selection != null)
        {
            SetPlucking(selection.Pluck(transform.position));
        }
    }

    public void PluckUp(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        if (pluckedObject != null)
        {
            pluckedObject.Release();
            SetPlucking(null);
        }
    }

    public void OnCreateDestroyCylinder(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        if (selection == null)
        {
            GameObject newObject = Instantiate(cylinderPrefab);
            newObject.transform.position = transform.position;
        }
        else
        {
            Destroy(selection.gameObject);
        }
    }

    public void OnChangeColor(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        if (selection != null)
        {
            selection.CycleColors();
        }
    }


    // Update is called once per frame
    void Update()
    {
        if (pluckedObject != null)
        {
            pluckedObject.PluckPull(transform.position);
        }
    }

    void FixedUpdate()
    {
        if (selection != null)
        {
            // Haptic feedback - really nice but can drain your controller's battery fast!
            //Haptics.Execute(0, Time.fixedDeltaTime, 160, 50, handType);
        }
    }

    void OnTriggerEnter(Collider collision)
    {
        if (grabbedObject == null)
        {
            selection = collision.gameObject.GetComponent<CylinderBehavior>();
            if (selection != null)
            {
                renderer.material.color = new Color(selection.color.r, selection.color.g, selection.color.b, defaultColor.a);
            }
        }
    }

    void OnTriggerExit(Collider collision)
    {
        if (collision.gameObject.GetComponent<CylinderBehavior>() == selection)
        {
            selection = null;
            renderer.material.color = defaultColor;
        }
    }
}
