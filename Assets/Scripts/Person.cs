﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Person : MonoBehaviour {
    public List<string> route;
    // names of respective vertice
    public string StartLoc, CurrentLoc, EndLoc;
    public bool hasnotStarted;

    public int StopNumber;
    public float Units;
    public float SpeedConstant;
    public string CurrentEdge;
	void Start () {
        SpeedConstant = 100;
        Units = MainController.Instance.Units;
        hasnotStarted = true;
        StartLoc = MainController.getStart();
        CurrentLoc = StartLoc;
        EndLoc = MainController.getEnd();
        route = new List<string>();
    }
    //public IEnumerator MoveOverSpeed(GameObject objectToMove, Vector3 end, float speed)
    //{
    //    //look up where i got the code from
    //    // speed should be 1 unit per second
    //    while (gameObject.transform.position != end)
    //    {
    //        objectToMove.transform.position = Vector3.MoveTowards(objectToMove.transform.position, end, speed/SpeedConstant);
    //        CurrentLoc = objectToMove.transform.position;
    //        yield return new WaitForEndOfFrame();
    //    }
    //}
    void DestroyMyself()
    {
        Destroy(gameObject.GetComponent<Person>());
        MainController.PersonList.Remove(gameObject);
        Destroy(gameObject);
        route = new List<string>();
    }
    void Update () {
        Dictionary<string, Vertice> CurrentVertices = MainController.getVertDict();
        //if(gameObject.transform.position == StartLoc)
        if (gameObject.transform.position == MainController.getVertDict()[StartLoc].transform.position)
        {
            FindBestPath(CurrentLoc,EndLoc,MainController.GetTimes(),MainController.GetAdjList());
        }
        if (route.Contains(EndLoc))
        {

            Dictionary<string, float> CurrentEdges = MainController.GetSpeeds();
            float k = 0.1f;
            float distfromend = (gameObject.transform.position - CurrentVertices[EndLoc].transform.position).magnitude;
            if (distfromend < k)
            {
                DestroyMyself();
                StopNumber = 0;
            }
            else
            { 
                string tempStart = route[0];
                string tempEnd = route[1];

                float dist = (gameObject.transform.position - CurrentVertices[tempEnd].transform.position).magnitude;

                bool temp1 = (dist >= k && hasnotStarted);
                bool temp2 = (dist < k);
                if (temp1)
                {
                    CurrentEdge = tempStart + tempEnd;
                    Vector3 direction = CurrentVertices[tempEnd].transform.position - gameObject.transform.position;
                    gameObject.GetComponent<Rigidbody>().velocity = direction.normalized * CurrentEdges[CurrentEdge];
                    if (!hasnotStarted)
                    {
                        FindBestPath(CurrentLoc, EndLoc, MainController.GetTimes(), MainController.GetAdjList());
                    }
                    //if (CurrentEdges.ContainsKey(tempStart + tempEnd) && hasnotStarted)
                    //{
                    //    CurrentEdge = tempStart + tempEnd;

                    //    print("speed i should go at is " + MainController.Speeds[CurrentEdge]);
                    //    print("i am currently at " + gameObject.transform.position);
                    //    print("i want to go to " + CurrentVertices[tempEnd].transform.position);
                    //    Vector3 direction = CurrentVertices[tempEnd].transform.position - gameObject.transform.position;
                    //    gameObject.GetComponent<Rigidbody>().velocity = direction.normalized * CurrentEdges[CurrentEdge];
                    //    //StartCoroutine(MoveOverSpeed(gameObject, CurrentVertices[tempEnd].Loc * Units, CurrentEdges[CurrentEdge]));
                    //    hasnotStarted = false;

                    //}else
                    //{
                    //    FindBestPath(CurrentLoc, EndLoc, MainController.GetTimes(), MainController.GetAdjList());
                    //}
                }
                //WHEN YOU REACHED THE CURRENT END OF EDGE
                else if (temp2)
                {
                    if (distfromend > k)
                    {
                        //IF YOU ARE NOT AT THE END
                        hasnotStarted = true;
                        CurrentLoc = tempEnd;
                        FindBestPath(CurrentLoc, EndLoc, MainController.GetTimes(), MainController.GetAdjList());
                    }

                }
            }
        }
    }
    void FindBestPath(string startName,string EndName, Dictionary<string, float> CurrentTimes,Dictionary<string,string> CurrentAdjacencyList)
    {
        // Uses Dijkstra's Algorithm for shortest paths
        Dictionary<string, string> Parents = new Dictionary<string, string>();
        Dictionary<string, float> verticesMinDists = new Dictionary<string, float>();
        float MAXEDGEWEIGHT = 1000;
        //Initialize all vertices to infinity(1000) first
        foreach (string temp in CurrentAdjacencyList.Keys)
        {
            verticesMinDists.Add(temp, MAXEDGEWEIGHT);
        }

        Dictionary<string, float> verticesSeen = new Dictionary<string, float>();
        //pass initial vertice into verticesSeen
        string v = startName;
        Parents[v] = null;
        verticesSeen[startName] = 0;
        //loop through recursively sorta 

        while(verticesSeen.Count < verticesMinDists.Count)
        {
            //find possible edges in adjlist
            string possibleEdges = CurrentAdjacencyList[v];
            foreach(char otherVertex in possibleEdges)
            {
                string u = otherVertex.ToString();
                //see if end point of the string has already been seen (no negative weight cycles plz)
                if (!verticesSeen.ContainsKey(u))
                {
                    // if the current weight up to the start node plus the new edge
                    // is smaller the min dist to get to the other vertice
                    // update the distance
                    if (CurrentTimes[v+u] + verticesSeen[v] < verticesMinDists[u])
                    {
                        Parents[u] = v;
                        verticesMinDists[u] = CurrentTimes[v + u] + verticesSeen[v];
                    }
                }
            }

            //find min vertice. at the start they are all basically infinity
            float[] temparray = new float[verticesMinDists.Count];
            verticesMinDists.Values.CopyTo(temparray,0);
            List<float> tempList = new List<float>();
            foreach(float temp in temparray)
            {
                tempList.Add(temp);
            }
            tempList.Sort();
            float minValue = tempList[0];
            foreach(string key in verticesMinDists.Keys)
            {
                if (verticesMinDists[key] == minValue)
                {
                    v = key;
                    break;
                }
            }
            verticesSeen[v] = verticesMinDists[v];
            verticesMinDists.Remove(v);
        }
        //foreach(string key in verticesSeen.Keys)
        //{
        //    //if (verticesSeen[key] != MAXEDGEWEIGHT);
        //        //route.Add(key);
        //}
        if (verticesSeen.ContainsKey(EndName) && verticesSeen[EndName] != MAXEDGEWEIGHT)
        {
            float totalPathLength = 0;
            string test = EndName;
            route = new List<string>();

            string tempedgetoCount;
            route.Add(test);

            while (test != startName)
            {
                tempedgetoCount = test + Parents[test];
                totalPathLength += CurrentTimes[tempedgetoCount];
                
                test = Parents[test];
                route.Add(test);
            }
            route.Reverse();
        }else
        {
            //if(MainController.getPersons().Count>0)
            //MainController.DeleteAllPeople();
        }
        }
    List<string> getRoute()
    {
        return route;
    }

    public string getCurrentEdge()
    {
        return CurrentEdge;
    }


    

    
}

