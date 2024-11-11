using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

public class SettingsSubsectionManager : MonoBehaviour
{
    [System.Serializable]
    public class SubsectionInfo
    {
        public string subsectionName;
        public RectTransform subsectionTransform;
        public CanvasGroup canvasGroup;
    }

    public List<SubsectionInfo> subsections = new List<SubsectionInfo>();
    public float transitionDuration = 0.5f;
    public Ease transitionEase = Ease.InOutQuad;
    private SubsectionInfo currentSubsection;

    void Start()
    {
        InitializeSubsections();
    }

    void InitializeSubsections()
    {
        if (subsections.Count == 0)
        {
            Debug.LogWarning("No subsections defined in SettingsSubsectionManager.");
            return;
        }

        currentSubsection = subsections[0];

        foreach (var subsection in subsections)
        {
            if (subsection.subsectionTransform == null || subsection.canvasGroup == null)
            {
                Debug.LogError($"Subsection {subsection.subsectionName} is missing RectTransform or CanvasGroup.");
                continue;
            }

            if (subsection != currentSubsection)
            {
                subsection.subsectionTransform.gameObject.SetActive(false);
                subsection.canvasGroup.alpha = 0;
            }
            else
            {
                subsection.subsectionTransform.gameObject.SetActive(true);
                subsection.canvasGroup.alpha = 1;
            }
        }
    }

    public void TransitionToSubsection(string targetSubsectionName)
    {
        SubsectionInfo targetSubsectionInfo = GetSubsectionByName(targetSubsectionName);
        if (targetSubsectionInfo == null || targetSubsectionInfo == currentSubsection) return;

        if (currentSubsection == null)
        {
            currentSubsection = subsections[0];
        }

        if (currentSubsection.canvasGroup == null || targetSubsectionInfo.canvasGroup == null)
        {
            Debug.LogError("CanvasGroup is missing for one of the subsections.");
            return;
        }

        currentSubsection.canvasGroup.DOFade(0, transitionDuration / 2)
            .SetEase(transitionEase)
            .OnComplete(() => {
                currentSubsection.subsectionTransform.gameObject.SetActive(false);
                targetSubsectionInfo.subsectionTransform.gameObject.SetActive(true);
                targetSubsectionInfo.canvasGroup.alpha = 0;
                targetSubsectionInfo.canvasGroup.DOFade(1, transitionDuration / 2)
                    .SetEase(transitionEase);
                currentSubsection = targetSubsectionInfo;
            });
    }

    private SubsectionInfo GetSubsectionByName(string subsectionName)
    {
        return subsections.Find(s => s.subsectionName == subsectionName);
    }
}