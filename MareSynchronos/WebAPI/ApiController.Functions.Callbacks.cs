﻿using Dalamud.Interface.Internal.Notifications;
using MareSynchronos.API.Data.Enum;
using MareSynchronos.API.Dto;
using MareSynchronos.API.Dto.Group;
using MareSynchronos.API.Dto.User;
using MareSynchronos.Mediator;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;

namespace MareSynchronos.WebAPI;

public partial class ApiController
{
    private void ExecuteSafely(Action act)
    {
        try
        {
            act();
        }
        catch (Exception ex)
        {
            _logger.LogCritical("Error on executing safely", ex);
        }
    }

    public void OnUpdateSystemInfo(Action<SystemInfoDto> act)
    {
        if (_initialized) return;
        _mareHub!.On(nameof(Client_UpdateSystemInfo), act);
    }

    public void OnUserReceiveCharacterData(Action<OnlineUserCharaDataDto> act)
    {
        if (_initialized) return;
        _mareHub!.On(nameof(Client_UserReceiveCharacterData), act);
    }

    public void OnReceiveServerMessage(Action<MessageSeverity, string> act)
    {
        if (_initialized) return;
        _mareHub!.On(nameof(Client_ReceiveServerMessage), act);
    }

    public void OnDownloadReady(Action<Guid> act)
    {
        if (_initialized) return;
        _mareHub!.On(nameof(Client_DownloadReady), act);
    }

    public void OnGroupSendFullInfo(Action<GroupFullInfoDto> act)
    {
        if (_initialized) return;
        _mareHub!.On(nameof(Client_GroupSendFullInfo), act);
    }

    public Task Client_GroupSendFullInfo(GroupFullInfoDto dto)
    {
        _logger.LogTrace("Client_GroupSendFullInfo: " + dto);
        ExecuteSafely(() => _pairManager.AddGroup(dto));
        return Task.CompletedTask;
    }

    public void OnGroupSendInfo(Action<GroupInfoDto> act)
    {
        if (_initialized) return;
        _mareHub!.On(nameof(Client_GroupSendInfo), act);
    }

    public Task Client_GroupSendInfo(GroupInfoDto dto)
    {
        _logger.LogTrace("Client_GroupSendInfo: " + dto);
        ExecuteSafely(() => _pairManager.SetGroupInfo(dto));
        return Task.CompletedTask;
    }

    public void OnGroupDelete(Action<GroupDto> act)
    {
        if (_initialized) return;
        _mareHub!.On(nameof(Client_GroupDelete), act);
    }

    public Task Client_GroupDelete(GroupDto dto)
    {
        _logger.LogTrace("Client_GroupDelete: " + dto);
        ExecuteSafely(() => _pairManager.RemoveGroup(dto.Group));
        return Task.CompletedTask;
    }

    public void OnGroupPairJoined(Action<GroupPairFullInfoDto> act)
    {
        if (_initialized) return;
        _mareHub!.On(nameof(Client_GroupPairJoined), act);
    }

    public Task Client_GroupPairJoined(GroupPairFullInfoDto dto)
    {
        _logger.LogTrace("Client_GroupPairJoined: " + dto);
        ExecuteSafely(() => _pairManager.AddGroupPair(dto));
        return Task.CompletedTask;
    }

    public void OnGroupPairLeft(Action<GroupPairDto> act)
    {
        if (_initialized) return;
        _mareHub!.On(nameof(Client_GroupPairLeft), act);
    }

    public Task Client_GroupPairLeft(GroupPairDto dto)
    {
        _logger.LogTrace("Client_GroupPairLeft: " + dto);
        ExecuteSafely(() => _pairManager.RemoveGroupPair(dto));
        return Task.CompletedTask;
    }

    public void OnGroupChangePermissions(Action<GroupPermissionDto> act)
    {
        if (_initialized) return;
        _mareHub!.On(nameof(Client_GroupChangePermissions), act);
    }

    public Task Client_GroupChangePermissions(GroupPermissionDto dto)
    {
        _logger.LogTrace("Client_GroupChangePermissions: " + dto);
        ExecuteSafely(() => _pairManager.SetGroupPermissions(dto));
        return Task.CompletedTask;
    }

    public void OnGroupPairChangePermissions(Action<GroupPairUserPermissionDto> act)
    {
        if (_initialized) return;
        _mareHub!.On(nameof(Client_GroupPairChangePermissions), act);
    }

    public Task Client_GroupPairChangePermissions(GroupPairUserPermissionDto dto)
    {
        _logger.LogTrace("Client_GroupPairChangePermissions: " + dto);
        ExecuteSafely(() =>
        {
            if (string.Equals(dto.UID, UID, StringComparison.Ordinal)) _pairManager.SetGroupUserPermissions(dto);
            else _pairManager.SetGroupPairUserPermissions(dto);
        });
        return Task.CompletedTask;
    }

    public void OnGroupPairChangeUserInfo(Action<GroupPairUserInfoDto> act)
    {
        if (_initialized) return;
        _mareHub!.On(nameof(Client_GroupPairChangeUserInfo), act);
    }

