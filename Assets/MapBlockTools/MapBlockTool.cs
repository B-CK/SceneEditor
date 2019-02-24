#if UNITY_EDITOR
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityScript;
using UnityEditorInternal;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

namespace EditorClass
{
    [InitializeOnLoad]
    public class MapBlockTool : Editor
    {
        ///  1.整体调整格子大小时 未做 y 轴偏移
        ///  2.键盘粘贴复制对象不在控制范围内
        ///  3.移动操作 - 移动操作只基于平面
        ///  4.移动操作 - 位移有偏差
        ///  5.SelectedBlock 的获取还有问题...还存在其他情况..

        static MapBlockTool()
        {
            SceneView.onSceneGUIDelegate -= OnSceneGUI;
            SceneView.onSceneGUIDelegate += OnSceneGUI;
            Selection.selectionChanged -= OnSelected;
            Selection.selectionChanged += OnSelected;
            //EditorApplication.update -= Update;
            //EditorApplication.update += Update;
            //EditorApplication.hierarchyWindowChanged -= OnHierarchyWindowChanged;
            //EditorApplication.hierarchyWindowChanged += OnHierarchyWindowChanged;

            //Debug.Log("MapBlockTool 构造函数");
        }

        public static Vector3 _mounseWorldPos;
        public static Vector3 Origin
        {
            get
            {
                float y = MapBlockWindow._floorNum * MapBlockWindow._cubeSize.y;
                return _origin + Vector3.up * y;
            }
        }

        public static void Init()
        {
            ArrayList svs = SceneView.sceneViews;
            SceneView view = svs[0] as SceneView;
            GameObject go = new GameObject("_");
            go.hideFlags = HideFlags.HideAndDontSave;
            Vector3 position = Origin;
            position += Vector3.up * 15f;
            go.transform.position = position;
            go.transform.eulerAngles = new Vector3(45f, 45f, 0);
            view.AlignViewToObject(go.transform);
            DestroyImmediate(go);

            InitPathNode();

            //_rayPlane = new Plane(Vector3.up, -Origin.y);
            _rayPlane = new Plane(Vector3.up * Origin.y, -Origin.y);
        }
        public static void Clear()
        {
            _blockObjsDic.Clear();
            _pathNode.Clear();

            GameObject sceneTree = GameObject.Find("/__Scene");
            if (sceneTree != null)
                _gridPainter = sceneTree.GetComponent<MapGridPainter>();

            DestroyImmediate(_gridPainter);

            _sceneTree = null;
            _selectedObj = null;

            Resources.UnloadUnusedAssets();
        }
        public static void UpdateBlockPos()
        {
            foreach (var item in _blockObjsDic)
            {
                Block block = item.Value;
                Vector3 offset = new Vector3(block.x * MapBlockWindow._cubeSize.x, 0, block.z * MapBlockWindow._cubeSize.z);
                block.obj.transform.position = Origin + offset;

                Scene current = SceneManager.GetActiveScene();
                EditorSceneManager.MarkSceneDirty(current);
            }
        }

        static MapGridPainter _gridPainter;

        public static bool IsInGUI(SceneView sceneView, Vector2 mousePos)
        {
            Rect position = sceneView.position;
            float barHeight = position.height - 200 - 3 * EditorGUIUtility.singleLineHeight;
            Rect barStateRect = new Rect(0, barHeight, position.width, 200 + barHeight);

            return barStateRect.Contains(mousePos);
        }

        static void OnSceneGUI(SceneView sceneView)
        {
            if (!MapBlockWindow._openEditor)
            {
                if (_gridPainter != null)
                {
                    DestroyImmediate(_gridPainter);
                    sceneView.Repaint();
                }
                return;
            }

            DrawGridArrow();
            Event current = Event.current;
            if (current.type == EventType.Layout || current.type == EventType.Repaint || current.type == EventType.used) return;

            var freshSelect = SelectedBlock;
            switch (current.type)
            {
                //case EventType.MouseDrag://以格子为单位拖动    
                case EventType.MouseMove:
                case EventType.MouseDown:
                    {
                        OnMouseMoveAndDown(sceneView, current);
                        break;
                    }
                default: _eventType = EventType.Ignore; break;
            }
        }

