using System;
using System.Linq;
using UnityEngine;

public class SoapGenerator : MonoBehaviour
{
    public static Action<GameObject> SoapGenerated;

    [SerializeField] private GameObject[] _soapLevelPrefabs;

    [SerializeField] private int _level = 0;

    private void OnEnable()
    {
        SoapGenerated += incrementLevel;
    }

    private void OnDisable()
    {
        SoapGenerated -= incrementLevel;
    }

    private void Start()
    {
        generateSoap(_level);
    }

    private void generateSoap(int level)
    {
        var soapPrefab = level < _soapLevelPrefabs.Count() ? _soapLevelPrefabs[level] : null;
        GameObject soapInstance = Instantiate(soapPrefab, transform);
        SoapGenerated?.Invoke(soapInstance);
    }

    private void incrementLevel(GameObject _)
    {
        _level++;
    }
}