using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AngleLine : MonoBehaviour
{

    public LineRenderer lineRenderer;
    public Transform trans1;
    public Transform trans2;
    // Start is called before the first frame update
    void Start()
    {
        lineRenderer.positionCount = 2;
    }

    // Update is called once per frame
    void Update()
    {
        lineRenderer.SetPosition(0, trans1.position);
        lineRenderer.SetPosition(1, trans2.position);
    }
}