        static void OnMouseMoveAndDown(SceneView sceneView, Event current)
        {
            if (IsInGUI(sceneView, current.mousePosition)) return;

            Ray ray = HandleUtility.GUIPointToWorldRay(current.mousePosition);
            _rayPlane = new Plane(Vector3.up, -Origin.y);
            float distance;
            bool planeHit = _rayPlane.Raycast(ray, out distance);
            _mounseWorldPos = ray.GetPoint(distance);
            if (planeHit)
            {
                RaycastHit hit;
                bool hitRes = Physics.Raycast(ray, out hit);
                bool flag = current.type == EventType.MouseMove;
                if (MapBlockWindow._barIndex != 0 && flag)
                {
                    float r = EditorPrefs.GetFloat("Cube Painter R");
                    float g = EditorPrefs.GetFloat("Cube Painter G");
                    float b = EditorPrefs.GetFloat("Cube Painter B");
                    Color color = new Color(r, g, b, 1);
                    if (MapBlockWindow._barIndex == 0)
                    {
                        if (hitRes)
                        {
                            //删除 - 物理碰撞
                            SelectedBlock = hit.transform;
                            Vector3 deleteCenter = SelectedBlock.position;
                            _gridPainter.SetCubeGizemosData(true, deleteCenter, color);
                        }
                        else
                        {
                            SelectedBlock = null;
                            _gridPainter.SetCubeGizemosData(true, Vector3.zero, Color.clear);
                        }
                    }
                    else if (MapBlockWindow._barIndex == 1)
                    {
                        //绘制 - 非物理碰撞
                        Vector3 paintCenter = CalcGridPos(_mounseWorldPos);
                        if (hitRes)
                        {
                            //绘制 - 物理碰撞
                            Vector3 offset = CalcPhysicalOffsetPos(hit);
                            SelectedBlock = hit.transform;
                            paintCenter = SelectedBlock.position + offset;
                        }
                        _gridPainter.SetCubeGizemosData(true, paintCenter, color);
                    }
                    sceneView.Repaint();
                    return;
                }

                bool flag2 = current.type == EventType.MouseDown;
                if (MapBlockWindow._barIndex != 0 && flag2 && current.button == 0)
                {
                    if (MapBlockWindow._barIndex == 0 && SelectedBlock != null)
                    {
                        RemoveBlock(SelectedBlock);
                    }
                    else if (MapBlockWindow._barIndex == 1)
                    {
                        int nox, noz;
                        Vector3 pos = CalcGridPos(_mounseWorldPos, out nox, out noz);
                        if (hitRes && SelectedBlock != null)
                        {
                            Vector3 offset = CalcPhysicalOffsetPos(hit);
                            pos = SelectedBlock.position + offset;
                        }
                        GameObject prefab = MapBlockWindow.CurrentBlockItem.prefab;
                        Transform tran = CreateTransInNode(prefab.transform);
                        AddBlock(tran, pos, nox, noz);
                    }
                    sceneView.Repaint();
                    return;
                }

                _gridPainter.SetCubeGizemosData(false, Vector3.zero, Color.clear);
            }
        }

        static void OnSelected()
        {
            SelectedBlock = Selection.activeTransform;
        }
        static Vector3 CalcPhysicalOffsetPos(RaycastHit hit)
        {
            float x = hit.normal.x * MapBlockWindow._cubeSize.x;
            float y = hit.normal.y * MapBlockWindow._cubeSize.y;
            float z = hit.normal.z * MapBlockWindow._cubeSize.z;
            Vector3 offset = new Vector3(x, y, z);
            return offset;
        }

