using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeScrewColor : MonoBehaviour
{
    public GameObject colCube;
    public Material[] material;
    public Renderer rend;
    public Transform[] currentPoints;
    public Transform[] targetPoints;

    public Transform screwTransform1;
    public Transform screwTransform2;
    public Transform objectTransform1;
    public Transform objectTransform2;
    
    void Start()
    {
        rend = GetComponent<Renderer>();
        rend.enabled = true;
        rend.sharedMaterial = material[0];
    }

    
    void Update()
    {
        Vector3 direction = (objectTransform1.position - objectTransform2.position).normalized;
        Vector3 extendedEndPoint = objectTransform1.position + direction * 5f;
        Debug.DrawLine(objectTransform1.position, extendedEndPoint, Color.red);

        Vector3 direction2 = screwTransform1.position - screwTransform2.position;
        Vector3 extendedEndPoint2 = screwTransform1.position + direction2.normalized * 5f;
        Debug.DrawLine(screwTransform1.position, extendedEndPoint2, Color.green);

        if (CorrectPosition() && CorrectAlignment()){
            rend.sharedMaterial = material[1];
            Debug.Log("collision has happened!");
        }
        else {
            rend.sharedMaterial = material[0];
        }
    }

    bool CorrectPosition (){
        for (int i=0; i<currentPoints.Length; i++){
            if ((Vector3.Distance(currentPoints[i].position, targetPoints[i].position)) > 0.02f){
                return false;
            }
        }
        return true;   
    }

    bool CorrectAlignment(){
        Vector3 forwardVector = (objectTransform1.position - objectTransform2.position).normalized;

        Vector3 screwVector = (screwTransform1.position - screwTransform2.position).normalized;

        if (Mathf.Abs(Vector3.Dot(forwardVector, screwVector)) < 0.99f){
            return false;
        }
        return true;
    }


    /*
    bool CorrectAlignment (){
        Vector3 distance1 = (screwTransform1.position - objectTransform.position).normalized;
        Vector3 distance2 = (screwTransform2.position - objectTransform.position).normalized;
        float centerDistance = Vector3.Distance((screwTransform1.position + screwTransform2.position) / 2, objectTransform.position);

        if ((Vector3.Dot(distance1, distance2) < 0.9f) || (centerDistance > 0.05f)){
            return false;
        }
        return true;
    }
    */

    /*
    bool CorrectAngle (){
        Vector3 screwUpDirection = screwTransform1.up;
        Vector3 distance = (objectTransform.position - screwTransform1.position).normalized;
        float rightAngle = Vector3.Angle(screwUpDirection, distance);
    
        if (Mathf.Abs(rightAngle - 90) <= 70f){
            return true;
        }
        return false;
    }
    */
}
