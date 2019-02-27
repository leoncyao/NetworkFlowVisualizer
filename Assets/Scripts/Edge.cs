using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Edge : MonoBehaviour {
    public Vector3 start, end;

    public float Speed;
    public float Time;
    public GameObject TimeText;
    public List<GameObject> TimeTexts;
    public string EdgeName;
	void Start () {
        TimeTexts = new List<GameObject>();
        Vector3 temp = transform.position;
        GameObject go = Instantiate<GameObject>(TimeText, temp, new Quaternion(0,0,0,0));
        go.transform.rotation = transform.rotation;

        //go.transform.localScale = new Vector3(1, 1, 1);
        //temp.transform.position = transform.position + new Vector3(0f, 0f, 0);

        TextMesh temptext = go.GetComponent<TextMesh>();
        temptext.text = (Time*MainController.Instance.Units).ToString();
        TimeTexts.Add(go);
    }
    void onCollisionEnter()
    {
    }
    public void deleteText()
    {
        foreach(GameObject go in TimeTexts)
        {
            Destroy(go);
        }
    }
    public void SetCoor(Vector3 input1, Vector3 input2)
    {
        start = input1;
        end = input2;
    }

    // Update is called once per frame
    void Update () {
		
	}

    public string getName()
    {
        return EdgeName;
    }


}