        ////移动拖拽功能
        //static void Update()
        //{
        //    if (!MapBlockWindow._openEditor || IsSelectedNull()) return;

        //    Selection.activeTransform = SelectedBlock;
        //    switch (_eventType)
        //    {
        //        case EventType.MouseDrag:
        //        case EventType.MouseDown:
        //            {
        //                int nox = 0, noz = 0;
        //                Vector3 blockPos = CalcGridPos(_mounseWorldPos, out nox, out noz);
        //                int id = SelectedBlock.GetInstanceID();
        //                Block block = null;
        //                if (_blockObjsDic.ContainsKey(id))
        //                    block = _blockObjsDic[id];
        //                else
        //                    block = AddBlock(SelectedBlock, blockPos, nox, noz);
        //                block.x = nox;
        //                block.z = noz;
        //                block.obj.transform.position = blockPos;
        //                break;
        //            }
        //    }
        //    _eventType = EventType.Ignore;
        //}
        /// <summary>
        /// 暂未开启此功能
        /// </summary>
        //void OnHierarchyWindowChanged()
        //{
        //    //Debug.Log(_blockObjsDic.Count);
        //    if (_blockNode == null || _blockNode.childCount == 0) return;


        //    HashSet<int> newSet = new HashSet<int>();
        //    for (int i = 0; i < _blockNode.childCount; i++)
        //    {
        //        Transform child = _blockNode.GetChild(i);
        //        int id = child.GetInstanceID();
        //        if (!newSet.Contains(id))
        //        {
        //            newSet.Add(id);
        //        }
        //    }
        //    HashSet<int> oldSet = new HashSet<int>(_blockObjsDic.Keys);
        //    if (newSet.Count > oldSet.Count && newSet.IsProperSupersetOf(oldSet))
        //    {
        //        //添加新数据 newSet.ExceptWith(oldSet);
        //        for (int i = 0; i < _blockNode.childCount; i++)
        //        {
        //            Transform child = _blockNode.GetChild(i);
        //            int id = child.GetInstanceID();
        //            if (!_blockObjsDic.ContainsKey(id))
        //            {
        //                int x = 0, z = 0;
        //                Vector3 worldPos = child.position;
        //                CalcGridPos(worldPos, out x, out z);
        //                Block block = new Block();
        //                block.obj = child;
        //                block.x = x;
        //                block.z = z;
        //                _blockObjsDic.Add(id, block);
        //            }
        //        }
        //    }
        //    else if (newSet.Count < oldSet.Count && oldSet.IsProperSupersetOf(newSet))
        //    {
        //        //移除多余数据 
        //        oldSet.ExceptWith(newSet);
        //        foreach (int id in oldSet)
        //        {
        //            if (_blockObjsDic.ContainsKey(id))
        //                _blockObjsDic.Remove(id);
        //            else
        //                Debug.Log("FreshBlockDic ==>> InstanceID 不在Dic中");
        //        }
        //    }
        //    else if (newSet.Count == oldSet.Count && newSet.Overlaps(oldSet))
        //    {
        //        //Debug.Log("集合相同 - 数量: " + _blockObjsDic.Count);
        //        return;
        //    }
        //    else
        //    {
        //        //InitTree();
        //        Debug.LogError("集合BUG... 例如:将非砖块对象移到了Blocks目录下; 新旧集合中均有彼此不包含的对象..");
        //        return;
        //    }
        //}


