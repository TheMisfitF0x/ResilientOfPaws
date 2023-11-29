using LetMeGo.Scripts.PathFindingAlgo;
using Plugins.RuntimePathFinding.Scripts;
using ProceduralPathFinding.PathFindingScripts.Character;
using ProceduralPathFinding.PathFindingScripts.Controller;
using UnityEngine;

public class Sample_3DPointerRaycast : MonoBehaviour
{
    [SerializeField] private PathSettingOnCharacter characterPathSetting;
    [SerializeField] private PathFindingComponent path;
    private readonly RaycastHit[] Hits = new RaycastHit[2];
    private PathFindingController.PathProcess pathProcess;
    private int _sizeHit;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            CheckRaycastPointer();
        }
    }

    private void CheckRaycastPointer()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        int layerZoneTarget = LayerMask.GetMask("Ground");

        float magnitudeRay = ray.origin.magnitude * 2;
        _sizeHit = Physics.RaycastNonAlloc(ray.origin, ray.direction, Hits, magnitudeRay, layerZoneTarget);
            
        if (_sizeHit == 0)
        {
            Debug.DrawLine(ray.origin, ray.origin + ray.direction * magnitudeRay, Color.yellow, 0.1f);
        }
        else
        {
            Debug.DrawLine(ray.origin, Hits[0].point, Color.green, 1f);
            CreateNewTargetPointToGo(Hits[0].point);
        }
    }
    
    private void CreateNewTargetPointToGo(Vector3 positionTarget)
    {
        Vector3 transformPos = transform.position;
        
        pathProcess = PathFindingController.PathProcess.WIP;
        path.CreateNewPath(characterPathSetting, transformPos, positionTarget, CallBackPathTravel);
    }

    private void CallBackPathTravel(PathFindingController.PathProcess obj)
    {
        pathProcess = obj;
    }
}
