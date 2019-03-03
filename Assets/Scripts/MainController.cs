using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

//Notes: STRINGS ARE MUTABLEEE i think? wait i didnt check, THEY are concatable
public class MainController : MonoBehaviour {

    public static MainController Instance;
    public float Units;

    public GameObject VerticePrefab, EdgePrefab, PersonPrefab;

    // Convenient to keep track of gameobjects and vertices seperately
    public List<GameObject> VerticeList, EdgeList;
    public static Dictionary<string, Vertice> VerticeDict;
    
    // Edge Properties
    // Keeps track of the speed of the edges, which can change based on user input
    public static Dictionary<string, float> Speeds;
    // Keeps track of time it takes the edges (which is the length of the edge divided by its speed)
    public static Dictionary<string, float> Times;

    //i think storing endpoints as strings is easier. though i can only have 26 damn
    public static Dictionary<string, string> AdjacencyList;


    public static List<GameObject> PersonList;
    public static int LastPersonCount;

    public int MainLength, MainWidth;
    public int SelectionX, SelectionY;

    public GameObject SelectedEdgeMesh;
    public Vertice SelectedVertice1,SelectedVertice2;
    public Edge SelectedEdge;

    public Material SelectedVertexMat, SelectedEdgeMat, PreviousVertexMat,PreviousEdgeMat;

    public static string StartPoint, EndPoint;

