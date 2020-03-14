using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioPointBehavior : MonoBehaviour
{
    private AudioSource audio;
    private CylinderBehavior parent;

    private Vector3 lastFramePosition;

    // Start is called before the first frame update
    void Start()
    {
        audio = GetComponent<AudioSource>();
        parent = transform.parent.GetComponent<CylinderBehavior>();

        lastFramePosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        float delta = Vector3.Distance(lastFramePosition, transform.position);
        delta = Mathf.Min(delta, 0.5f);
        audio.volume = 0.5f + delta;

        audio.pitch = parent.pitch;

        lastFramePosition = transform.position;
    }
}
