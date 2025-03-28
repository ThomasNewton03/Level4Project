using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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


    public float timeTaken = 0f;
    public bool timeUp = false;

    
    void Start()
    {
        rend = GetComponent<Renderer>();
        rend.enabled = true;
        rend.sharedMaterial = material[0];
    }

    
    void Update()
    {
        Vector3 direction = (objectTransform1.position - objectTransform2.position).normalized;
        Vector3 extendedEndPoint = objectTransform1.position + direction * 3f;
        Debug.DrawLine(objectTransform1.position, extendedEndPoint, Color.red);

        Vector3 direction2 = (screwTransform1.position - objectTransform1.position).normalized;
        Vector3 extendedEndPoint2 = objectTransform1.position + direction2.normalized;
        Debug.DrawLine(objectTransform1.position, extendedEndPoint2, Color.green);

        if (CorrectPosition() && CorrectAlignment()){
            rend.sharedMaterial = material[1];
            Debug.Log("collision has happened!");
            timeTaken += Time.deltaTime;
            if (timeTaken > 2f && !timeUp){
                timeUp = true;
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
            }
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
}
