using UnityEngine;
using DG.Tweening;

public class GiftObject : MonoBehaviour
{
    [SerializeField] private float _riseDuration = 0.5f;
    [SerializeField] private Transform _modelTransform;
    [SerializeField] private GameObject _effectsParent;
    private void OnEnable()
    {
        FractureManager.OnAllLayersCleared += onAllLayersCleared;
    }

    private void OnDisable()
    {
        FractureManager.OnAllLayersCleared -= onAllLayersCleared;
    }

    private void onAllLayersCleared()
    {
        Debug.Log("onAllLayersCleared");
        Sequence seq = DOTween.Sequence();

        seq.Join(_modelTransform.DORotate(
            new Vector3(0, 360, 0),
            _riseDuration,
            RotateMode.FastBeyond360
        ).SetEase(Ease.OutQuad));


        seq.Join(_modelTransform.DOMoveY(
            _modelTransform.position.y + 5f,
            _riseDuration
        ).SetEase(Ease.OutQuad));

        ParticleSystem[] effects = _effectsParent.GetComponentsInChildren<ParticleSystem>();
        foreach (var effect in effects)
        {
            effect.Play();
        }
    }

}
