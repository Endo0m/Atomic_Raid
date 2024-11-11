using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatingEffect : MonoBehaviour
{
    [System.Serializable]
    public class FloatingObject
    {
        public RectTransform rectTransform;
        public Vector3 startRotation;
    }

    [SerializeField] private List<FloatingObject> floatingObjects = new List<FloatingObject>();
    [SerializeField] private float floatDuration = 2f;
    [SerializeField] private Vector3 rotationAmount = new Vector3(1f, 0f, 2f);
    private MusicManager musicManager;

    private void Start()
    {
        musicManager = FindObjectOfType<MusicManager>();

        SetupFloatingEffects();
    }

    private void SetupFloatingEffects()
    {
        foreach (var obj in floatingObjects)
        {
            obj.startRotation = obj.rectTransform.localRotation.eulerAngles;
            AnimateObject(obj);
        }
    }

    private void AnimateObject(FloatingObject obj)
    {
        obj.rectTransform.DORotate(obj.startRotation + rotationAmount, floatDuration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }

    // Метод для добавления объектов во время выполнения, если нужно
    public void AddFloatingObject(RectTransform rectTransform)
    {
        FloatingObject newObj = new FloatingObject { rectTransform = rectTransform, startRotation = rectTransform.localRotation.eulerAngles };
        floatingObjects.Add(newObj);
        AnimateObject(newObj);
    }
}