using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JuneTestScript : MonoBehaviour
{
    public Material targetMaterial; // 찾고자 하는 마테리얼을 할당합니다.
    public GameObject cubePrefab;   // 소환할 큐브 프리팹을 할당합니다.

    void Start()
    {
    InitialRoadLight();
    }

    public void InitialRoadLight()
    {
        // 씬의 모든 렌더러를 가져옵니다.
        Renderer[] renderers = FindObjectsOfType<Renderer>();

        foreach (Renderer renderer in renderers)
        {
            // 현재 렌더러에 적용된 모든 마테리얼을 검사합니다.
            for (int i = 0; i < renderer.sharedMaterials.Length; i++)
            {
                if (renderer.sharedMaterials[i] == targetMaterial)
                {
                    // 메쉬 필터와 메쉬를 가져옵니다.
                    MeshFilter meshFilter = renderer.GetComponent<MeshFilter>();
                    if (meshFilter == null)
                    {
                        continue; // 메쉬 필터가 없으면 스킵합니다.
                    }

                    Mesh mesh = meshFilter.mesh;

                    // 해당 서브메쉬의 인덱스를 가져옵니다.
                    int[] indices = mesh.GetIndices(i);

                    // 서브메쉬의 모든 버텍스 위치에 큐브를 소환합니다.
                    foreach (int index in indices)
                    {
                        Vector3 localVertexPosition = mesh.vertices[index];
                        Vector3 worldVertexPosition = renderer.transform.TransformPoint(localVertexPosition);

                        //// 월드 좌표에 큐브 오브젝트를 소환합니다.
                        //if (cubePrefab != null)
                        //{
                        //    Instantiate(cubePrefab, worldVertexPosition, Quaternion.identity);
                        //}
                        //else
                        //{
                        //    Debug.LogError("큐브 프리팹이 할당되지 않았습니다.");
                        //}
                        // 해당 위치에 겹치는 큐브가 있는지 확인합니다.
                        Collider[] hitColliders = Physics.OverlapBox(worldVertexPosition, Vector3.one * 1 * 0.5f);
                        bool isOverlapping = false;

                        foreach (Collider hitCollider in hitColliders)
                        {
                            if (hitCollider.gameObject.CompareTag("RoadSpotLight"))
                            {
                                isOverlapping = true;
                                break;
                            }
                        }

                        // 겹치는 큐브가 없다면 새 큐브를 소환합니다.
                        if (!isOverlapping)
                        {
                            GameObject newCube = Instantiate(cubePrefab, worldVertexPosition, Quaternion.Euler(90, 0, 0));
                            newCube.tag = "RoadSpotLight";
                            newCube.transform.parent = transform;
                        }
                    }
                }
            }
        }
    }
}
