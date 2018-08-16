﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ATTRIBUTES;
using Facepunch.Steamworks;
using MANAGERS;
using MISC;
using Newtonsoft.Json.Linq;
using UnityEngine;
using Types = DATA.Types;

namespace MENU
{
    [MenuType(Types.Menu.LobbyCharacterMenu)]
    public class LobbyCharacterMenu : Menu
    {
        [SerializeField] private PlayerProfilePanel _playerProfilerPanel;

        private int _playerReady;
        
        protected override void SwitchToThis()
        {   
            Client.Instance.Lobby.OnLobbyCreated = success => 
            {
                if (!success) return;

                _playerProfilerPanel.AddPlayerProfile(Client.Instance.SteamId);
                
                Debug.Log("lobby created: " + Client.Instance.Lobby.CurrentLobby);
                Debug.Log($"Owner: {Client.Instance.Lobby.Owner}");
                Debug.Log($"Max Members: {Client.Instance.Lobby.MaxMembers}");
                Debug.Log($"Num Members: {Client.Instance.Lobby.NumMembers}");
            };
                
            Client.Instance.Lobby.OnLobbyJoined = success =>
            {
                if (!success) return;
            };
            
            Client.Instance.Lobby.OnLobbyDataUpdated = delegate
            {
                foreach (var member in Client.Instance.Lobby.GetMemberIDs())
                {
                    Debug.Log("kpompompompompompom: " + member);
                    _playerProfilerPanel.ClearPlayerProfiles();
                    _playerProfilerPanel.AddPlayerProfile(member);
                }
            };

            Client.Instance.Lobby.OnLobbyMemberDataUpdated = delegate(ulong member)
            {
                Debug.Log("memememember" + member);
                if (Client.Instance.Lobby.GetMemberData(member, "ready").Equals("true"))
                {
                    ++_playerReady;
                    if (_playerReady >= 1)
                        MenuManager.Instance.MenuState = Types.Menu.MainMenu;
                }
            };
            
            Client.Instance.Lobby.OnLobbyStateChanged = delegate(Lobby.MemberStateChange change, ulong initiator, ulong affectee)
            {
                Debug.Log("yallreadu know");
                switch (change)
                {
                    case Lobby.MemberStateChange.Entered:
                        _playerProfilerPanel.AddPlayerProfile(initiator);
                        break;
                    case Lobby.MemberStateChange.Disconnected:
                        _playerProfilerPanel.RemovePlayerProfile(initiator);
                        break;
                    case Lobby.MemberStateChange.Left:
                        _playerProfilerPanel.RemovePlayerProfile(initiator);
                        break;
                    case Lobby.MemberStateChange.Kicked:
                        break;
                    case Lobby.MemberStateChange.Banned:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(change), change, null);
                }
            };
        }

        public void ReadyToPlay()
        {
            Client.Instance.Lobby.SetMemberData("ready", "true");
            Debug.Log("wedy 2 pway");
            Debug.Log(Client.Instance.Lobby.GetMemberData(Client.Instance.SteamId, "ready"));
        }
        
        public void GoBack()
        {
            _playerProfilerPanel.ClearPlayerProfiles();
            Client.Instance.Lobby.Leave();
            MenuManager.Instance.SwitchToPreviousMenu();
        }
    }
}