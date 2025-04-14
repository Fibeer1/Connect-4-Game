using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugMessenger : MonoBehaviour
{
    public static DebugMessenger Instance;
    [SerializeField] private bool showDebugMessages;

    private void Awake()
    {
        Instance = this;
    }

    public static void DebugMessage(string message)
    {
        if (Instance == null || (Instance != null && !Instance.showDebugMessages))
        {
            return;
        }
        Debug.Log(message);
    }
}