        #region 限定选择对象
        static Transform _selectedObj;
        static Transform SelectedBlock
        {
            /// 1.在根路径下 - 移入_blockNode下
            /// 2.在_blockNode下 - 是最顶层对象,直接返回
            /// 3.在_blockNode下 - 是子对象,返回最顶层对象
            /// 4.复制粘贴出来的对象
            get
            {
                if (IsSelectedNull()) return null;
                int id = _selectedObj.GetInstanceID();
                Selection.activeTransform = _selectedObj;
                return _selectedObj;

                //if (_blockObjsDic.ContainsKey(id))
                //{//##2
                //    Selection.activeTransform = _selectedObj;
                //    return _selectedObj;
                //}
                //else if (_selectedObj.root == _sceneTree)
                //{//##3
                //    for (int i = 0; i < 5; i++)
                //    {
                //        Transform target = _selectedObj.parent;
                //        id = target.GetInstanceID();
                //        if (_blockObjsDic.ContainsKey(id))
                //        {
                //            _selectedObj = target;
                //            Selection.activeTransform = _selectedObj;
                //            return _selectedObj;
                //        }
                //    }
                //}
                //else if (IsPathNode(_selectedObj.parent))
                //{//##4
                //    Debug.Log(_selectedObj);
                //}
                //else if (!IsPathNode(_selectedObj.parent) || _selectedObj.root != _sceneTree && _selectedObj.root == _selectedObj && _selectedObj.parent == null)
                //{//##1
                //    _selectedObj.parent = GetNode();
                //    Selection.activeTransform = _selectedObj;
                //    return _selectedObj;
                //}
                //Debug.LogError("==>> 不在判断条件之内 " + Selection.activeTransform);
                //Selection.activeTransform = null;
                //return null;
            }
            set
            {
                _selectedObj = value;
            }
        }
        static bool IsSelectedNull()
        {
            if (_selectedObj == null ||
                _selectedObj == _sceneTree ||
                 IsPathNode(_selectedObj) ||
                _selectedObj.name == "Directional Light" ||
                _selectedObj.name == "Main Camera")
                return true;

            return false;
        }
        #endregion

        #region 绘制方块格子
        static Plane _rayPlane;
        public static Vector3 _origin = new Vector3(10, 0, 10);
        static EventType _eventType = EventType.Ignore;
        static Vector3 _oldCubeSize;
        /// <summary>
        /// key - 块编号Transform.instanceID; value -  块对象数据
        /// 路径: __Scene/Blocks/
        /// </summary>
        static Dictionary<int, Block> _blockObjsDic = new Dictionary<int, Block>();
        /// <summary>
        /// 场景实例对象
        /// </summary>
        class Block
        {
            public Transform obj;
            public int x;
            public int y;
            public int z;
        }
        static Block AddBlock(Transform tran, Vector3 pos, int x, int z)
        {
            int id = tran.GetInstanceID();
            Block block = new Block();
            if (!_blockObjsDic.ContainsKey(id))
            {
                tran.position = pos;
                block.obj = tran;
                block.x = x;
                block.z = z;
                _blockObjsDic.Add(id, block);

                Scene current = SceneManager.GetActiveScene();
                EditorSceneManager.MarkSceneDirty(current);
            }
            else
                Debug.Log("已存在此Block");

            return block;
        }
        static void RemoveBlock(Transform tran)
        {
            int id = tran.GetInstanceID();
            if (_blockObjsDic.ContainsKey(id))
            {
                _blockObjsDic.Remove(id);
                DestroyImmediate(tran.gameObject);

                Scene current = SceneManager.GetActiveScene();
                EditorSceneManager.MarkSceneDirty(current);
            }
            SelectedBlock = null;
        }
        static Vector3 CalcGridPos(Vector3 worldPos, out int nox, out int noz)
        {
            Vector3 size = MapBlockWindow._cubeSize;
            Vector3 delta = worldPos - Origin;
            nox = Mathf.FloorToInt(delta.x / size.x);
            noz = Mathf.FloorToInt(delta.z / size.z);

            float x = nox * size.x;
            float z = noz * size.z;
            float deltax = delta.x - x;
            float deltaz = delta.z - z;
            float radius = size.x / 2;

            x += deltax > radius ? size.x : 0;
            z += deltaz > radius ? size.z : 0;

            nox += Mathf.Abs(deltax) > radius ? 1 : 0;
            noz += Mathf.Abs(deltaz) > radius ? 1 : 0;

            return Origin + new Vector3(x, 0, z);
        }
        static Vector3 CalcGridPos(Vector3 worldPos)
        {
            Vector3 size = MapBlockWindow._cubeSize;
            Vector3 delta = worldPos - Origin;
            int nox = Mathf.FloorToInt(delta.x / size.x);
            int noz = Mathf.FloorToInt(delta.z / size.z);

            float x = nox * size.x;
            float z = noz * size.z;
            float deltax = delta.x - x;
            float deltaz = delta.z - z;
            float radius = size.x / 2;

            x += deltax > radius ? size.x : 0;
            z += deltaz > radius ? size.z : 0;

            return Origin + new Vector3(x, 0, z);
        }
        static void DrawGridArrow()
        {
            Handles.color = new Color(64f / 255f, 141 / 255f, 242 / 255f, 1);
            Vector3 zArrow = _origin + Vector3.right * -MapBlockWindow._cubeSize.x;
            Handles.ArrowCap(0, zArrow, Quaternion.LookRotation(Vector3.forward), 3);

            Handles.color = new Color(242 / 255f, 66 / 255f, 33 / 255f, 1);
            Vector3 xArrow = _origin + Vector3.forward * -MapBlockWindow._cubeSize.z;
            Handles.ArrowCap(0, xArrow, Quaternion.LookRotation(Vector3.right), 3);
        }
        #endregion

