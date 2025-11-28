// Replace DecalProjector with this super simple version
using DG.Tweening;
using UnityEngine;

public class RipplePointerSimple : MonoBehaviour
{
    private SpriteRenderer sr;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        transform.localScale = Vector3.one * 0.1f;

        transform.DOScale(Vector3.one * 30f, 1.8f).SetEase(Ease.OutQuart);
        sr.DOFade(0f, 1.8f).SetEase(Ease.OutQuart)
            .OnComplete(() => Destroy(gameObject));
    }
}
