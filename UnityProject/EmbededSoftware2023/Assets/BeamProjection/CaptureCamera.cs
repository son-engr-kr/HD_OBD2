using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaptureCamera : MonoBehaviour
{
    Camera captureCamera;
    void Start()
    {
        captureCamera = GetComponent<Camera>();

    }

    // Update is called once per frame
    void Update()
    {
        Ray ray0 = captureCamera.ViewportPointToRay(new Vector3(0, 1, 0));
        Ray ray1 = captureCamera.ViewportPointToRay(new Vector3(1, 1, 0));
        Ray ray2 = captureCamera.ViewportPointToRay(new Vector3(1, 0, 0));
        Ray ray3 = captureCamera.ViewportPointToRay(new Vector3(0, 0, 0));

        var rays = new List<Ray>() { ray0, ray1, ray2, ray3 };
        for (int jdx = 0; jdx < 4; jdx++)
        {
            //https://kukuta.tistory.com/391
            RaycastHit hitData;
            Physics.Raycast(rays[jdx], out hitData);
            var hitPoint = hitData.point;

            Debug.DrawRay(rays[jdx].origin, (hitPoint - rays[jdx].origin), new Color(1, 0, 0));

        }
    }
}
