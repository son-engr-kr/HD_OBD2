using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectionCamera : MonoBehaviour
{

    Camera projectionCamera;
    Mesh mesh;
    [SerializeField] Material projectionMaterial;
    int partial = 1000;
    //Texture2D texture = new Texture2D(1024, 1024, TextureFormat.ARGB32, false);
    //[SerializeField]Texture2D texture;
    void Start()
    {
        projectionCamera = GetComponent<Camera>();
        var meshGameObject = new GameObject("ProjectionMeshObject");
        mesh = new Mesh() { name = "ProjectionMesh" };
        meshGameObject.AddComponent<MeshFilter>().mesh = mesh;
        meshGameObject.AddComponent<MeshRenderer>();
        mesh.vertices = new Vector3[4 * partial];
        //for(int idx = 0; idx < 10; idx++)
        //{
        //    mesh.ve
        //}
        //mesh.triangles = new int[6*10] { 1, 2, 3,
        //                            0, 1, 3,  };
        int[] triangles = new int[6 * partial];
        for (int idx = 0; idx < partial; idx++)
        {
            triangles[idx * 6+0] = 4*idx + 1;
            triangles[idx * 6+1] = 4*idx + 2;
            triangles[idx * 6+2] = 4*idx + 3;
            triangles[idx * 6+3] = 4*idx + 0;
            triangles[idx * 6+4] = 4*idx + 1;
            triangles[idx * 6+5] = 4*idx + 3;
        }
        mesh.triangles = triangles;
        //texture = new Texture2D(1024, 1024, TextureFormat.ARGB32, false);
        RenderTexture renderTexture = new RenderTexture(1024, 1024, 32);
        projectionCamera.targetTexture = renderTexture;

        meshGameObject.GetComponent<Renderer>().material = projectionMaterial;
        meshGameObject.GetComponent<Renderer>().material.mainTexture = renderTexture;

    }
    public float factor;
    // Update is called once per frame
    void Update()
    {
        //The bottom-left of the screen or window is at (0, 0). The top-right of the screen or window is at (Screen.width, Screen.height).
        //Ray ray0 = projectionCamera.ScreenPointToRay(new Vector3(0,0,0));
        //Ray ray1 = projectionCamera.ScreenPointToRay(new Vector3(Screen.width, 0,0));
        //Ray ray2 = projectionCamera.ScreenPointToRay(new Vector3(Screen.width, Screen.height, 0));
        //Ray ray3 = projectionCamera.ScreenPointToRay(new Vector3(0,Screen.height,0));

        var vertices = mesh.vertices;
        for(int idx = 0; idx < partial; idx++)
        {
            Ray ray0 = projectionCamera.ViewportPointToRay(new Vector3(0, 1f/ partial * (float)(idx+1), 0));
            Ray ray1 = projectionCamera.ViewportPointToRay(new Vector3(1, 1f / partial * (float)(idx + 1), 0));
            Ray ray2 = projectionCamera.ViewportPointToRay(new Vector3(1, 1f / partial * (float)(idx + 0), 0));
            Ray ray3 = projectionCamera.ViewportPointToRay(new Vector3(0, 1f / partial * (float)(idx + 0), 0));

            var rays = new List<Ray>() { ray0, ray1, ray2, ray3 };
            for(int jdx = 0; jdx < 4; jdx++)
            {
                //https://kukuta.tistory.com/391
                RaycastHit hitData;
                Physics.Raycast(rays[jdx], out hitData);
                var hitPoint = hitData.point;
                vertices[idx * 4 + jdx] = hitPoint;


                if(idx == 0 || idx == partial - 1)
                {
                    Debug.DrawRay(rays[jdx].origin, (hitPoint - rays[jdx].origin), new Color(1, 1, 1));
                }
            }

        }
        mesh.vertices = vertices;
        mesh.RecalculateBounds();
        Vector2[] uvs = new Vector2[vertices.Length];
        float topLength = (vertices[0] - vertices[1]).magnitude;
        float bottomLength = (vertices[2] - vertices[3]).magnitude;
        float topBottomRatio = bottomLength / topLength;
        for(int idx = 0; idx < partial; idx++)
        {
            uvs[idx*4 + 0] = new Vector2(0, 1f / partial * (float)(idx + 1));
            uvs[idx*4 + 1] = new Vector2(1, 1f / partial * (float)(idx + 1));
            uvs[idx*4 + 2] = new Vector2(1, 1f / partial * (float)(idx + 0));
            uvs[idx * 4 + 3] = new Vector2(0, 1f / partial * (float)(idx + 0));
            //uvs[2] = new Vector2(0.5f + topBottomRatio / 2f, factor);
            //uvs[3] = new Vector2(0.5f - topBottomRatio /2f, factor);
        }




        mesh.uv = uvs;
    }
}
