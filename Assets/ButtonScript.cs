using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonScript : MonoBehaviour
{
    public Button nextTaskButton;

    void OnClick(){
        nextTaskButton.onClick.Invoke();
    }
}
