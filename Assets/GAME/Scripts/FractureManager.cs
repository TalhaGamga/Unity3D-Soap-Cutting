using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class FractureManager : MonoBehaviour
{
    [Header("Layering")]
    public float layerThickness = 0.25f;

    [SerializeField] public List<List<Fracture>> layers = new List<List<Fracture>>();
    private int currentLayer = 0;
    private bool waitingForKnifeReturn = false;

    public Action OnLayerCleared;
    public Action<float> OnNeedKnifeDescend;
    public Transform[] layerMarkers;

    public int CurrentLayerIndex => currentLayer;

    public IReadOnlyList<Fracture> CurrentLayerRemaining =>
            (currentLayer < layers.Count) ? layers[currentLayer] : Array.Empty<Fracture>();

    public static Action OnAllLayersCleared;

    private void Start()
    {
        BinChunksIntoLayers();
        ActivateCurrentLayer();
    }

    public void NotifyChunkCut(Fracture chunk)
    {
        int foundLayer = -1;
        for (int i = 0; i < layers.Count; i++)
        {
            if (layers[i].Contains(chunk))
            {
                layers[i].Remove(chunk);
                foundLayer = i;
                break;
            }
        }

        if (foundLayer == currentLayer)
        {
            if (layers[currentLayer].Count == 0)
            {
                waitingForKnifeReturn = true;
                OnLayerCleared?.Invoke();
            }
        }
    }

    public void NotifyKnifeReturnedToBase()
    {
        if (!waitingForKnifeReturn) return;
        waitingForKnifeReturn = false;
        AdvanceToNextLayer();
    }

    private void BinChunksIntoLayers()
    {
        layers.Clear();
        if (layerMarkers == null || layerMarkers.Length == 0)
        {
            Debug.LogError("No layer markers assigned!");
            return;
        }

        var sortedMarkers = layerMarkers.OrderByDescending(m => m.position.y).ToArray();

        for (int i = 0; i <= sortedMarkers.Length; i++)
            layers.Add(new List<Fracture>());

        var allChunks = FindObjectsByType<Fracture>(FindObjectsSortMode.None);

        foreach (var chunk in allChunks)
        {
            float y = chunk.GetWorldCenter().y;
            int group = sortedMarkers.Length;

            for (int i = 0; i < sortedMarkers.Length; i++)
            {
                if (y >= sortedMarkers[i].position.y)
                {
                    group = i;
                    break;
                }
            }
            layers[group].Add(chunk);

            float t = (layers.Count <= 1) ? 0f : (group / (float)(layers.Count - 1));
            Color color = Color.Lerp(Color.red, Color.green, t);
            var renderer = chunk.GetComponent<Renderer>();
            if (renderer != null)
                renderer.material.color = color;
        }
    }



    private void ActivateCurrentLayer()
    {
        for (int i = 0; i < layers.Count; i++)
        {
            foreach (var chunk in layers[i])
                chunk.SetCuttable(i == currentLayer);
        }
    }
    private void AdvanceToNextLayer()
    {
        currentLayer++;
        if (currentLayer >= layers.Count - 1)
        {
            OnAllLayersCleared?.Invoke();
            return;
        }

        ActivateCurrentLayer();

        float nextY = GetLayerY(currentLayer);
        OnNeedKnifeDescend?.Invoke(nextY);
    }

    public float GetLayerY(int layerIndex)
    {
        if (layerIndex < 0 || layerIndex >= layers.Count || layers[layerIndex].Count == 0)
            return 0f;

        return layers[layerIndex].Average(chunk => chunk.GetWorldCenter().y);
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;
        if (currentLayer >= layers.Count) return;

        Gizmos.color = Color.cyan;
        var layer = layers[currentLayer];
        for (int i = 0; i < layer.Count; i++)
        {
            var c = layer[i];
            if (c == null) continue;
            Vector3 p = c.GetWorldCenter();
            Gizmos.DrawWireSphere(p, 0.04f);
#if UNITY_EDITOR
            UnityEditor.Handles.Label(p + Vector3.up * 0.02f, i.ToString());
#endif
        }
    }
#endif
}