    public static float inputSpeed;
    public int framecount;
    public int SpawnTimer;
    public float defaultspeed;
    public int MaxPeople = 10;
    void Start () {
        defaultspeed = 5;
        SpawnTimer = 100;
        LastPersonCount = 1;
        framecount = 0;
        Units = 2.0f;

        Instance = this;

        // Start and End
        StartPoint = "A";
        EndPoint = "I";

        // For varying the size of the squares later
        //MainLength = 5f;
        //MainWidth = 5f;
        MainLength = 3;
        MainWidth= 3;


        Vector3 CameraPosition = new Vector3(MainLength/5f*2f * Units, MainLength / 5f * 2f * Units, -2.5f * Units);
        Camera.main.transform.position = CameraPosition;

        VerticeDict = new Dictionary<string, Vertice>();
        VerticeList = new List<GameObject>();
        EdgeList = new List<GameObject>();
        PersonList = new List<GameObject>();
        Speeds = new Dictionary<string, float>();
        Times = new Dictionary<string, float>();
        AdjacencyList = new Dictionary<string,string>();

        SelectionX = -1;
        SelectionY = -1;

        SelectedVertice1 = null;
        int t = 0;
        string random = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
		for(int i = 0; i < MainLength; i++)
        {
            for (int j = 0; j < MainWidth; j++)
            {
                Vector3 Loc = new Vector3(i*Units, j* Units);
                GameObject temp = Instantiate(VerticePrefab, Loc, Quaternion.Euler(0,0,0));
                VerticeList.Add(temp);
                Vertice TempVertice = temp.GetComponent<Vertice>();
                TempVertice.SetLoc(i, j);
                TempVertice.Name = random[t].ToString();
                t++;
                VerticeDict.Add(TempVertice.Name, TempVertice);
            }
        }
        foreach(string key in VerticeDict.Keys)
        {
            AdjacencyList[key] = "";
        }

        //MakeEdge(VerticeDict["A"], VerticeDict["I"]);

    }
    void GetSelection()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;

            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 25.0f, LayerMask.GetMask("UI"), QueryTriggerInteraction.Collide))
            {
            }
            else if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 25.0f, LayerMask.GetMask("Vertices"), QueryTriggerInteraction.Collide))
            {
                //if (SelectedEdgeMesh)
                //{
                //    SelectedEdgeMesh.GetComponent<MeshRenderer>().material = PreviousEdgeMat;
                //    SelectedEdgeMesh = null;
                //    SelectedEdge = null;
                //}
                Vertice Temp = hit.collider.gameObject.GetComponent<Vertice>();
                SelectionX = Temp.LocX;
                SelectionY = Temp.LocY;

                // SelectionX and Selection Y were initially -1, so checking if I didn't click on anything
                if (SelectionX == -1 || SelectionY == -1)
                {
                    print("invalid");
                    return;
                }
                if (!SelectedVertice1)
                {
                    SelectedVertice1 = Temp;
                    if (SelectedVertice1)
                    {
                        PreviousVertexMat = SelectedVertice1.GetComponent<MeshRenderer>().material;
                        SelectedVertexMat.mainTexture = PreviousVertexMat.mainTexture;
                        SelectedVertice1.GetComponent<MeshRenderer>().material = SelectedVertexMat;
                    }
                }
                else if (!SelectedVertice2)
                {
                    SelectedVertice2 = Temp;
                    if (SelectedVertice2)
                    {
                        SelectedVertexMat.mainTexture = PreviousVertexMat.mainTexture;
                        SelectedVertice2.GetComponent<MeshRenderer>().material = SelectedVertexMat;
                    }
                }
            }
            else if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 25.0f, LayerMask.GetMask("Edges"), QueryTriggerInteraction.Collide))
            {
                if (SelectedEdgeMesh)
                {
                    SelectedEdgeMesh.GetComponent<MeshRenderer>().material = PreviousEdgeMat;
                    SelectedEdgeMesh = null;
                    SelectedEdge = null;
                }
                SelectedEdgeMesh = hit.collider.gameObject;
                SelectedEdge = SelectedEdgeMesh.GetComponent<Edge>();
                PreviousEdgeMat = SelectedEdgeMesh.GetComponent<MeshRenderer>().material;
                SelectedEdgeMesh.GetComponent<MeshRenderer>().material = SelectedEdgeMat;
            }
            else if (SelectedEdgeMesh)
            {
                SelectedEdgeMesh.GetComponent<MeshRenderer>().material = PreviousEdgeMat;
                SelectedEdgeMesh = null;
                SelectedEdge = null;
            }

            SelectionX = -1;
            SelectionY = -1;
        }
    }
    public void DeleteEdge()
    {
        if (SelectedEdgeMesh)
        {
            GameObject parentOfEdge = SelectedEdgeMesh.transform.parent.gameObject;
            string SelectedEdgeName = parentOfEdge.GetComponent<Edge>().EdgeName;

            // Need to remove twice because of undirected, so stores edge (u, v) and (v, u)
            Speeds.Remove(SelectedEdgeName);
            Speeds.Remove(new string(SelectedEdgeName.ToCharArray().Reverse().ToArray()));
            Times.Remove(SelectedEdgeName);
            Times.Remove(new string(SelectedEdgeName.ToCharArray().Reverse().ToArray()));
            EdgeList.Remove(parentOfEdge);
            string word = "";
            foreach (string Name in GetAdjList().Keys)
            {
                word += Name;
            }

            //because edges are undirected i do this twice
            int otherVerticeIndex = AdjacencyList[SelectedEdgeName[0].ToString()].IndexOf(SelectedEdgeName[1].ToString());
            AdjacencyList[SelectedEdgeName[0].ToString()] = AdjacencyList[SelectedEdgeName[0].ToString()].Remove(otherVerticeIndex,1);
            otherVerticeIndex = AdjacencyList[SelectedEdgeName[1].ToString()].IndexOf(SelectedEdgeName[0].ToString());
            AdjacencyList[SelectedEdgeName[1].ToString()]= AdjacencyList[SelectedEdgeName[1].ToString()].Remove(otherVerticeIndex,1);

            word = "";
            foreach (string Name in GetAdjList().Keys)
            {
                word += Name;
            }

            Destroy(parentOfEdge);
            
        }
    }
    public void SetEdgeSpeed()
    {
        if (SelectedEdgeMesh)
        {
            GameObject parentOfEdge = SelectedEdgeMesh.transform.parent.gameObject;
            string SelectedEdgeName = parentOfEdge.GetComponent<Edge>().EdgeName;


            parentOfEdge.GetComponent<Edge>().Speed = inputSpeed;
            Speeds[SelectedEdgeName] = inputSpeed;
            Speeds[new string(SelectedEdgeName.ToCharArray().Reverse().ToArray())] = inputSpeed;

            float length = parentOfEdge.transform.localScale.x;
            Times[SelectedEdgeName] = length / inputSpeed;
            Times[new string(SelectedEdgeName.ToCharArray().Reverse().ToArray())] = length / inputSpeed;
        }
    }
    void MakeEdge(Vertice v1, Vertice v2)
    {
        //Instantiate the edge
        GameObject temp = Instantiate(EdgePrefab, v1.Loc*Units, Quaternion.Euler(0, 0, 0));
        //Add its PREFAB to a list
        EdgeList.Add(temp);

        Vector3 difference = v2.Loc - v1.Loc;

        float Angle;
        float AngleDegrees;
        Angle = Mathf.Atan(difference.y / difference.x);
        AngleDegrees = Angle*180/Mathf.PI;
        // i might want to make the y length change with the flow amount. itd be a nice touch. 
        float length = Mathf.Sqrt(Mathf.Pow(difference.x, 2) + Mathf.Pow(difference.y, 2)) * Units;
        temp.transform.localScale = new Vector3(length, 1, 1);
        //handling undefined cases of Atan. i think only the first one is necessary but not sure too lazy to test.
        if (difference.x == 0.0)
        {
            if (difference.y > 0.0)
            {
                AngleDegrees = 90;
            }
            else
            {
                AngleDegrees = -90;
            }
        }
        
        if (difference.y == 0.0)
        {
            if (difference.x > 0.0)
            {
                AngleDegrees = 0;
            }
            else
            {
                AngleDegrees = 180;
            }
        }
        
        if (difference.x < 0 && difference.y < 0)
        {
            AngleDegrees += 180;
        }else if (difference.x < 0 && difference.y > 0)
        {
            AngleDegrees += 180;
        }

        //For setting rotation in Degrees
        temp.transform.rotation = Quaternion.Euler(0, 0, AngleDegrees);

        //for routes
        Edge tempEdge = temp.GetComponent<Edge>();
        tempEdge.SetCoor(v1.Loc, v2.Loc);
        
        float speed = defaultspeed;
        tempEdge.Speed = speed;
        

        string EdgeName = v1.Name + v2.Name;
        tempEdge.EdgeName = EdgeName;
        string EdgeNameReversed = v2.Name + v1.Name;
        tempEdge.Speed = speed;
        Speeds.Add(EdgeName, speed);
        Speeds.Add(EdgeNameReversed, speed);
        float time = length / speed;
        tempEdge.Time = time;
        Times.Add(EdgeName, time);
        Times.Add(EdgeNameReversed, time);
        string key = v1.Name;
        if (!AdjacencyList.ContainsKey(key))
        {
            AdjacencyList.Add(key, v2.Name);
        }
        else
        {
            AdjacencyList[key] += v2.Name;
        }
        key = v2.Name;
        if (!AdjacencyList.ContainsKey(key))
        {
            AdjacencyList.Add(key, v1.Name);
        }
        else
        {
            AdjacencyList[key] += v1.Name;
        }

        /*
        //For graph searching
        Vector3 key = SelectedVertice1.Loc;
        if (!AdjacencyList.ContainsKey(key))
            AdjacencyList.Add(key, new List<Vector3>());
        AdjacencyList[key].Add(v2.Loc);
        */

        //might want to put this somewhere else
        //v1.GetComponent<MeshRenderer>().material = PreviousVertexMat;
        //v2.GetComponent<MeshRenderer>().material = PreviousVertexMat;
        //v1 = null;
        //v2 = null;
        SelectionX = -1;
        SelectionY = -1;
    }
    public static void DeleteAllPeople()
    {
        foreach(GameObject person in PersonList)
        {
            Destroy(person);
        }
    }
    void SpawnPerson()
    {
        GameObject temp= Instantiate(PersonPrefab, VerticeDict[StartPoint].Loc, Quaternion.Euler(0, 0, 0));
        PersonList.Add(temp);
    }
	void Update () {
        if(PersonList.Count < LastPersonCount)
        {
            SpawnPerson();
        }
        GetSelection();

        if(SelectedVertice1 && SelectedVertice2)
        {
            if (SelectedVertice1 == SelectedVertice2)
            {
                print("No Loops Please");
            }
            else
            {
                MakeEdge(SelectedVertice1, SelectedVertice2);
            }
            SelectedVertice1.GetComponent<MeshRenderer>().material = PreviousVertexMat;
            SelectedVertice2.GetComponent<MeshRenderer>().material = PreviousVertexMat;
            SelectedVertice1 = null;
            SelectedVertice2 = null;
        }
        framecount = Time.frameCount;
        if (framecount % SpawnTimer == 0 && LastPersonCount < MaxPeople)
        {
            SpawnPerson();
            LastPersonCount += 1;
        }

        // update times based on currentUsages;
        if (framecount % 10 == 0)
        {
            Dictionary<string, int> currentUsages = getCurrentUsages();
            foreach (GameObject edge in EdgeList)
            {
                Edge e = edge.GetComponent<Edge>();
                string EdgeName = e.EdgeName;
                int usage = currentUsages[EdgeName];
                if (usage > 0)
                {
                    float length = edge.transform.localScale.x;

                    float newSpeed = e.Speed / usage;

                    Speeds[EdgeName] = newSpeed;
                    Speeds[new string(EdgeName.ToCharArray().Reverse().ToArray())] = newSpeed;
                    float val = length / newSpeed;

                    Times[EdgeName] = val;
                    Times[new string(EdgeName.ToCharArray().Reverse().ToArray())] = val;
                }
            }
        }
    }
    Dictionary<string, int> getCurrentUsages()
    {
        Dictionary<string, int> currentUsages = new Dictionary<string, int>();
        foreach (GameObject edge in EdgeList)
        {
            Edge e = edge.GetComponent<Edge>();
            string name = e.getName();
            currentUsages.Add(name, 0);
        }
        foreach (GameObject person in PersonList)
        {
            Person p = person.GetComponent<Person>();
            string edge = p.getCurrentEdge();
            if (edge != "")
            {
                currentUsages[edge] += 1;
            }
        }
        return currentUsages;
    }
    public void config1()
    {
        //MakeEdge(VerticeDict[])
    }
    public static void getInput(float toChange)
    {
        inputSpeed = toChange;
        Instance.SetEdgeSpeed();
    }
    public static string getStart()
    {
        return StartPoint;
    }
    public static string getEnd()
    {
        return EndPoint;
    }
    public static List<GameObject> getPersons()
    {
        return PersonList;
    }
    public static Dictionary<string, float> GetSpeeds()
    {
        return Speeds;
    }
    public static Dictionary<string, float> GetTimes()
    {
        return Times;
    }
    public static Dictionary<string, string> GetAdjList()
    {
        return AdjacencyList;
    }
    public static Dictionary<string, Vertice> getVertDict()
    {
        return VerticeDict;
    }
}
