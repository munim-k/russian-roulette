using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Multiplayer;
using UnityEditor.Networking.PlayerConnection;
using UnityEngine;
using UnityUtils;

public class SessionManager : Singleton<SessionManager> {
    public ISession activeSession;
    public GameManager gameManager;
    public event Action OnSessionJoined;

    public PlayerManager playerManager;
    
    ISession ActiveSession {
        get => activeSession;
        set {
            activeSession = value;
            Debug.Log($"Active session: {activeSession}");
        }
    }
    
    const string playerNamePropertyKey = "playerName";

    async void Start() {
        try {
            await UnityServices.InitializeAsync(); // Initialize Unity Gaming Services SDKs.
            await AuthenticationService.Instance.SignInAnonymouslyAsync(); // Anonymously authenticate the player
            // Debug.Log($"Sign in anonymously succeeded! PlayerID: {AuthenticationService.Instance.PlayerId}");
            TurnManager.Instance.currentPlayerId = AuthenticationService.Instance.PlayerId;
            playerManager.sessionId = AuthenticationService.Instance.PlayerId;
            // Start a new session as a host
            // StartSessionAsHost();
        }
        catch (Exception e) {
            Debug.LogException(e);
        }
    }

    async UniTask<Dictionary<string, PlayerProperty>> GetPlayerProperties() {
        // Custom game-specific properties that apply to an individual player, ie: name, role, skill level, etc.
        var playerName = await AuthenticationService.Instance.GetPlayerNameAsync();
        var playerNameProperty = new PlayerProperty(playerName, VisibilityPropertyOptions.Member);
        return new Dictionary<string, PlayerProperty> { { playerNamePropertyKey, playerNameProperty } };
    }

    public async void StartSessionAsHost() {
        var playerProperties = await GetPlayerProperties(); 
        
        var options = new SessionOptions {
            MaxPlayers = gameManager.Players,
            IsLocked = false,
            IsPrivate = gameManager.isPrivate,
            PlayerProperties = playerProperties 
        }.WithRelayNetwork();
        
        ActiveSession = await MultiplayerService.Instance.CreateSessionAsync(options);
        Debug.Log($"Session {ActiveSession.Id} created! Join code: {ActiveSession.Code}");
        gameManager.SessionStarted();
        OnSessionJoined?.Invoke();
    }

    // Doesnt run ever
    public async UniTaskVoid JoinSessionById(string sessionId) {
        ActiveSession = await MultiplayerService.Instance.JoinSessionByIdAsync(sessionId);
        Debug.Log($"Session {ActiveSession.Id} joined!");
        OnSessionJoined?.Invoke();
    }

    // Doesnt run ever
    public async UniTaskVoid JoinSessionByCode(string sessionCode) {
        ActiveSession = await MultiplayerService.Instance.JoinSessionByCodeAsync(sessionCode);
        Debug.Log($"Session {ActiveSession.Id} joined!");
        OnSessionJoined?.Invoke();
    }

    async UniTask KickPlayer(string playerId) {
        if (!ActiveSession.IsHost) return;
        await ActiveSession.AsHost().RemovePlayerAsync(playerId);
    }

    async UniTask<IList<ISessionInfo>> QuerySessions() {
        var sessionQueryOptions = new QuerySessionsOptions();
        QuerySessionsResults results = await MultiplayerService.Instance.QuerySessionsAsync(sessionQueryOptions);
        return results.Sessions;
    }

    async public UniTask KickAll() {
        if (!ActiveSession.IsHost) return;
        foreach (var player in ActiveSession.Players) {
            if (player.Id == AuthenticationService.Instance.PlayerId) continue;
            await KickPlayer(player.Id);
        }
    }

   
    public async UniTask LeaveSession() {
        try {
            await ActiveSession.LeaveAsync();
        }
        catch {
            // Ignored as we are exiting the game
        }
        finally {
            ActiveSession = null;
        }
    }
}