using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class FoodWasteController : MonoBehaviour

{

    // Il GameFlowManager si collegherà qui
    public System.Action OnAllWasteRemoved;
    

    [Header("Food Waste Items")]
    [SerializeField] private GameObject[] _foodWasteItems;
    private int _remainingWasteCount;




    private void Start()
    {
        //trovo tutta la spazzatura iniziale
        _foodWasteItems = GameObject.FindGameObjectsWithTag("Waste");
        _remainingWasteCount = _foodWasteItems.Length;

    }

    public void NotifyWasteRemoved()
    {
        Debug.Log("Cibo rimosso! Rimanenti: " + (_remainingWasteCount - 1));
        
        _remainingWasteCount--;

        if (_remainingWasteCount == 0)
        {
            Debug.Log("Tutta la spazzatura è stata rimossa!");
            if (OnAllWasteRemoved == null)
            {
                Debug.Log("Nessun listener registrato per OnAllWasteRemoved.");
            }
            else
            {
                OnAllWasteRemoved?.Invoke();
                Debug.Log("Evento OnAllWasteRemoved invocato.");
            }
            
        }
    }

    

    
}
