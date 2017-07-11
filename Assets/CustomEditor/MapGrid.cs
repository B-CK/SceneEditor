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

        Transform _selectedObj;
        SceneView _currentView;
        Vector3 _mounseWorldPos;
        Plane _rayPlane;
        bool _isPaste = false;
        EventType _eventType = EventType.Ignore;

        Vector3 _cubeSize;
        Transform _sceneTree;
        Transform _blockNode;
        /// <summary>
        /// key - 块编号Transform.instanceID; value -  块对象数据
        /// 路径: __Scene/Bocks/
        /// </summary>
        Dictionary<int, Block> _blockObjsDic = new Dictionary<int, Block>();
        class Block
        {
            public Transform obj;
            public int x;
            public int z;
        }


        //=====>>>后期优化功能
        //对场景上的类容进行管理
        //对象名均添加格子编号,编号重复则删除

        void Awake()
        {
            _window = BuildingBlocksWindow.window;
            _cubeSize = _window._cubeSize;

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
            InitTree();

            _rayPlane = new Plane(Vector3.up * _origin.y, -_origin.y);

            Selection.selectionChanged += OnSelected;
            SceneView.onSceneGUIDelegate += OnSceneItemDrag;
        }
        void InitTree()
        {
            GameObject sceneTree = GameObject.Find("/__Scene");
            _sceneTree = sceneTree == null ? new GameObject("__Scene").transform : sceneTree.transform;
            GameObject blockNode = GameObject.Find("/__Scene/Bocks");
            _blockNode = blockNode == null ? new GameObject("Bocks").transform : blockNode.transform;
            if (_blockNode != null)
                UtiliTool.AddChildToZero(_sceneTree.transform, _blockNode.transform);

            foreach (Transform child in _blockNode)
            {
                int x = 0, z = 0;
                Vector3 worldPos = child.position;
                CalcGridPos(worldPos, out x, out z);
                Block block = new Block();
                block.obj = child;
                block.x = x;
                block.z = z;
                Debug.Log(x.ToString() + "   " + z);
                int instanceID = child.GetInstanceID();
                _blockObjsDic.Add(instanceID, block);
            }
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
            _selectedObj = Selection.activeTransform;
            if (SelectedFilter()) return;
            //Debug.Log(_selectedObj.name);
            if (_selectedObj != _sceneTree && _selectedObj.parent != _blockNode)
            {
                _selectedObj.parent = _blockNode;
            }
        }
        void OnSceneItemDrag(SceneView view)
        {
            Event current = Event.current;
            if (current.type == EventType.Layout || current.type == EventType.Repaint || current.type == EventType.used) return;
            Vector3 direction = view.camera.transform.forward;
            Vector3 position = view.camera.transform.position;
            Ray ray = HandleUtility.GUIPointToWorldRay(current.mousePosition);
            float distance;
            bool res = _rayPlane.Raycast(ray, out distance);
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
            if (SelectedFilter()) return;

            switch (_eventType)
            {
                case EventType.MouseDrag:
                case EventType.MouseDown:
                    {
                        int nox = 0, noz = 0;
                        Vector3 blockPos = CalcGridPos(_mounseWorldPos, out nox, out noz);
                        int instanceID = _selectedObj.GetInstanceID();
                        Block block = null;
                        if (_blockObjsDic.ContainsKey(instanceID))
                        {
                            block = _blockObjsDic[instanceID];
                        }
                        else
                        {
                            block = new Block();
                            block.obj = _selectedObj;
                            _blockObjsDic.Add(instanceID, block);
                        }
                        block.x = nox;
                        block.z = noz;
                        Debug.Log("===>" + nox + "   " + noz);
                        block.obj.transform.position = blockPos;
                        //Debug.Log("InstanceID: " + instanceID + "     " + _selectedObj.transform.position);
                        break;
                    }
                case EventType.Ignore: break;
            }
            _eventType = EventType.Ignore;
        }
        bool SelectedFilter()
        {
            if (_selectedObj == null) return true;
            if (_selectedObj == transform) return true;

            return false;
        }
        Vector3 CalcGridPos(Vector3 worldPos, out int nox, out int noy)
        {
            Vector3 size = _window._cubeSize;
            Vector3 delta = worldPos - _origin;
            nox = Mathf.FloorToInt(delta.x / size.x);
            noy = Mathf.FloorToInt(delta.z / size.z);
            float x = Mathf.FloorToInt(delta.x / size.x) * size.x;
            float z = Mathf.FloorToInt(delta.z / size.z) * size.z;
            x += (delta.x - x) > (size.x / 2) ? 1 : 0;
            z += (delta.z - z) > (size.z / 2) ? 1 : 0;

            return _origin + new Vector3(x, 0, z);
        }

        /// <summary>
        /// 依据方格尺寸调整
        /// </summary>
        public void ResetBlocks()
        {
            float sizeDelta = Vector3.Distance(_cubeSize, _window._cubeSize);
            if (!(-0.005f < sizeDelta && sizeDelta < 0.005f))
            {   //场景格子尺寸改变
                _cubeSize = _window._cubeSize;
                Debug.Log(_cubeSize);
                foreach (var item in _blockObjsDic)
                {
                    Block block = item.Value;
                    Vector3 offset =  new Vector3(block.x * _cubeSize.x, 0, block.z * _cubeSize.z);
                    block.obj.transform.position = _origin + offset;
                    Debug.Log(block.obj.transform.position.ToString() + "  x:" + block.x + "   z:" + block.z);
                }
            }
            else
                Debug.Log("无修改");
        }

        //Vector3 CalcGridPos()
        //{
        //    Vector3 size = _window._cubeSize;
        //    Vector3 delta = _mounseWorldPos - _origin;
        //    float x = Mathf.FloorToInt(delta.x / size.x) * size.x;
        //    float z = Mathf.FloorToInt(delta.z / size.z) * size.z;
        //    x += (delta.x - x) > (size.x / 2) ? 1 : 0;
        //    z += (delta.z - z) > (size.z / 2) ? 1 : 0;

        //    return _origin + new Vector3(x, 0, z);
        //}

    }

}
#endif