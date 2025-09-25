using UnityEngine;
using DG.Tweening;
using System.Collections;

[SerializeField]
public class Fracture : MonoBehaviour
{
    [Header("Animation Settings")]
    public float upDistance = 1f;
    public float moveUpDuration = 0.25f;
    public float vibrateStrength = 0.05f;
    public float vibrateDuration = 0.1f;
    public float vibrateRandomness = 90f;
    public float fadeOutDuration = 5f;
    public Ease _fadeOutEase;

    [Header("Cutter Drop Settings")]
    public float dropDistance = 1.25f;

    private Rigidbody rb;
    private MeshRenderer meshRenderer;
    public bool IsCut = false;
    public bool IsOnKnife = false;
    private Tween vibrateTween;

    private Transform _cutterTransform;

    private bool isCuttable = false;


    void Start()
    {
        rb = GetComponent<Rigidbody>();
        meshRenderer = GetComponent<MeshRenderer>();
        rb.constraints = RigidbodyConstraints.FreezeAll;
    }

    public void Cut(Transform cutterTransform)
    {
        if (IsCut) return;

        IsCut = true;
        isCuttable = false;
        _cutterTransform = cutterTransform;
        Vector3 targetPos = transform.position + Vector3.up * upDistance;
        transform.DOMove(targetPos, moveUpDuration).SetEase(Ease.OutBack).OnComplete(() =>
        {
            StartVibration();
            IsOnKnife = true;
        });
    }

    void Update()
    {
        if (IsCut && IsOnKnife && _cutterTransform != null)
        {
            Vector3 center = GetWorldCenter();
            Vector3 planePoint = _cutterTransform.position;

            Vector3 planeNormal = _cutterTransform.right;

            float orthogonalDist = Mathf.Abs(Vector3.Dot(center - planePoint, planeNormal.normalized));
            if (orthogonalDist > dropDistance)
            {
                Drop();
            }
        }
    }

    private void StartVibration()
    {
        vibrateTween = transform.DOShakePosition(
            vibrateDuration,
            vibrateStrength,
            10,
            vibrateRandomness,
            false,
            true
        ).SetLoops(-1, LoopType.Restart);
    }

    private void Drop()
    {
        if (!IsOnKnife) return;
        Debug.Log("Dropped");
        vibrateTween?.Kill();
        rb.constraints = RigidbodyConstraints.None;
        rb.useGravity = true;
        IsOnKnife = false;

        StartCoroutine(FadeOutAndDestroy());
    }

    private IEnumerator FadeOutAndDestroy()
    {
        if (meshRenderer == null || meshRenderer.material == null)
        {
            Destroy(gameObject, 0.2f);
            yield return null;
        }
        yield return new WaitForSeconds(0.75f);
        var mat = meshRenderer.material;
        Color color = mat.color;
        DOTween.To(() => color.a, a =>
        {
            color.a = a;
            mat.color = color;
        }, 0f, fadeOutDuration)
            .SetEase(_fadeOutEase)
            .OnComplete(() => Destroy(gameObject));
    }

    private void OnDestroy()
    {
        vibrateTween?.Kill();
    }

    public Vector3 GetWorldCenter()
    {
        MeshFilter mf = GetComponent<MeshFilter>();
        if (mf == null || mf.sharedMesh == null) return transform.position;
        Vector3 localCenter = mf.sharedMesh.bounds.center;
        return transform.TransformPoint(localCenter);
    }


    public void SetCuttable(bool value)
    {
        isCuttable = value;
    }
}
