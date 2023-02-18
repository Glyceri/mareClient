﻿using Dalamud.Utility;
using MareSynchronos.API.Data;
using MareSynchronos.API.Data.Comparer;
using MareSynchronos.API.Data.Extensions;
using MareSynchronos.API.Dto.Group;
using MareSynchronos.API.Dto.User;
using MareSynchronos.Managers;
using MareSynchronos.MareConfiguration;
using MareSynchronos.Utils;
using Microsoft.Extensions.Logging;

namespace MareSynchronos.Models;

public class Pair
{
    private readonly ILogger<Pair> _logger;
    private readonly MareConfigService _configService;
    private readonly ServerConfigurationManager _serverConfigurationManager;
    private OptionalPluginWarning? _pluginWarnings;

    public Pair(ILogger<Pair> logger, MareConfigService configService, ServerConfigurationManager serverConfigurationManager)
    {
        _logger = logger;
        _configService = configService;
        _serverConfigurationManager = serverConfigurationManager;
    }

    public UserPairDto? UserPair { get; set; }
    public CachedPlayer? CachedPlayer { get; set; }
    public API.Data.CharacterData? LastReceivedCharacterData { get; set; }
    public Dictionary<GroupFullInfoDto, GroupPairFullInfoDto> GroupPair { get; set; } = new(GroupDtoComparer.Instance);
    public string PlayerNameHash => CachedPlayer?.PlayerNameHash ?? string.Empty;
    public string? PlayerName => CachedPlayer?.PlayerName ?? string.Empty;
    public UserData UserData => UserPair?.User ?? GroupPair.First().Value.User;
    public bool IsOnline => CachedPlayer != null;
    public bool IsVisible => CachedPlayer?.PlayerName != null;
    public bool IsPaused => UserPair != null && UserPair.OtherPermissions.IsPaired() ? (UserPair.OtherPermissions.IsPaused() || UserPair.OwnPermissions.IsPaused())
            : GroupPair.All(p => p.Key.GroupUserPermissions.IsPaused() || p.Value.GroupUserPermissions.IsPaused());

    public string? GetNote()
    {
        return _serverConfigurationManager.GetNoteForUid(UserData.UID);
    }

    public void SetNote(string note)
    {
        _serverConfigurationManager.SetNoteForUid(UserData.UID, note);
    }

    public bool HasAnyConnection()
    {
        return UserPair != null || GroupPair.Any();
    }

    public bool InitializePair(string name)
    {
        if (!PlayerName.IsNullOrEmpty()) return false;

        if (CachedPlayer == null) throw new InvalidOperationException("CachedPlayer not initialized");
        _pluginWarnings ??= new()
        {
            ShownCustomizePlusWarning = _configService.Current.DisableOptionalPluginWarnings,
            ShownHeelsWarning = _configService.Current.DisableOptionalPluginWarnings,
            ShownPalettePlusWarning = _configService.Current.DisableOptionalPluginWarnings,
        };

        CachedPlayer.Initialize(name);

        ApplyLastReceivedData();

        return true;
    }

    public void ApplyData(OnlineUserCharaDataDto data)
    {
        if (CachedPlayer == null) throw new InvalidOperationException("CachedPlayer not initialized");

        if (string.Equals(LastReceivedCharacterData?.DataHash.Value, data.CharaData.DataHash.Value, StringComparison.Ordinal)) return;

        LastReceivedCharacterData = data.CharaData;

        ApplyLastReceivedData();
    }

    public void ApplyLastReceivedData(bool forced = false)
    {
        if (CachedPlayer == null) return;
        if (LastReceivedCharacterData == null) return;

        _pluginWarnings ??= new()
        {
            ShownCustomizePlusWarning = _configService.Current.DisableOptionalPluginWarnings,
            ShownHeelsWarning = _configService.Current.DisableOptionalPluginWarnings,
            ShownPalettePlusWarning = _configService.Current.DisableOptionalPluginWarnings,
        };

        CachedPlayer.ApplyCharacterData(RemoveNotSyncedFiles(LastReceivedCharacterData.DeepClone())!, _pluginWarnings, forced);
    }

    private API.Data.CharacterData? RemoveNotSyncedFiles(API.Data.CharacterData? data)
    {
        _logger.LogTrace("Removing not synced files");
        if (data == null || (UserPair != null && UserPair.OtherPermissions.IsPaired()))
        {
            _logger.LogTrace("Nothing to remove or user is paired directly");
            return data;
        }

        bool disableAnimations = GroupPair.All(pair =>
        {
            return pair.Value.GroupUserPermissions.IsDisableAnimations() || pair.Key.GroupPermissions.IsDisableAnimations() || pair.Key.GroupUserPermissions.IsDisableAnimations();
        });
        bool disableSounds = GroupPair.All(pair =>
        {
            return pair.Value.GroupUserPermissions.IsDisableSounds() || pair.Key.GroupPermissions.IsDisableSounds() || pair.Key.GroupUserPermissions.IsDisableSounds();
        });

        if (disableAnimations || disableSounds)
        {
            _logger.LogTrace($"Data cleaned up: Animations disabled: {disableAnimations}, Sounds disabled: {disableSounds}");
            foreach (var kvp in data.FileReplacements)
            {
                if (disableSounds)
                    data.FileReplacements[kvp.Key] = data.FileReplacements[kvp.Key]
                        .Where(f => !f.GamePaths.Any(p => p.EndsWith("scd", StringComparison.OrdinalIgnoreCase)))
                        .ToList();
                if (disableAnimations)
                    data.FileReplacements[kvp.Key] = data.FileReplacements[kvp.Key]
                        .Where(f => !f.GamePaths.Any(p => p.EndsWith("tmb", StringComparison.OrdinalIgnoreCase) || p.EndsWith("pap", StringComparison.OrdinalIgnoreCase)))
                        .ToList();
            }
        }

        return data;
    }
}
