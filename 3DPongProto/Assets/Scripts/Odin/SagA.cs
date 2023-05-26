using UnityEngine;
using Sirenix.OdinInspector;

public class SagA : MonoBehaviour
{
    //[ActionButton("I don't work!")]
    [ActionButton("PrintInDebugConsole")]
    [SerializeField] private string m_buttonString;

    [ActionButton("@UnityEngine.Debug.Log(\"I did an Action without a method!\")")]
    [SerializeField] private string m_anotherButtonString;

    private void PrintInDebugConsole()
    {
        Debug.Log("Hab was geschrieben!");
    }
}