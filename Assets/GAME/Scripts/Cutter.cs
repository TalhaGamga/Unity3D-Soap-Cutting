using UnityEngine;

public class Cutter : MonoBehaviour
{
    private FractureManager _fractureManager;

    private void OnEnable()
    {
        SoapGenerator.SoapGenerated += onSoapGenerated;
    }

    private void OnDisable()
    {
        SoapGenerator.SoapGenerated -= onSoapGenerated;
    }
    private void onSoapGenerated(GameObject soap)
    {
        var fractureManager = soap.GetComponentInChildren<FractureManager>();
        _fractureManager = fractureManager != null ? fractureManager : null;
    }

    private void onLevelCompleted()
    {
        _fractureManager = null;
    }


    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.TryGetComponent<Fracture>(out Fracture fracture))
        {
            fracture.Cut(transform);
            _fractureManager?.NotifyChunkCut(fracture);
        }
    }
}
