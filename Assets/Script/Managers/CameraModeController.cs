using UnityEngine;
using Cinemachine;
using System.Collections;

public class CameraModeController : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    [SerializeField] private GameObject flyingModeObject;
    [SerializeField] private GameObject runningModeObject;
    [SerializeField] private float runningModeCameraPositionY = -1.5f;
    [SerializeField] private float runningModeAimTrackedObjectY = 1f;
    [SerializeField] private float transitionDuration = 1f;
    [SerializeField] private float runningModeDuration = 40f;
    [SerializeField] private float objectSwitchAdvanceTime = 2f; // Время раннего переключения объектов

    private Vector3 originalCameraPosition;
    private Vector3 originalTrackedObjectOffset;
    private bool isInRunningMode = false;
    private float transitionTimer = 0f;
    private Coroutine runningModeCoroutine;

    private void Start()
    {
        if (virtualCamera == null)
        {
            virtualCamera = GetComponent<CinemachineVirtualCamera>();
        }
        originalCameraPosition = virtualCamera.transform.localPosition;
        var composer = virtualCamera.GetCinemachineComponent<CinemachineComposer>();
        if (composer != null)
        {
            originalTrackedObjectOffset = composer.m_TrackedObjectOffset;
        }
    }

    private void Update()
    {
        if (transitionTimer > 0)
        {
            transitionTimer -= Time.deltaTime;
            float t = 1 - (transitionTimer / transitionDuration);
            // Изменяем только Y-координату позиции камеры
            Vector3 currentPosition = virtualCamera.transform.localPosition;
            float targetY = isInRunningMode ? runningModeCameraPositionY : originalCameraPosition.y;
            virtualCamera.transform.localPosition = new Vector3(currentPosition.x, Mathf.Lerp(currentPosition.y, targetY, t), currentPosition.z);
            var composer = virtualCamera.GetCinemachineComponent<CinemachineComposer>();
            if (composer != null)
            {
                Vector3 targetTrackedOffset = isInRunningMode
                    ? new Vector3(originalTrackedObjectOffset.x, runningModeAimTrackedObjectY, originalTrackedObjectOffset.z)
                    : originalTrackedObjectOffset;
                composer.m_TrackedObjectOffset = Vector3.Lerp(composer.m_TrackedObjectOffset, targetTrackedOffset, t);
            }
        }
    }

    public void SetRunningMode(bool isRunning)
    {
        if (isRunning && !isInRunningMode)
        {
            StartRunningMode();
        }
        else if (!isRunning && isInRunningMode)
        {
            SetFlyingMode();
        }
    }

    private void StartRunningMode()
    {
        isInRunningMode = true;
        transitionTimer = transitionDuration;
        SwitchObjects(true);

        if (runningModeCoroutine != null)
        {
            StopCoroutine(runningModeCoroutine);
        }
        runningModeCoroutine = StartCoroutine(RunningModeTimer());
    }

    private void SetFlyingMode()
    {
        isInRunningMode = false;
        transitionTimer = transitionDuration;
        SwitchObjects(false);

        if (runningModeCoroutine != null)
        {
            StopCoroutine(runningModeCoroutine);
            runningModeCoroutine = null;
        }
    }

    private IEnumerator RunningModeTimer()
    {
        yield return new WaitForSeconds(runningModeDuration - objectSwitchAdvanceTime);

        // Раннее переключение объектов
        SwitchObjects(false);

        yield return new WaitForSeconds(objectSwitchAdvanceTime);

        // Полное переключение в режим полета
        SetFlyingMode();
    }

    private void SwitchObjects(bool toRunningMode)
    {
        if (flyingModeObject != null)
        {
            flyingModeObject.SetActive(!toRunningMode);
        }
        if (runningModeObject != null)
        {
            runningModeObject.SetActive(toRunningMode);
        }
    }
}