using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ConsoleToGui : MonoBehaviour
{
    [ SerializeField ] TextMeshProUGUI textArea;
    
    static string _myLog = "";
    private string _output;
    private string _stack;

    void OnEnable() => Application.logMessageReceived += Log;
    void OnDisable() => Application.logMessageReceived -= Log;

    public void Log( string logString, string stackTrace, LogType type )
    {
        _output = logString;
        _stack = stackTrace;
        _myLog = _output + "\n" + stackTrace + "\n" + _myLog;
        if( _myLog.Length > 5000 )
        {
            _myLog = _myLog[ ..4000 ];
        }
    }

    void OnGUI() => textArea.text = _myLog;
}
