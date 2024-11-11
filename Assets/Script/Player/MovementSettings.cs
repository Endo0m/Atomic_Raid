using UnityEngine;

[CreateAssetMenu(fileName = "MovementSettings", menuName = "Game/Movement Settings")]
public class MovementSettings : ScriptableObject
{
    [Header("��������� ������")]
    public float flyingMoveSpeed = 300f;
    public Vector2 flyingMinBounds = new Vector2(-4f, -10f); // ������ ����������� �����
    public Vector2 flyingMaxBounds = new Vector2(5f, 7f);  // ��������� ������� �����������

    [Header("��������� ����")]
    public float runningMoveSpeed = 5f; // ��������� ��� �������������, �� �� ����������
    public float gravity = -9.81f;
    public Vector2 runningMinBounds = new Vector2(-4f, 0f);
    public Vector2 runningMaxBounds = new Vector2(5f, 4f); // ��������� ������� �����������

    [Header("��������� ������")]
    public float jumpForce = 10f;  // ���� ������� ������
    public float doubleJumpForce = 8f;  // ��������� ���� ������� ������
    public float doubleJumpDuration = 1.2f;  // ����������������� ������� ������
    public float doubleJumpUpwardSpeed = 5f;  // �������� ������� ��� ������ ������

    [Header("��������� �������")]
    public float slideDuration = 1f;  // ����������������� �������
    public float slideWorldSpeedMultiplier = 1.3f;  // ��������� �������� ���� ��� �������

    [Header("��������� ���������")]
    public float flyingEnvironmentSpeed = 75f;
    public float runningEnvironmentSpeed = 20f;
}