#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using UnityScript;
using System.Collections;
using UnityEngine.EventSystems;

namespace EditorClass
{
    /// <summary>
    /// 该类伴随场景编辑器存在
    /// </summary>
    [ExecuteInEditMode]
    public class MapGrid : MonoBehaviour
    {
        BuildingBlocksWindow _window;

        readonly Vector3 _origin = Vector3.one * 2000f;

        GameObject _selectedObj;
        SceneView _currentView;
        Vector3 _mounseWorldPos;
        Plane _plane;
        bool _isPaste = false;
        EventType _eventType = EventType.Ignore;

        //=====>>>后期优化功能
        //对场景上的类容进行管理
        //对象名均添加格子编号,编号重复则删除

        void Awake()
        {
            transform.position = _origin;
            ArrayList svs = SceneView.sceneViews;
            _currentView = svs[0] as SceneView;

            GameObject go = new GameObject("_");
            go.hideFlags = HideFlags.HideAndDontSave;
            Vector3 position = transform.position;
            position += Vector3.up * 15f;
            go.transform.position = position;
            go.transform.eulerAngles = new Vector3(45f, 45f, 0);
            _currentView.AlignViewToObject(go.transform);
            DestroyImmediate(go);

            _window = BuildingBlocksWindow.window;

            Selection.selectionChanged += OnSelected;
            SceneView.onSceneGUIDelegate += OnSceneItemDrag;

            _plane = new Plane(Vector3.up * _origin.y, -_origin.y);
        }
        void OnDestroy()
        {
            Selection.selectionChanged -= OnSelected;
            SceneView.onSceneGUIDelegate -= OnSceneItemDrag;
            _currentView.Repaint();
        }
        void OnEnable()
        {
            _window = BuildingBlocksWindow.window;
        }

        void OnDrawGizmos()
        {
            if (_window == null) return;

            //Gizmos.color = Color.red;
            //Gizmos.DrawSphere(_origin, 0.1f);
            Gizmos.color = Color.black;
            Gizmos.DrawSphere(_mounseWorldPos, 0.2f);
            if (_selectedObj != null)
            {
                Debug.Log("OnDrawGizmos : " + _mounseWorldPos);
            }


            Vector3 size = _window._cubeSize;
            //Gizmos.color = Color.green - Color.black * 0.7f;
            for (int x = 0; x < _window._sceneSizeX; x++)
            {
                for (int z = 0; z < _window._sceneSizeZ; z++)
                {
                    Vector3 center = _origin + new Vector3(x * size.x, 0, z * size.z);
                    float halfX = (size.x) / 2;
                    float halfZ = (size.z) / 2;

                    Gizmos.color = Color.red;
                    Gizmos.DrawSphere(center, 0.1f);
                    Gizmos.color = Color.green - Color.black * 0.7f;

                    Vector3 p1 = center + new Vector3(halfX, 0, halfZ);
                    Vector3 p2 = center + new Vector3(-halfX, 0, halfZ);
                    Vector3 p3 = center + new Vector3(-halfX, 0, -halfZ);
                    Vector3 p4 = center + new Vector3(halfX, 0, -halfZ);
                    Gizmos.DrawLine(p1, p2);
                    Gizmos.DrawLine(p2, p3);
                    Gizmos.DrawLine(p3, p4);
                    Gizmos.DrawLine(p4, p1);
                }
            }
        }
        void OnSelected()
        {
            _selectedObj = Selection.activeGameObject;
            //if (_selectedObj == null) return;
            //Debug.Log(_selectedObj.name);
        }
        void OnSceneItemDrag(SceneView view)
        {
            Event current = Event.current;
            if (current.type == EventType.Layout || current.type == EventType.Repaint || current.type == EventType.used) return;
            Vector3 direction = view.camera.transform.forward;
            Vector3 position = view.camera.transform.position;
            Ray ray = HandleUtility.GUIPointToWorldRay(current.mousePosition);
            float distance;
            bool res = _plane.Raycast(ray, out distance);
            if (res)
            {
                switch (current.type)
                {
                    case EventType.MouseDrag://以格子为单位拖动    
                    case EventType.MouseDown://放置粘贴对象
                        {
                            _mounseWorldPos = ray.GetPoint(distance);
                            if (_selectedObj != null && current.button == 0)
                                _eventType = current.type;
                            
                            break;
                        }
                    //case EventType.ExecuteCommand:
                    //    {
                    //        switch (current.commandName)
                    //        {
                    //            case "Paste"://小黑球所在区粘贴
                    //            case "Duplicate"://须指定坐标
                    //                _mounseWorldPos = ray.GetPoint(distance);
                    //                _eventType = EventType.MouseMove;
                    //                _isPaste = true;
                    //                break;
                    //        }
                    //        break;
                    //    }
                    default: _eventType = EventType.Ignore; break;
                }
            }
        }
        void Update()
        {
            if (_selectedObj == null) return;

            switch (_eventType)
            {
                case EventType.MouseDrag:
                case EventType.MouseDown:
                    _selectedObj.transform.position = CalcGridPos();
                    break;
                case EventType.Ignore: break;
            }
            _eventType = EventType.Ignore;
        }
        Vector3 CalcGridPos()
        {
            Vector3 size = _window._cubeSize;
            Vector3 delta = _mounseWorldPos - _origin;
            float x = Mathf.FloorToInt(delta.x / size.x) * size.x;
            float z = Mathf.FloorToInt(delta.z / size.z) * size.z;
            x += (delta.x - x) > (size.x / 2) ? 1 : 0;
            z += (delta.z - z) > (size.z / 2) ? 1 : 0;

            return _origin + new Vector3(x, 0, z);
        }
    }
}
#endif