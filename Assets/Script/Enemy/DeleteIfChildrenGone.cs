using UnityEngine;

public class DeleteIfChildrenGone : MonoBehaviour
{
    private int initialChildCount;

    private void Start()
    {
        // Запоминаем начальное количество дочерних объектов
        initialChildCount = transform.childCount;
    }

    private void Update()
    {
        // Проверяем, остались ли дочерние объекты
        if (transform.childCount == 0 && initialChildCount > 0)
        {
            // Если все дочерние объекты удалены, удаляем этот объект
            Destroy(gameObject);
        }
    }
}