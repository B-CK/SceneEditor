#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EditorClass
{
    public class MapGridPainter : MonoBehaviour
    {
        void OnDrawGizmos()
        {
            if (!MapBlockWindow._showGrid) return;

            DrawCubeGizemos();

            Gizmos.color = Color.black;
            Gizmos.DrawSphere(MapBlockTool._mounseWorldPos, 0.1f);
            Gizmos.DrawSphere(MapBlockTool.Origin, 0.2f);

            for (int x = 0; x < MapBlockWindow._sceneSizeX; x++)
            {
                for (int z = 0; z < MapBlockWindow._sceneSizeZ; z++)
                {
                    Vector3 center = MapBlockTool.Origin + new Vector3(x * MapBlockWindow._cubeSize.x, 0, z * MapBlockWindow._cubeSize.z);
                    float halfX = (MapBlockWindow._cubeSize.x) / 2;
                    float halfZ = (MapBlockWindow._cubeSize.z) / 2;

                    Gizmos.color = Color.red;
                    Gizmos.DrawSphere(center, 0.05f);
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



        bool _isCubeGizemos = false;
        Vector3 _cubeGizemosPos = Vector3.zero;
        Color _cubeColor = Color.clear;
        void DrawCubeGizemos()
        {
            if (!_isCubeGizemos) return;

            Gizmos.color = _cubeColor;

            Vector3 center = _cubeGizemosPos;
            float halfX = (MapBlockWindow._cubeSize.x) / 2;
            float halfZ = (MapBlockWindow._cubeSize.z) / 2;
            float y = MapBlockWindow._cubeSize.y;

            Vector3 p1 = center + new Vector3(halfX, 0, halfZ);
            Vector3 p2 = center + new Vector3(-halfX, 0, halfZ);
            Vector3 p3 = center + new Vector3(-halfX, 0, -halfZ);
            Vector3 p4 = center + new Vector3(halfX, 0, -halfZ);

            Vector3 p5 = center + new Vector3(halfX, y, halfZ);
            Vector3 p6 = center + new Vector3(-halfX, y, halfZ);
            Vector3 p7 = center + new Vector3(-halfX, y, -halfZ);
            Vector3 p8 = center + new Vector3(halfX, y, -halfZ);

            Gizmos.DrawLine(p1, p2);
            Gizmos.DrawLine(p2, p3);
            Gizmos.DrawLine(p3, p4);
            Gizmos.DrawLine(p4, p1);

            Gizmos.DrawLine(p1, p5);
            Gizmos.DrawLine(p2, p6);
            Gizmos.DrawLine(p3, p7);
            Gizmos.DrawLine(p4, p8);

            Gizmos.DrawLine(p5, p6);
            Gizmos.DrawLine(p6, p7);
            Gizmos.DrawLine(p7, p8);
            Gizmos.DrawLine(p8, p5);

            Gizmos.DrawSphere(p1, 0.075f);
            Gizmos.DrawSphere(p2, 0.075f);
            Gizmos.DrawSphere(p3, 0.075f);
            Gizmos.DrawSphere(p4, 0.075f);
            Gizmos.DrawSphere(p5, 0.075f);
            Gizmos.DrawSphere(p6, 0.075f);
            Gizmos.DrawSphere(p7, 0.075f);
            Gizmos.DrawSphere(p8, 0.075f);
        }
        public void SetCubeGizemosData(bool isShow, Vector3 cubePos, Color cubeColor)
        {
            _isCubeGizemos = isShow;
            _cubeGizemosPos = cubePos;
            _cubeColor = cubeColor;
        }
    }
}
#endif