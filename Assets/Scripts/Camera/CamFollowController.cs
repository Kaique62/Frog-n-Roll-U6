using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraFollowPoints : MonoBehaviour
{
    public Transform cameraPointsParent;  
    private CinemachineVirtualCamera virtualCamera;
    private List<CameraPointController> cameraPoints = new List<CameraPointController>();
    private int currentPointIndex = 0;
    
    void Start()
    {
        virtualCamera = GetComponent<CinemachineVirtualCamera>();
        LoadCameraPoints();
        
        if (cameraPoints.Count > 0)
        {
            StartCoroutine(MoveToNextPoint());
        }
    }

    void LoadCameraPoints()
    {
        if (cameraPointsParent == null) return;

        cameraPoints.Clear();
        foreach (Transform child in cameraPointsParent)
        {
            if (child.name.StartsWith("CameraPoint"))
            {
                CameraPointController pointController = child.GetComponent<CameraPointController>();
                if (pointController != null)
                {
                    cameraPoints.Add(pointController);
                }
            }
        }

        cameraPoints.Sort((a, b) => ExtractNumber(a.name).CompareTo(ExtractNumber(b.name)));
    }

    int ExtractNumber(string name)
    {
        string number = System.Text.RegularExpressions.Regex.Match(name, "\\d+").Value;
        return int.TryParse(number, out int result) ? result : int.MaxValue;
    }

    IEnumerator MoveToNextPoint()
    {
        while (currentPointIndex < cameraPoints.Count)
        {
            CameraPointController targetPoint = cameraPoints[currentPointIndex];
            float speed = targetPoint.moveSpeed;
            Vector3 targetPosition = new Vector3(targetPoint.transform.position.x, targetPoint.transform.position.y, transform.position.z);

            // Aguarda o tempo de espera antes de começar a se mover
            yield return new WaitForSeconds(targetPoint.waitTime);

            while (Vector3.Distance(transform.position, targetPosition) > 0.05f)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
                yield return null;
            }

            currentPointIndex++;
        }
    }
}
