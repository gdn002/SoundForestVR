using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CylinderBehavior : MonoBehaviour
{
    private static GameObject pluckSpherePrefab;
    private static List<Color> cylinderColors;

    public AudioPointBehavior upperPoint;
    public AudioPointBehavior lowerPoint;

    private Transform mainGrabPoint;
    private Transform secGrabPoint;
    private Transform defaultParent;

    private Vector3 mainLocal;
    private Vector3 secLocal;
    private float distLocal;

    public float pitch { get; private set; }

    private bool mainGrabPointIsOrigin;

    private int colorIndex;
    public Color color { get { return cylinderColors[colorIndex]; } }

    private new Renderer renderer;
    private Light[] lightSources;

    // Start is called before the first frame update
    void Start()
    {
        if (pluckSpherePrefab == null)
            pluckSpherePrefab = Resources.Load("Prefabs/PluckSphere") as GameObject;

        if (cylinderColors == null)
            CreateColorList();

        renderer = GetComponent<Renderer>();
        lightSources = GetComponentsInChildren<Light>();

        defaultParent = transform.parent;

        pitch = 1;

        colorIndex = Random.Range(0, cylinderColors.Count);

        UpdateColor();
    }

    private void CreateColorList()
    {
        cylinderColors = new List<Color>();
        cylinderColors.Add(Color.red);
        cylinderColors.Add(Color.yellow);
        cylinderColors.Add(Color.green);
        cylinderColors.Add(Color.cyan);
        cylinderColors.Add(Color.blue);
        cylinderColors.Add(Color.magenta);
    }

    private void UpdateColor()
    {
        renderer.material.color = color;
        renderer.material.SetColor("_EmissionColor", color);

        foreach (var src in lightSources)
        {
            src.color = color;
        }
    }

    // Update is called once per frame
    void Update()
    {
        pitch = CylinderLength() / 2;

        if (mainGrabPoint != null && secGrabPoint != null)
        {
            TwoHandedGrab();
        }
    }

    private void TwoHandedGrab()
    {
        // If the object has two grab points, rotation is enabled
        Vector3 origin = mainGrabPointIsOrigin ? mainGrabPoint.position : secGrabPoint.position;
        Vector3 reference = mainGrabPointIsOrigin ? secGrabPoint.position : mainGrabPoint.position;

        // Rotate so the cylinder follows the direction defined by the two grab points
        Vector3 normalized = (reference - origin).normalized;
        transform.up = normalized;

        // Scale cylinder according to relative distance between the grab points
        float distGlobal = Vector3.Distance(origin, reference);
        float scaleFactor = Mathf.Clamp(distGlobal / distLocal, 0.5f, 1.5f);

        Vector3 scale = transform.localScale;
        transform.localScale = new Vector3(scale.x, scaleFactor * 20, scale.z);

        // Translate the cylinder so the relative position of the main grab point matches
        Vector3 currentPosition = transform.TransformPoint(mainLocal);
        Vector3 translation = mainGrabPoint.position - currentPosition;
        transform.Translate(translation, Space.World);
    }

    public void Grab(Transform grabTransform)
    {
        if (mainGrabPoint == null)
        {
            // If the object is not already grabbed, set main grab point
            Vector3 grabPoint = FindGrabPoint(grabTransform.position);
            Vector3 translation = grabTransform.position - grabPoint;

            transform.Translate(translation, Space.World);
            transform.SetParent(grabTransform, true);
            mainGrabPoint = grabTransform;
            mainLocal = transform.InverseTransformPoint(grabPoint);
            mainLocal.x = 0;
            mainLocal.z = 0;
        }
        else
        {
            // Otherwise, set secondary grab point
            secGrabPoint = grabTransform;

            // We need to know which grab point is the lowest relative to the cylinder for the rotation controls
            secLocal = transform.InverseTransformPoint(secGrabPoint.position);
            secLocal.x = 0;
            secLocal.z = 0;

            mainGrabPointIsOrigin = mainLocal.y < secLocal.y;

            distLocal = Vector3.Distance(mainLocal, secLocal);
        }
    }

    public void Release(Transform releaseTransform)
    {
        if (releaseTransform == secGrabPoint)
        {
            // Secondary grab point was released, just clear it
            secGrabPoint = null;
        }
        else if (releaseTransform == mainGrabPoint)
        {
            // Main grab point was released
            if (secGrabPoint == null)
            {
                // No secondary grab point, fully release the object
                transform.SetParent(defaultParent, true);
                mainGrabPoint = null;
            }
            else
            {
                // Has a secondary grab point, it becomes the main grab point
                transform.SetParent(secGrabPoint, true);
                mainGrabPoint = secGrabPoint;
                mainLocal = secLocal;
                secGrabPoint = null;
            }
        }
    }

    public PluckSphereBehavior Pluck(Vector3 pluckPoint)
    {
        GameObject newObject = Instantiate(pluckSpherePrefab);
        Vector3 grabPoint = FindGrabPoint(pluckPoint);

        newObject.transform.position = grabPoint;
        newObject.GetComponent<Renderer>().material = renderer.material;
        PluckSphereBehavior pluckSphere = newObject.GetComponent<PluckSphereBehavior>();
        pluckSphere.Initialize(transform, grabPoint);
        return pluckSphere;
    }

    public void CycleColors()
    {
        colorIndex++;
        if (colorIndex >= cylinderColors.Count) colorIndex = 0;
        UpdateColor();
    }

    public float CylinderLength()
    {
        return Vector3.Distance(lowerPoint.transform.position, upperPoint.transform.position);
    }

    private Vector3 FindGrabPoint(Vector3 initialGrabPoint)
    {
        Vector3 vA;
        Vector3 vB;

        if (lowerPoint.transform.position.y < upperPoint.transform.position.y)
        {
            vA = lowerPoint.transform.position;
            vB = upperPoint.transform.position;
        }
        else
        {
            vA = upperPoint.transform.position;
            vB = lowerPoint.transform.position;
        }

        Vector3 v1 = initialGrabPoint - vA;
        Vector3 v2 = (vB - vA).normalized;

        float d = Vector3.Distance(vA, vB);
        float t = Vector3.Dot(v2, v1);

        if (t <= 0)
            return vA;

        if (t >= d)
            return vB;

        Vector3 v3 = v2 * t;
        Vector3 grabPoint = vA + v3;

        return grabPoint;
    }
}
