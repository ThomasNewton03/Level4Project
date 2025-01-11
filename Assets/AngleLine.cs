using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AngleLine : MonoBehaviour
{

    public LineRenderer lineRenderer;
    public Transform trans;
    // Start is called before the first frame update
    void Start()
    {
        lineRenderer.positionCount = 2;
    }

    // Update is called once per frame
    void Update()
    {
        lineRenderer.SetPosition(0, trans.position);
        lineRenderer.SetPosition(1, trans.position + Vector3.up * 3f);
    }
}
