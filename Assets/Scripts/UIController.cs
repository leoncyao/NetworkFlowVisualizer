using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class UIController : MonoBehaviour {
    float CurrentInput;
	void Start () {
        CurrentInput = 0;
	}
	void Update () {
		
	}

    public void DeleteSelectedEdge()
    {
        MainController.Instance.DeleteEdge();
    }

    public void SetText(string newText)
    {
        string text = newText;
        CurrentInput = float.Parse(text);
    }
    public void ConfirmInput()
    {
        MainController.getInput(CurrentInput);
    }
}
