using UnityEngine;

[CreateAssetMenu(fileName = "MovementSettings", menuName = "Game/Movement Settings")]
public class MovementSettings : ScriptableObject
{
    [Header("Настройки полета")]
    public float flyingMoveSpeed = 300f;
    public Vector2 flyingMinBounds = new Vector2(-4f, -10f); // Убрали ограничение снизу
    public Vector2 flyingMaxBounds = new Vector2(5f, 7f);  // Увеличили верхнее ограничение

    [Header("Настройки бега")]
    public float runningMoveSpeed = 5f; // Оставляем для совместимости, но не используем
    public float gravity = -9.81f;
    public Vector2 runningMinBounds = new Vector2(-4f, 0f);
    public Vector2 runningMaxBounds = new Vector2(5f, 4f); // Увеличили верхнее ограничение

    [Header("Настройки прыжка")]
    public float jumpForce = 10f;  // Сила первого прыжка
    public float doubleJumpForce = 8f;  // Начальная сила второго прыжка
    public float doubleJumpDuration = 1.2f;  // Продолжительность второго прыжка
    public float doubleJumpUpwardSpeed = 5f;  // Скорость подъема при втором прыжке

    [Header("Настройки подката")]
    public float slideDuration = 1f;  // Продолжительность подката
    public float slideWorldSpeedMultiplier = 1.3f;  // Множитель скорости мира при подкате

    [Header("Настройки окружения")]
    public float flyingEnvironmentSpeed = 75f;
    public float runningEnvironmentSpeed = 20f;
}