        #region 资源路径管理
        static Transform _sceneTree;

        static Dictionary<string, Transform> _pathNode = new Dictionary<string, Transform>();

        static void InitPathNode()
        {
            _pathNode.Clear();
            _blockObjsDic.Clear();
            GameObject sceneTree = GameObject.Find("/__Scene");
            _sceneTree = sceneTree == null ? new GameObject("__Scene").transform : sceneTree.transform;
            _sceneTree.transform.position = Origin;
            string winPath = EditorUtiliTool.UnityPath2WinAbs(MapBlockWindow.BlocksPrefabsDir);
            List<string> dirs = new List<string>(Directory.GetDirectories(winPath));
            for (int i = 0; i < dirs.Count; i++)
            {
                string blockNodeName = Path.GetFileName(dirs[i]);
                string nodePath = string.Format("/{0}/{1}", _sceneTree.name, blockNodeName);
                GameObject nodeGo = GameObject.Find(nodePath);
                Transform nodeTran = nodeGo == null ? new GameObject(blockNodeName).transform : nodeGo.transform;
                if (!_pathNode.ContainsKey(nodeTran.name))
                    _pathNode.Add(nodeTran.name, nodeTran);

                EditorUtiliTool.AddChildToP(_sceneTree, nodeTran);

                foreach (Transform child in nodeTran)
                {
                    int x = 0, z = 0;
                    Vector3 worldPos = child.position;
                    CalcGridPos(worldPos, out x, out z);
                    Block block = new Block();
                    block.obj = child;
                    block.x = x;
                    block.z = z;
                    int instanceID = child.GetInstanceID();
                    _blockObjsDic.Add(instanceID, block);
                }
            }

            _gridPainter = _sceneTree.GetComponent<MapGridPainter>();
            if (_gridPainter == null)
                _gridPainter = _sceneTree.gameObject.AddComponent<MapGridPainter>();
            _gridPainter.enabled = true;
            _gridPainter.hideFlags = HideFlags.DontSave;
        }

        static bool IsPathNode(Transform node)
        {
            return _pathNode.ContainsKey(node.name);
        }
        static Transform CreateTransInNode(Transform prefab)
        {
            Transform node = GetNode();
            Transform obj = PrefabUtility.InstantiatePrefab(prefab.transform) as Transform;
            EditorUtiliTool.AddChildToP(node, obj);
            return obj;
        }
        static Transform GetNode()
        {
            return _pathNode[MapBlockWindow.GetBlockType()];
        }
        #endregion
    }
}
#endif