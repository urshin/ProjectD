using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JuneTestScript : MonoBehaviour
{
    public Material targetMaterial; // ã���� �ϴ� ���׸����� �Ҵ��մϴ�.
    public GameObject cubePrefab;   // ��ȯ�� ť�� �������� �Ҵ��մϴ�.

    void Start()
    {
    InitialRoadLight();
    }

    public void InitialRoadLight()
    {
        // ���� ��� �������� �����ɴϴ�.
        Renderer[] renderers = FindObjectsOfType<Renderer>();

        foreach (Renderer renderer in renderers)
        {
            // ���� �������� ����� ��� ���׸����� �˻��մϴ�.
            for (int i = 0; i < renderer.sharedMaterials.Length; i++)
            {
                if (renderer.sharedMaterials[i] == targetMaterial)
                {
                    // �޽� ���Ϳ� �޽��� �����ɴϴ�.
                    MeshFilter meshFilter = renderer.GetComponent<MeshFilter>();
                    if (meshFilter == null)
                    {
                        continue; // �޽� ���Ͱ� ������ ��ŵ�մϴ�.
                    }

                    Mesh mesh = meshFilter.mesh;

                    // �ش� ����޽��� �ε����� �����ɴϴ�.
                    int[] indices = mesh.GetIndices(i);

                    // ����޽��� ��� ���ؽ� ��ġ�� ť�긦 ��ȯ�մϴ�.
                    foreach (int index in indices)
                    {
                        Vector3 localVertexPosition = mesh.vertices[index];
                        Vector3 worldVertexPosition = renderer.transform.TransformPoint(localVertexPosition);

                        //// ���� ��ǥ�� ť�� ������Ʈ�� ��ȯ�մϴ�.
                        //if (cubePrefab != null)
                        //{
                        //    Instantiate(cubePrefab, worldVertexPosition, Quaternion.identity);
                        //}
                        //else
                        //{
                        //    Debug.LogError("ť�� �������� �Ҵ���� �ʾҽ��ϴ�.");
                        //}
                        // �ش� ��ġ�� ��ġ�� ť�갡 �ִ��� Ȯ���մϴ�.
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

                        // ��ġ�� ť�갡 ���ٸ� �� ť�긦 ��ȯ�մϴ�.
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
