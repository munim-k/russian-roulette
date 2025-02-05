using UnityEngine;
using TMPro;
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Multiplayer;
using UnityUtils;

public class PlayerListItemScript : MonoBehaviour
{
    public TMP_Text PlayerNameText;
    public string PlayerId { get; set; }
    
    internal void Initialize(string playerName, string playerId)
    {
        PlayerNameText.text = playerName;
        PlayerId = playerId;
    }
}
