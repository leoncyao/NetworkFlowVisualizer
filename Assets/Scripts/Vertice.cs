using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vertice : MonoBehaviour {
    public Vector3 Loc;
    public int LocX, LocY;
    public float Units;
    public string Name;
    public void SetLoc(int x,int y)
    {
        LocX = x;
        LocY = y;

        Loc = new Vector3(x, y);
    }
	void Start () {
        Units = MainController.Instance.Units;
    }
	
	void Update () {
		
	}
    public Vector3 getLoc()
    {
        return Loc;
    }

}
