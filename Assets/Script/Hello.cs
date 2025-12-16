using UnityEngine;

public class Hello : MonoBehaviour
{
    void Awake() { Debug.Log("Awake() OK"); }
    void OnEnable() { Debug.Log("OnEnable() OK"); }
    void Start() { Debug.Log("Start() OK"); }
}
