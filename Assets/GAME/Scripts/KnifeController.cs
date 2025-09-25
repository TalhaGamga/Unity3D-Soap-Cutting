using UnityEngine;
using DG.Tweening;

public class KnifeController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float followSpeed = 20f;
    public float returnDuration = 0.4f;
    public Camera cam;

    [SerializeField] private CollisionSensor _collisionSensor;

    private FractureManager _fractureManager;

    private Vector3 initialPosition;
    private bool isDragging = false;
    private bool readyToDescend = false;


    private void OnEnable()
    {
        SoapGenerator.SoapGenerated += onSoapGenerated;
        _collisionSensor.OnCollisionEntered += onCollisionEntered;
    }

    private void OnDisable()
    {
        SoapGenerator.SoapGenerated -= onSoapGenerated;
        _collisionSensor.OnCollisionEntered -= onCollisionEntered;
    }

    private void onCollisionEntered(Collision collision)
    {
        if (collision.gameObject.TryGetComponent<Fracture>(out Fracture fracture))
        {
            if (fracture.IsCut && !fracture.IsOnKnife)
            {
                if (fracture.TryGetComponent<Rigidbody>(out Rigidbody rb))
                {
                    var contactPoint = collision.GetContact(0);
                    var forceVector = fracture.transform.position - contactPoint.point;
                    rb.AddForce(forceVector * 20);
                }
            }
        }
    }

    private void onSoapGenerated(GameObject soap)
    {
        var fractureManager = soap.GetComponentInChildren<FractureManager>();
        _fractureManager = fractureManager != null ? fractureManager : null;

        setFractureBindings(true);
    }

    private void onLevelCompleted()
    {
        setFractureBindings(false);
    }

    private void setFractureBindings(bool mustBind)
    {
        if (mustBind)
        {
            _fractureManager.OnLayerCleared += HandleLayerCleared;
            _fractureManager.OnNeedKnifeDescend += DescendKnife;
            return;
        }

        _fractureManager.OnLayerCleared -= HandleLayerCleared;
        _fractureManager.OnNeedKnifeDescend -= DescendKnife;
    }

    private void Start()
    {
        initialPosition = transform.position;
        if (cam == null)
            cam = Camera.main;
    }

    private void Update()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        HandleMouseInput();
#else
        HandleTouchInput();
#endif
    }

    void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
            isDragging = true;

        if (Input.GetMouseButton(0) && isDragging)
        {
            Vector3 screenPoint = Input.mousePosition;
            Vector3 worldPoint = cam.ScreenToWorldPoint(new Vector3(screenPoint.x, screenPoint.y, GetZDistance()));
            Vector3 target = new Vector3(worldPoint.x, initialPosition.y, worldPoint.z);
            transform.position = Vector3.Lerp(transform.position, target, followSpeed * Time.deltaTime);
        }

        if (Input.GetMouseButtonUp(0) && isDragging)
        {
            isDragging = false;
            ReturnToInitial();
        }
    }

    void HandleTouchInput()
    {
        if (Input.touchCount == 0) return;
        Touch touch = Input.GetTouch(0);

        if (touch.phase == TouchPhase.Began)
            isDragging = true;
        else if ((touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary) && isDragging)
        {
            Vector3 screenPoint = touch.position;
            Vector3 worldPoint = cam.ScreenToWorldPoint(new Vector3(screenPoint.x, screenPoint.y, GetZDistance()));
            Vector3 target = new Vector3(worldPoint.x, initialPosition.y, worldPoint.z);
            transform.position = Vector3.Lerp(transform.position, target, followSpeed * Time.deltaTime);
        }
        else if ((touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled) && isDragging)
        {
            isDragging = false;
            ReturnToInitial();
        }
    }

    float GetZDistance()
    {
        return Vector3.Distance(cam.transform.position, initialPosition);
    }

    void ReturnToInitial()
    {
        transform.DOMove(initialPosition, returnDuration)
            .SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                if (readyToDescend)
                {
                    readyToDescend = false;
                    _fractureManager?.NotifyKnifeReturnedToBase();
                }
            });
    }

    void HandleLayerCleared()
    {
        readyToDescend = true;
    }

    void DescendKnife(float targetY)
    {
        Vector3 newPos = new Vector3(initialPosition.x, targetY, initialPosition.z);
        initialPosition = newPos;

        transform.DOMove(newPos, 0.3f)
            .SetEase(Ease.OutQuad);
    }


}
