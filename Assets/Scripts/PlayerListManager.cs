using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityUtils;

public class PlayerListManager : MonoBehaviour
{
   [SerializeField]
   private SessionManager sessionManager;
   private ISession activeSession;
   public GameObject ListItem;
   public Transform ContentRoot;
   Dictionary<string, PlayerListItemScript> m_PlayerListItems = new();
   List<PlayerListItemScript> m_CachedPlayerListItems = new();
   
   
   private void Start()
   {
      activeSession = sessionManager.activeSession;
   }

   public void ResetState()
   {
      Debug.Log("Clearing player list");
      foreach (var playerListItem in m_PlayerListItems.Values)
      {
         Destroy(playerListItem.gameObject);
      }
      m_PlayerListItems.Clear();
      m_CachedPlayerListItems.Clear();
   }

   private void Update()
   {
      if(activeSession == null)
         activeSession = sessionManager.activeSession;
   }

   void UpdatePlayerList()
   {
      if (activeSession == null)
         return;
            
      foreach (var player in activeSession.Players)
      {
         var playerId = player.Id;
                
         if (m_PlayerListItems.ContainsKey(playerId))
            continue;
                
         var playerListItem = GetPlayerListItem(playerId);
         playerListItem.gameObject.SetActive(true);
                
         var playerName = "Anonymous";
         if (player.Properties.TryGetValue("playerName", out var playerNameProperty))
            playerName = playerNameProperty.Value;
         
         playerListItem.Initialize(playerName, playerId);
      }
   }
   PlayerListItemScript GetPlayerListItem(string playerId)
   {
      if(m_PlayerListItems.TryGetValue(playerId, out var playerListItem))
         return playerListItem;
            
      if(m_CachedPlayerListItems.Count > 0)
      {
         playerListItem = m_CachedPlayerListItems[0];
         m_CachedPlayerListItems.RemoveAt(0);
      }
      else
      {
         playerListItem = Instantiate(ListItem, ContentRoot).GetComponent<PlayerListItemScript>();
      }
            
      m_PlayerListItems.Add(playerId, playerListItem);
      return playerListItem;
   }

   public void refresh()
   {      UpdatePlayerList();
   }
}
