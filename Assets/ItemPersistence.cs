using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemPersistence : MonoBehaviour
{
    public static ItemPersistence Instance { get; private set; }
    private HashSet<string> _pickedIds = new HashSet<string>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    public void MarkPicked(string id)   => _pickedIds.Add(id);
    public void UnmarkPicked(string id) => _pickedIds.Remove(id);
    public bool IsPicked(string id)     => _pickedIds.Contains(id);
}
