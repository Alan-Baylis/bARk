﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Database;
using Firebase.Unity.Editor;
using System.Threading.Tasks;
using System;

public class DatabaseHandler : MonoBehaviour
{
    public delegate void DatabaseEvent(ARTree tree);
    public static event DatabaseEvent NewTreeAdded;
    public static event DatabaseEvent NewTreeToRemove;

    public Material leafMat;

    DatabaseReference rootRef;
    DatabaseReference treeRef;
    List<ARTree> allTrees;

    bool treesLoaded = false;
    DatabaseReference treeEvent;

	void Awake()
    {
        FirebaseApp.DefaultInstance.SetEditorDatabaseUrl("https://bark-11fd5.firebaseio.com/");
        rootRef = FirebaseDatabase.DefaultInstance.RootReference;
        treeRef = rootRef.Child("Trees");

        // Listen for events 
        treeEvent = FirebaseDatabase.DefaultInstance.GetReference("Trees");
        treeEvent.ChildAdded += NewTree; // This will trigger at startup on childs already existing in database 
        treeEvent.ChildRemoved += TreeRemove;
    }

    private void NewTree(object sender, ChildChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }
        DataSnapshot snap = args.Snapshot;
        ARTree tree = new ARTree(snap);
        NewTreeAdded(tree);
    }

    private void TreeRemove(object sender, ChildChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }
        DataSnapshot snap = args.Snapshot;
        ARTree tree = new ARTree(snap);
        NewTreeToRemove(tree);
    }

    /// <summary>
    /// Adds a tree to firebase database
    /// </summary>
    public void AddTreeToFirebase(int seed, int maxNumVertices, int numberOfSides, float baseRadius, float radiusStep, float minimumRadius,
        float branchRoundness, float segmentLength, float twisting, float branchProbability, float growthPercent, string matName)
    {
        string key = treeRef.Push().Key;
        ARTree myTree = new ARTree(seed, maxNumVertices, numberOfSides, baseRadius, radiusStep, minimumRadius,
        branchRoundness, segmentLength, twisting, branchProbability, growthPercent, key, matName);

        myTree.ConvertToString(leafMat.mainTexture as Texture2D);
        Dictionary<string, object> entryVal = myTree.ToDictionary();

        Dictionary<string, object> childUpdate = new Dictionary<string, object>();
        childUpdate["/Trees/" + key] = entryVal;
        treeRef.Parent.UpdateChildrenAsync(childUpdate);
    }

    void OnDestroy()
    {
        treeEvent.ChildAdded -= NewTree;
        treeEvent.ChildRemoved -= TreeRemove;
    }
}
