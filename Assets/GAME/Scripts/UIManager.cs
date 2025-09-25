using DG.Tweening;
using System.Collections;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject _winUI;
    [SerializeField] private float _targetScale = 1;
    [SerializeField] private float _duration = 0.2f;
    [SerializeField] private Ease _ease;

    private void OnEnable()
    {
        FractureManager.OnAllLayersCleared += onAllLayersCleared;
    }

    private void OnDisable()
    {
        FractureManager.OnAllLayersCleared -= onAllLayersCleared;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            activateAndDoScale();
        }
    }
    private void onAllLayersCleared()
    {
        StartCoroutine(activateWinUI());
    }

    private IEnumerator activateWinUI()
    {
        yield return new WaitForSeconds(1f);
        activateAndDoScale();
    }

    private void activateAndDoScale()
    {
        _winUI.SetActive(true);
        _winUI.transform.DOScale(_targetScale, _duration).SetEase(_ease);

    }

}