    public Task Client_GroupPairChangeUserInfo(GroupPairUserInfoDto dto)
    {
        _logger.LogTrace("Client_GroupPairChangeUserInfo: " + dto);
        ExecuteSafely(() =>
        {
            if (string.Equals(dto.UID, UID, StringComparison.Ordinal)) _pairManager.SetGroupStatusInfo(dto);
            else _pairManager.SetGroupPairStatusInfo(dto);
        });
        return Task.CompletedTask;
    }

    public Task Client_UserReceiveCharacterData(OnlineUserCharaDataDto dto)
    {
        _logger.LogTrace("Client_UserReceiveCharacterData: " + dto.User);
        ExecuteSafely(() => _pairManager.ReceiveCharaData(dto));
        return Task.CompletedTask;
    }

    public void OnUserAddClientPair(Action<UserPairDto> act)
    {
        if (_initialized) return;
        _mareHub!.On(nameof(Client_UserAddClientPair), act);
    }

    public Task Client_UserAddClientPair(UserPairDto dto)
    {
        _logger.LogDebug($"Client_UserAddClientPair: " + dto);
        ExecuteSafely(() => _pairManager.AddUserPair(dto));
        return Task.CompletedTask;
    }

    public void OnUserRemoveClientPair(Action<UserDto> act)
    {
        if (_initialized) return;
        _mareHub!.On(nameof(Client_UserRemoveClientPair), act);
    }

    public Task Client_UserRemoveClientPair(UserDto dto)
    {
        _logger.LogDebug($"Client_UserRemoveClientPair: " + dto);
        ExecuteSafely(() => _pairManager.RemoveUserPair(dto));
        return Task.CompletedTask;
    }

    public void OnUserSendOffline(Action<UserDto> act)
    {
        if (_initialized) return;
        _mareHub!.On(nameof(Client_UserSendOffline), act);
    }

    public Task Client_UserSendOffline(UserDto dto)
    {
        _logger.LogDebug($"Client_UserSendOffline: {dto}");
        ExecuteSafely(() => _pairManager.MarkPairOffline(dto.User));
        return Task.CompletedTask;
    }

    public void OnUserSendOnline(Action<OnlineUserIdentDto> act)
    {
        if (_initialized) return;
        _mareHub!.On(nameof(Client_UserSendOnline), act);
    }

    public Task Client_UserSendOnline(OnlineUserIdentDto dto)
    {
        _logger.LogDebug($"Client_UserSendOnline: {dto}");
        ExecuteSafely(() => _pairManager.MarkPairOnline(dto, this));
        return Task.CompletedTask;
    }

    public void OnUserUpdateOtherPairPermissions(Action<UserPermissionsDto> act)
    {
        if (_initialized) return;
        _mareHub!.On(nameof(Client_UserUpdateOtherPairPermissions), act);
    }

    public Task Client_UserUpdateOtherPairPermissions(UserPermissionsDto dto)
    {
        _logger.LogDebug($"Client_UserUpdateOtherPairPermissions: {dto}");
        ExecuteSafely(() => _pairManager.UpdatePairPermissions(dto));
        return Task.CompletedTask;
    }

    public void OnUserUpdateSelfPairPermissions(Action<UserPermissionsDto> act)
    {
        if (_initialized) return;
        _mareHub!.On(nameof(Client_UserUpdateSelfPairPermissions), act);
    }

    public Task Client_UserUpdateSelfPairPermissions(UserPermissionsDto dto)
    {
        _logger.LogDebug($"Client_UserUpdateSelfPairPermissions: {dto}");
        ExecuteSafely(() => _pairManager.UpdateSelfPairPermissions(dto));
        return Task.CompletedTask;
    }

    public Task Client_UpdateSystemInfo(SystemInfoDto systemInfo)
    {
        SystemInfoDto = systemInfo;
        return Task.CompletedTask;
    }

    public Task Client_ReceiveServerMessage(MessageSeverity severity, string message)
    {
        switch (severity)
        {
            case MessageSeverity.Error:
                Mediator.Publish(new NotificationMessage("Warning from " + _serverManager.CurrentServer!.ServerName, message, NotificationType.Error, 7500));
                break;
            case MessageSeverity.Warning:
                Mediator.Publish(new NotificationMessage("Warning from " + _serverManager.CurrentServer!.ServerName, message, NotificationType.Warning, 7500));
                break;
            case MessageSeverity.Information:
                if (_doNotNotifiyOnNextInfo)
                {
                    _doNotNotifiyOnNextInfo = false;
                    break;
                }
                Mediator.Publish(new NotificationMessage("Info from " + _serverManager.CurrentServer!.ServerName, message, NotificationType.Info, 5000));
                break;
        }

        return Task.CompletedTask;
    }

    public Task Client_DownloadReady(Guid requestId)
    {
        _logger.LogDebug($"Server sent {requestId} ready");
        _downloadReady[requestId] = true;
        return Task.CompletedTask;
    }
}
