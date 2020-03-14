using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PluckSphereBehavior : MonoBehaviour
{
    public float vibratingRate = 500;
    public float dampeningRate = 0.01f;

    private Transform parent;
    private CylinderBehavior source;

    private AudioSource audio;

    private Vector3 localAnchor;
    private Vector3 localInitial;

    // Start is called before the first frame update
    void Start()
    {
        audio = GetComponent<AudioSource>();
    }

    public void Initialize(Transform source, Vector3 anchor)
    {
        parent = source;
        this.source = source.GetComponent<CylinderBehavior>();
        localAnchor = parent.InverseTransformPoint(anchor);
    }

    public void PluckPull(Vector3 position)
    {
        transform.position = position;
    }

    public void Release()
    {
        localInitial = parent.InverseTransformPoint(transform.position);
        audio.Play();

        StartCoroutine(Vibrate());
    }

    // Update is called once per frame
    void Update()
    {
        audio.pitch = source.pitch;
    }

    private IEnumerator Vibrate()
    {
        float distance = Vector3.Distance(parent.TransformPoint(localAnchor), transform.position);
        distance = Mathf.Min(distance, 0.2f);

        float limit = 1;
        float pos = 0;
        float t;

        float localRate = dampeningRate * (distance * 10);

        while (limit > 0)
        {
            if (parent == null)
                break;

            t = Mathf.Cos(pos);
            transform.position = parent.TransformPoint(Vector3.LerpUnclamped(localAnchor, localInitial, t * limit));

            float delta = Time.deltaTime;
            pos += delta * vibratingRate;
            limit -= delta / localRate;

            if (pos >= Mathf.PI * 2)
                pos = pos - (Mathf.PI * 2);

            audio.volume = limit;
            yield return null;
        }

        Destroy(gameObject);
    }
}
