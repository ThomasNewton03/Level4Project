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
    public Transform objectTransform;
    
    void Start()
    {
        rend = GetComponent<Renderer>();
        rend.enabled = true;
        rend.sharedMaterial = material[0];
    }

    
    void Update()
    {
        Debug.Log(CorrectAlignment());

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

    bool CorrectAlignment (){
        Vector3 distance1 = (screwTransform1.position - objectTransform.position).normalized;
        Vector3 distance2 = (screwTransform2.position - objectTransform.position).normalized;
        if (Vector3.Dot(distance1, distance2) < 0.9f){
            return false;
        }
        return true;
    }
}
