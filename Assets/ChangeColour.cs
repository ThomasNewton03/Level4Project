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
        /*
        else if (IncorrectCollision()) {
            rend.sharedMaterial = material[2];
            Debug.Log("collision has not happened!");
        }
        */
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

    /*
    bool IncorrectCollision(){
        // change this method
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        // If ray hits the cube and it's not at the correct point
        if (Physics.Raycast(ray, out hit, 0.5f))
        {
            if (hit.collider.gameObject == colCube)
            {
                return true;
            }
        }
        return false;
    }
    */
    
}
