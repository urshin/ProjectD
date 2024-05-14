using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parabola : MonoBehaviour
{
    public LineRenderer line;//���η���
    [Min(2)]
    public int lineCnt = 20;//���� ��

    public Vector3 p1Pos = new Vector3(0, 0, 0); // P0�������� p1 ���ϱ�
    public Vector3 p2Pos = new Vector3(0, 0, 0);// P3�������� p2 ���ϱ�

    public GameObject P0_GameObject;
    public GameObject P1_GameObject;
    public GameObject P2_GameObject;
    public GameObject P3_GameObject;
    private void OnEnable()
    {
        line = GetComponentInChildren<LineRenderer>();
    }
    void Update()
    {
        LineDraw();
    }

    public Vector3 Bezier(Vector3 P0, Vector3 P1, Vector3 P2, Vector3 P3, float t)
    {
        Vector3 M0 = Vector3.Lerp(P0, P1, t);
        Vector3 M1 = Vector3.Lerp(P1, P2, t);
        Vector3 M2 = Vector3.Lerp(P2, P3, t);

        Vector3 B0 = Vector3.Lerp(M0, M1, t);
        Vector3 B1 = Vector3.Lerp(M1, M2, t);

        return Vector3.Lerp(B0, B1, t);
    }

    //���� 4���ִ� 3�� ������ ����
    public float Bezier(float P0, float P1, float P2, float P3, float t)
    {
        return Mathf.Pow((1 - t), 3) * P0 + Mathf.Pow((1 - t), 2) * 3 * t * P1 + Mathf.Pow(t, 2) * 3 * (1 - t) * P2 +
               Mathf.Pow(t, 3) * P3;
    }



    void LineDraw()
    {
        line.positionCount = lineCnt;
        //���η����� ���μ� ����

        //P1_GameObject.transform.position = P0_GameObject.transform.position + p1Pos;
        //�����ֱ�� P1�� ����
        //P2_GameObject.transform.position = P3_GameObject.transform.position + p2Pos;
        //�����ֱ�� P2�� ����
        

        for (int i = 0; i < lineCnt; i++)
        {
            float t;
            if (i == 0)
            {
                t = 0;
            }
            else
            {
                t = (float)i / (lineCnt - 1);
            }

            Vector3 bezier = Bezier(P0_GameObject.transform.position, P1_GameObject.transform.position, P2_GameObject.transform.position,
                P3_GameObject.transform.position, t);

            line.SetPosition(i, bezier); // ������ ����
        }
    }


}
