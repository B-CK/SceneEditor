using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

[ExecuteInEditMode]
public class EventTest : MonoBehaviour
{
    public GameObject selected;

    Camera cam;
    void Awake()
    {
        Selection.selectionChanged += Selected;
        cam = Camera.main;

        Object[] objs = new Object[4];
        for (int i = 0; i < 4; i++)
        {           
            objs[i] = GameObject.Find("Cube" + i);
        }
        DragAndDrop.objectReferences = objs;
    }

    Color bgColor = new Color(56, 56, 56, 0);

    void OnEnable()
    {
        //transform.position = selected.transform.position;
        //transform.eulerAngles = new Vector3(10, 30, 0);

        //cam.orthographic = true;
        //cam.backgroundColor = bgColor;
        //cam.cullingMask = 
    }

    void Update()
    {
         
    }

    public void Selected()
    {
        Object obj = Selection.activeObject;
        if (obj == null) return;
        Debug.Log(obj);
    }

}
