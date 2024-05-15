using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class AIController : MonoBehaviour
{
    [Range(-540, 540)] public float autoHandle = 0;
    [SerializeField] float frontDetectedSensitive; // �ӷ¿� ���� �� �Ÿ� ����
    [SerializeField] float frontDetectedMinSensitive; // �ּ� ����
    [SerializeField] float steeringDetectedSensitive; // �ӷ¿� ���� �� �Ÿ� ����
    [SerializeField] float brakeSensitive;
    [SerializeField] float brakeMinDistance;

    //�� ���� ���� ����
    [SerializeField] Transform frontMiddleRaySP;
    [SerializeField] Transform frontLeftRaySP;
    [SerializeField] Transform frontRightleRaySP;
    [SerializeField] Transform wallLeftRaySP;
    [SerializeField] Transform WallRightRaySP;

    [SerializeField] AnimationCurve AISteerCurve;

    CarController_ carController;
    [SerializeField] float gasInput;
    LayerMask ignoreLayer;

    private void Start()
    {
        InitializeSetting();
    }

    private void FixedUpdate()
    {
        if(carController != null)
        {
            AIGas();
            AISteering();
        }
    }

    public void InitializeSetting()
    {
        carController = GetComponentInChildren<CarController_>();

        frontMiddleRaySP = transform.GetChild(0).GetChild(4).GetChild(0).transform;
        frontLeftRaySP = transform.GetChild(0).GetChild(4).GetChild(1).transform;
        frontRightleRaySP = transform.GetChild(0).GetChild(4).GetChild(2).transform;
        wallLeftRaySP = transform.GetChild(0).GetChild(4).GetChild(3).transform;
        WallRightRaySP = transform.GetChild(0).GetChild(4).GetChild(4).transform;
        ignoreLayer = (-1) - (1 << LayerMask.NameToLayer("CheckPoint"));  // Everything���� Player ���̾ �����ϰ� �浹 üũ��

    }




    [Range(-540, 540)] public float wheelAngle = 0;
    public void AIGas() //�Ķ���
    {
        float slopeAngle = (Vector3.SignedAngle(transform.forward.normalized, Vector3.up, transform.forward.normalized) - 90) * -1f;
        RaycastHit GasfrontHit;

        float frontDistance = 0;

        Ray front = new Ray(frontMiddleRaySP.position, Quaternion.Euler(0, wheelAngle, 0) * frontMiddleRaySP.forward);

        Debug.DrawRay(frontMiddleRaySP.position - new Vector3(0, 1, 0), Quaternion.Euler(0, wheelAngle, 0) * frontMiddleRaySP.forward * 1000, Color.blue);
        // ���ʰ� ������ ���̸� ��� �浹�� �����մϴ�.

        if (Physics.Raycast(front, out GasfrontHit, Mathf.Infinity, ignoreLayer))
        {
            frontDistance = GasfrontHit.distance;
        }
        float angle = 0;

        // ���� ����� ���� ���� ���͸� ����մϴ�.
        Vector3 wallNormal = GasfrontHit.normal;
        Vector3 horizontalNormal = new Vector3(wallNormal.x, 0, wallNormal.z).normalized;


        // ���� ���� ���Ϳ� ���� ���� ���� ���� ������ ������ ����մϴ�.
        angle = Vector3.Angle(horizontalNormal, frontMiddleRaySP.forward);

        if (frontDistance * brakeSensitive + brakeMinDistance < carController.RigidSpeed)
        {
            gasInput = Mathf.Lerp(gasInput, -1, 0.1f);
        }
        else if (frontDistance < carController.RigidSpeed)
        {
            gasInput = Mathf.Lerp(gasInput, 0, 0.1f);
        }
        else
        {
            gasInput = Mathf.Lerp(gasInput, 1, 0.1f);
        }

        carController.accel = gasInput;
    }


    public void AISteering()
    {
        RaycastHit leftHit;
        RaycastHit rightHit;
        float leftDistance = 0;
        float rightDistance = 0;
        Ray leftFront = new Ray(frontLeftRaySP.position, frontLeftRaySP.forward);
        Ray rightFront = new Ray(frontRightleRaySP.position, frontRightleRaySP.forward);
        // ���ʰ� ������ ���̸� ��� �浹�� �����մϴ�.
        if (Physics.Raycast(leftFront, out leftHit, (carController.RigidSpeed * frontDetectedSensitive) + steeringDetectedSensitive, ignoreLayer))
        {
            leftDistance = leftHit.distance;
        }
        if (Physics.Raycast(rightFront, out rightHit, (carController.RigidSpeed * frontDetectedSensitive) + steeringDetectedSensitive, ignoreLayer))
        {
            rightDistance = rightHit.distance;
        }
        Debug.DrawRay(frontLeftRaySP.position, frontLeftRaySP.forward*(carController.RigidSpeed * frontDetectedSensitive + steeringDetectedSensitive), Color.yellow);
        Debug.DrawRay(frontRightleRaySP.position, frontRightleRaySP.forward*(carController.RigidSpeed * frontDetectedSensitive + steeringDetectedSensitive), Color.yellow);


        RaycastHit frontHit;

        float frontDistance = 0;

        Ray front = new Ray(frontMiddleRaySP.position, frontMiddleRaySP.forward);
        Debug.DrawRay(frontMiddleRaySP.position, frontMiddleRaySP.forward* 100, Color.red);
        if (Physics.Raycast(front, out frontHit, (carController.RigidSpeed * frontDetectedSensitive) + steeringDetectedSensitive + 2, ignoreLayer))
        {
            frontDistance = frontHit.distance;
        }
        else
        {
            AIWallCheck();
        }
        float angle = 0;

        // ���� ����� ���� ���� ���͸� ����մϴ�.
        Vector3 wallNormal = frontHit.normal;
        Vector3 horizontalNormal = new Vector3(wallNormal.x, 0, wallNormal.z).normalized;
        angle = Vector3.Angle(horizontalNormal, frontMiddleRaySP.forward);
        if (leftDistance != rightDistance)
        {
            float steerAmount = leftDistance < rightDistance ? AISteerCurve.Evaluate(angle) : -AISteerCurve.Evaluate(angle);
            autoHandle = Mathf.Lerp(autoHandle, steerAmount, 0.08f);
        }
        else
        {
            AIWallCheck();
        }

        wheelAngle = autoHandle * carController.maxSteerAngle / (1080.0f / 2);
        carController.steerValue = autoHandle;
    }
    public void AIWallCheck() //���
    {
        float wallAngle = 20;
        RaycastHit leftHit;
        RaycastHit rightHit;
        float leftDistance = 0;
        float rightDistance = 0;
        Ray leftFront = new Ray(wallLeftRaySP.position, Quaternion.Euler(0, -wallAngle + wheelAngle, 0) * wallLeftRaySP.forward);
        Ray rightFront = new Ray(WallRightRaySP.position, Quaternion.Euler(0, wallAngle + wheelAngle, 0) * WallRightRaySP.forward);
        Debug.DrawRay(wallLeftRaySP.position, Quaternion.Euler(0, -wallAngle + wheelAngle, 0) * wallLeftRaySP.forward * 100, Color.white);
        Debug.DrawRay(WallRightRaySP.position, Quaternion.Euler(0, wallAngle + wheelAngle, 0) * wallLeftRaySP.forward * 100, Color.white);

        // ���ʰ� ������ ���̸� ��� �浹�� �����մϴ�.
        if (Physics.Raycast(leftFront, out leftHit, Mathf.Infinity, ignoreLayer))
        {
            leftDistance = leftHit.distance;
        }
        if (Physics.Raycast(rightFront, out rightHit, Mathf.Infinity, ignoreLayer))
        {
            rightDistance = rightHit.distance;
        }

        if (leftDistance > rightDistance)
        {
            autoHandle = Mathf.Lerp(autoHandle, -180, 0.08f);

        }
        else if (rightDistance > leftDistance)
        {
            autoHandle = Mathf.Lerp(autoHandle, 180, 0.08f);
        }
        else
        {
            autoHandle = Mathf.Lerp(autoHandle, 0, 0.08f);
        }

    }




}
