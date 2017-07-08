#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using UnityScript;
using System.Collections;

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
        readonly float _grap = 0.025f;

        GameObject _selectedObj;
        SceneView _currentView;

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

            transform.position = Vector3.one * 10000f;
            _window = BuildingBlocksWindow.window;

            Selection.selectionChanged += OnSelected;
        }

        public void OnSelected()
        {
            _selectedObj = Selection.activeGameObject;
            if (_selectedObj == null) return;
            //Debug.Log(_selectedObj.name);
        }
        void OnDestroy()
        {
            Selection.selectionChanged -= OnSelected;
        }
        void OnEnable()
        {
            _window = BuildingBlocksWindow.window;
        }
        void OnDrawGizmos()
        {
            if (_window == null) return;

            Vector3 size = _window._cubeSize;
            int y = _window._sceneSizeY;
            Gizmos.color = Color.green - Color.black * 0.7f;
            for (int x = 0; x < _window._sceneSizeX; x++)
            {
                for (int z = 0; z < _window._sceneSizeZ; z++)
                {
                    Vector3 center = _origin + new Vector3(x, y, z);
                    float halfW = (size.x - _grap) / 2;
                    float halfH = (size.y - _grap) / 2;

                    Vector3 one = center + new Vector3(halfW, 0, halfH);
                    Vector3 two = center + new Vector3(-halfW, 0, halfH);
                    Vector3 thr = center + new Vector3(-halfW, 0, -halfH);
                    Vector3 fou = center + new Vector3(halfW, 0, -halfH);
                    Gizmos.DrawLine(one, two);
                    Gizmos.DrawLine(two, thr);
                    Gizmos.DrawLine(thr, fou);
                    Gizmos.DrawLine(fou, one);
                }
            }
        }
        void Update()
        {
            Vector3 mpos = _currentView.camera.ScreenToWorldPoint(Input.mousePosition);
            Debug.Log(mpos);
            //if (_selectedObj == null) return;
            //Debug.Log(_selectedObj.name);
            return;
            if (_window == null || _selectedObj == null) return;

            Vector3 position = _currentView.camera.ScreenToWorldPoint(Input.mousePosition);
            position.y = _window._sceneSizeY;

            //多种个格子站位计算(1个格子)
            //TODO
            Vector3 cube = _selectedObj.transform.position;
            cube.y = _window._sceneSizeY;      



            _selectedObj.transform.position = position;
        }
        
        Vector3 GetGridPos(Vector3 pos)
        {

            return pos;
        }
    }
}
#endif