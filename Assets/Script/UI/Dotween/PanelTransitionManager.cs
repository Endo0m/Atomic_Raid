using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;

public class PanelTransitionManager : MonoBehaviour
{
    [System.Serializable]
    public class ScreenInfo
    {
        public string screenName;
        public RectTransform screenTransform;
        public CanvasGroup canvasGroup;
    }

    public List<ScreenInfo> screens = new List<ScreenInfo>();
    public float transitionDuration = 1f;
    public Ease transitionEase = Ease.InOutQuad;

    private ScreenInfo currentScreen;

    void Start()
    {
        if (screens.Count > 0)
        {
            currentScreen = screens[0];
            InitializeScreens();
        }
    }

    void InitializeScreens()
    {
        foreach (var screen in screens)
        {
            if (screen != currentScreen)
            {
                screen.screenTransform.gameObject.SetActive(false);
                screen.canvasGroup.alpha = 0;
            }
            else
            {
                screen.screenTransform.gameObject.SetActive(true);
                screen.canvasGroup.alpha = 1;
            }
        }
    }

    public void TransitionToScreen(string targetScreenName)
    {
        ScreenInfo targetScreenInfo = GetScreenByName(targetScreenName);
        if (targetScreenInfo == null || targetScreenInfo == currentScreen) return;

        currentScreen.canvasGroup.DOFade(0, transitionDuration / 2)
            .SetEase(transitionEase)
            .OnComplete(() => {
                currentScreen.screenTransform.gameObject.SetActive(false);

                targetScreenInfo.screenTransform.gameObject.SetActive(true);
                targetScreenInfo.canvasGroup.alpha = 0;

                targetScreenInfo.canvasGroup.DOFade(1, transitionDuration / 2)
                    .SetEase(transitionEase);

                currentScreen = targetScreenInfo;
            });
    }

    private ScreenInfo GetScreenByName(string screenName)
    {
        return screens.Find(s => s.screenName == screenName);
    }
}