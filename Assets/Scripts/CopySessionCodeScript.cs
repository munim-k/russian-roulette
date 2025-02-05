using System;
using TMPro;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class CopySessionCodeScript : MonoBehaviour
{
    const string k_NoCode = "–";
    [SerializeField]
    private SessionManager sessionManager;
    private ISession activeSession;
    [SerializeField]
    TMP_Text m_Text;

    private void Start()
    {
        activeSession = sessionManager.activeSession;
        if(m_Text == null)
            m_Text = GetComponentInChildren<TMP_Text>();
        
        m_Text.text = activeSession?.Code ?? k_NoCode;
    }

    private void Update()
    {
        if (activeSession == null)
        {
            activeSession = sessionManager.activeSession;
            if(activeSession != null)
                m_Text.text = activeSession.Code;
        }
    }
}
