﻿using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Interface.ImGuiFileDialog;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using MareSynchronos.MareConfiguration;
using MareSynchronos.PlayerData.Export;
using MareSynchronos.Services;
using MareSynchronos.Services.Mediator;
using Microsoft.Extensions.Logging;

namespace MareSynchronos.UI;

public class GposeUi : WindowMediatorSubscriberBase, IDisposable
{
    private readonly WindowSystem _windowSystem;
    private readonly MareCharaFileManager _mareCharaFileManager;
    private readonly DalamudUtil _dalamudUtil;
    private readonly FileDialogManager _fileDialogManager;
    private readonly MareConfigService _configService;

    public GposeUi(ILogger<GposeUi> logger, WindowSystem windowSystem, MareCharaFileManager mareCharaFileManager,
        DalamudUtil dalamudUtil, FileDialogManager fileDialogManager, MareConfigService configService,
        MareMediator mediator) : base(logger, mediator, "Mare Synchronos Gpose Import UI###MareSynchronosGposeUI")
    {
        _windowSystem = windowSystem;
        _mareCharaFileManager = mareCharaFileManager;
        _dalamudUtil = dalamudUtil;
        _fileDialogManager = fileDialogManager;
        _configService = configService;
        Mediator.Subscribe<GposeStartMessage>(this, (_) => StartGpose());
        Mediator.Subscribe<GposeEndMessage>(this, (_) => EndGpose());
        IsOpen = _dalamudUtil.IsInGpose;
        Flags = ImGuiWindowFlags.AlwaysAutoResize;
        _windowSystem.AddWindow(this);
    }

    private void EndGpose()
    {
        IsOpen = false;
        _mareCharaFileManager.ClearMareCharaFile();
    }

    private void StartGpose()
    {
        IsOpen = _configService.Current.OpenGposeImportOnGposeStart;
    }

    public override void Dispose()
    {
        base.Dispose();
        _windowSystem.RemoveWindow(this);
    }

    public override void Draw()
    {
        if (!_dalamudUtil.IsInGpose) IsOpen = false;

        if (!_mareCharaFileManager.CurrentlyWorking)
        {
            if (UiShared.IconTextButton(FontAwesomeIcon.FolderOpen, "Load MCDF"))
            {
                _fileDialogManager.OpenFileDialog("Pick MCDF file", ".mcdf", (success, path) =>
                {
                    if (!success) return;

                    Task.Run(() => _mareCharaFileManager.LoadMareCharaFile(path));
                });
            }
            UiShared.AttachToolTip("Applies it to the currently selected GPose actor");
            if (_mareCharaFileManager.LoadedCharaFile != null)
            {
                UiShared.TextWrapped("Loaded file: " + _mareCharaFileManager.LoadedCharaFile.FilePath);
                UiShared.TextWrapped("File Description: " + _mareCharaFileManager.LoadedCharaFile.CharaFileData.Description);
                if (UiShared.IconTextButton(FontAwesomeIcon.Check, "Apply loaded MCDF"))
                {
                    Task.Run(async () => await _mareCharaFileManager.ApplyMareCharaFile(_dalamudUtil.GposeTargetGameObject).ConfigureAwait(false));
                }
                UiShared.AttachToolTip("Applies it to the currently selected GPose actor");
                UiShared.ColorTextWrapped("Warning: redrawing or changing the character will revert all applied mods.", ImGuiColors.DalamudYellow);
            }
        }
        else
        {
            UiShared.ColorTextWrapped("Loading Character...", ImGuiColors.DalamudYellow);
        }
        UiShared.TextWrapped("Hint: You can disable the automatic loading of this window in the Mare settings and open it manually with /mare gpose");
    }
}
