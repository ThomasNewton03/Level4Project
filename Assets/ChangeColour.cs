using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeColour : MonoBehaviour
{
    public GameObject colCube;
    public Material[] material;
    public Renderer rend;
    public Transform[] currentPoints;
    public Transform[] targetPoints;

    
    void Start (){
        rend = GetComponent<Renderer>();
        rend.enabled = true;
        rend.sharedMaterial = material[0];
    }


    void Update (){
        if (CorrectPosition()){
            rend.sharedMaterial = material[1];
            Debug.Log("collision has happened!");
        }
        else {
            rend.sharedMaterial = material[0];
            Debug.Log("collision has not happened!");
        }
    }


    bool CorrectPosition (){
        for (int i=0; i<currentPoints.Length; i++){
            if ((Vector3.Distance(currentPoints[i].position, targetPoints[i].position)) > 0.2f){
                return false;
            }
        }
        return true;   
    }
    
}
