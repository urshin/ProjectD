using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parabola : MonoBehaviour
{
    public LineRenderer line;//라인렌더
    [Min(2)]
    public int lineCnt = 20;//라인 수

    public Vector3 p1Pos = new Vector3(0, 0, 0); // P0기준으로 p1 더하기
    public Vector3 p2Pos = new Vector3(0, 0, 0);// P3기준으로 p2 더하기

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

    //점이 4개있는 3차 베지어 공식
    public float Bezier(float P0, float P1, float P2, float P3, float t)
    {
        return Mathf.Pow((1 - t), 3) * P0 + Mathf.Pow((1 - t), 2) * 3 * t * P1 + Mathf.Pow(t, 2) * 3 * (1 - t) * P2 +
               Mathf.Pow(t, 3) * P3;
    }



    void LineDraw()
    {
        line.positionCount = lineCnt;
        //라인렌더러 라인수 설정

        //P1_GameObject.transform.position = P0_GameObject.transform.position + p1Pos;
        //보여주기식 P1를 설정
        //P2_GameObject.transform.position = P3_GameObject.transform.position + p2Pos;
        //보여주기식 P2를 설정
        

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

            line.SetPosition(i, bezier); // 라인을 설정
        }
    }


}
