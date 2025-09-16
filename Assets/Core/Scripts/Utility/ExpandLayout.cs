using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExpandLayout : MonoBehaviour
{
    private int _currentCount = 0;
    private int _maxCount = 10;
    
    void Update()
    {
        
        _currentCount++;
        if (_currentCount < _maxCount)
        {
            gameObject.SetActive(false);
            gameObject.SetActive(true);
        }
    }
}
