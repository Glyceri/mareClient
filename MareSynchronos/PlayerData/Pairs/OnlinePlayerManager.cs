﻿using MareSynchronos.API.Data;
using MareSynchronos.FileCache;
using MareSynchronos.Services;
using MareSynchronos.Services.Mediator;
using MareSynchronos.Utils;
using MareSynchronos.WebAPI;
using MareSynchronos.WebAPI.Files;
using Microsoft.Extensions.Logging;

namespace MareSynchronos.PlayerData.Pairs;

public class OnlinePlayerManager : MediatorSubscriberBase
{
    private readonly ApiController _apiController;
    private readonly DalamudUtil _dalamudUtil;
    private readonly PairManager _pairManager;
    private readonly FileUploadManager _fileTransferManager;
    private CharacterData? _lastSentData;

    public OnlinePlayerManager(ILogger<OnlinePlayerManager> logger, ApiController apiController, DalamudUtil dalamudUtil,
        PairManager pairManager, MareMediator mediator, FileUploadManager fileTransferManager) : base(logger, mediator)
    {
        _logger.LogTrace("Creating " + nameof(OnlinePlayerManager));
        _apiController = apiController;
        _dalamudUtil = dalamudUtil;
        _pairManager = pairManager;
        _fileTransferManager = fileTransferManager;
        Mediator.Subscribe<PlayerChangedMessage>(this, (_) => PlayerManagerOnPlayerHasChanged());
        Mediator.Subscribe<DelayedFrameworkUpdateMessage>(this, (_) => FrameworkOnUpdate());
        Mediator.Subscribe<CharacterDataCreatedMessage>(this, (msg) =>
        {
            var newData = ((CharacterDataCreatedMessage)msg).CharacterData;
            if (_lastSentData == null || (!string.Equals(newData.DataHash.Value, _lastSentData.DataHash.Value, StringComparison.Ordinal)))
            {
                _logger.LogDebug("Pushing data for visible players");
                _lastSentData = newData;
                PushCharacterData(_pairManager.GetVisibleUsers());
            }
            else
            {
                _logger.LogDebug("Not sending data for {hash}", newData.DataHash.Value);
            }
        });
    }

    private void PlayerManagerOnPlayerHasChanged()
    {
        PushCharacterData(_pairManager.GetVisibleUsers());
    }

    private void FrameworkOnUpdate()
    {
        if (!_dalamudUtil.IsPlayerPresent || !_apiController.IsConnected) return;

        var playerCharacters = _dalamudUtil.GetPlayerCharacters();
        var newVisiblePlayers = new List<UserData>();
        foreach (var pChar in playerCharacters)
        {
            var pair = _pairManager.FindPair(pChar);
            if (pair == null) continue;

            if (pair.InitializePair(pChar.Name.ToString()))
            {
                newVisiblePlayers.Add(pair.UserData ?? pair.GroupPair.First().Value.User);
            }
        }

        if (newVisiblePlayers.Any())
        {
            _logger.LogTrace("Has new visible players, pushing character data");
            PushCharacterData(newVisiblePlayers);
        }
    }

    private void PushCharacterData(List<UserData> visiblePlayers)
    {
        if (visiblePlayers.Any() && _lastSentData != null)
        {
            Task.Run(async () =>
            {
                var dataToSend = await _fileTransferManager.UploadFiles(_lastSentData.DeepClone()).ConfigureAwait(false);
                await _apiController.PushCharacterData(dataToSend, visiblePlayers).ConfigureAwait(false);
            });
        }
    }
}