using Grauenwolf.TravellerTools.Maps;
using Grauenwolf.TravellerTools.Shared;
using Grauenwolf.TravellerTools.Web.Data;
using Microsoft.AspNetCore.Components;
using System.ComponentModel;

namespace Grauenwolf.TravellerTools.Web.Controls;

partial class WorldPicker
{
    bool m_IsApplyingRouteSelection;
    string? m_LastMilieuCode;
    string? m_LastSectorHex;
    string? m_LastWorldHex;

    [Parameter] public string? SelectedMilieuCode { get; set; }
    [Parameter] public string? SelectedSectorHex { get; set; }
    [Parameter] public string? SelectedWorldHex { get; set; }
    [Parameter] public bool ShowWorldActionLinks { get; set; } = true;
    [Parameter] public string LoadSuffix { get; set; } = "info";

    protected bool SectorNotSelected => Model.SelectedSector == null;

    protected bool SubsectorNotSelected => Model.SelectedSubsector == null;

    protected bool WorldNotSelected => Model.SelectedWorld == null;
    protected string? LoadWorldUrl => PlanetUrl(LoadSuffix);

    //PlanetPickerOptions Model { get; } = new PlanetPickerOptions();
    [Inject] TravellerMapServiceLocator TravellerMapServiceLocator { get; set; } = null!;

    protected override async Task InitializedAsync()
    {
        await ApplySelectionFromParametersAsync();
    }

    protected override async Task ParametersSetAsync()
    {
        await ApplySelectionFromParametersAsync();
    }

    protected override async void OnModelPropertyChanged(PropertyChangedEventArgs e)
    {
        if (m_IsApplyingRouteSelection)
            return;

        switch (e.PropertyName)
        {
            case nameof(PlanetPickerOptions.SelectedMilieu): await OnMilieuChangedAsync(); break;
            case nameof(PlanetPickerOptions.SelectedSector): await OnSectorChangedAsync(); break;
            case nameof(PlanetPickerOptions.SelectedSubsector): await OnSubsectorChangedAsync(); break;
        }
    }

    protected string? PlanetUrl(string suffix)
    {
        if (Model.SelectedMilieuCode == null || Model.SelectedSectorHex == null || Model.SelectedWorldHex == null)
            return null;
        return $"/world/{Model.SelectedMilieuCode}/{Model.SelectedSectorHex}/{Model.SelectedWorldHex}/{suffix}";
    }

    protected void LoadSelectedWorld()
    {
        var url = LoadWorldUrl;
        if (url == null)
            return;

        Navigation.NavigateTo(url);
    }

    protected string? SectorTasUrl()
    {
        if (Model.SelectedMilieuCode == null || Model.SelectedSector == null)
            return null;
        return $"https://wiki.travellerrpg.com/{Model.SelectedSector.Name}_Sector";
    }

    protected string? SectorUrl()
    {
        if (Model.SelectedMilieuCode == null || Model.SelectedSectorHex == null)
            return null;
        return $"/world/{Model.SelectedMilieuCode}/{Model.SelectedSectorHex}";
    }

    protected string? SubsectorTasUrl()
    {
        if (Model.SelectedMilieuCode == null || Model.SelectedSectorHex == null || Model.SelectedSubsectorIndex == null)
            return null;
        return $"https://wiki.travellerrpg.com/{Model.SelectedSubsector!.Name}_Subsector";
    }

    protected string? SubsectorUrl()
    {
        if (Model.SelectedMilieuCode == null || Model.SelectedSectorHex == null || Model.SelectedSubsectorIndex == null)
            return null;
        return $"/world/{Model.SelectedMilieuCode}/{Model.SelectedSectorHex}/subsector/{Model.SelectedSubsectorIndex}";
    }

    async Task OnMilieuChangedAsync()
    {
        if (Model.SelectedMilieu == null)
            Model.SelectedMilieu = Milieu.DefaultMilieu;

        var service = TravellerMapServiceLocator.GetMapService(Model.SelectedMilieu!);
        Model.SectorList = await service.FetchUniverseAsync();
    }

    async Task OnSectorChangedAsync()
    {
        if (Model.SelectedSector == null)
        {
            Model.SubsectorList = Array.Empty<Subsector>();
            Model.WorldList = Array.Empty<World>();
            Model.SelectedSubsector = null;
            Model.SelectedWorld = null;
            return;
        }

        var service = TravellerMapServiceLocator.GetMapService(Model.SelectedMilieu!);
        Model.SubsectorList = await service.FetchSubsectorsInSectorAsync(Model.SelectedSector);
        Model.SelectedSubsector = null;
        Model.SelectedWorld = null;
    }

    async Task OnSubsectorChangedAsync()
    {
        if (Model.SelectedSector == null || Model.SelectedSubsector == null)
        {
            Model.WorldList = Array.Empty<World>();
            Model.SelectedWorld = null;
            return;
        }

        var service = TravellerMapServiceLocator.GetMapService(Model.SelectedMilieu!);
        Model.WorldList = await service.FetchWorldsInSubsectorAsync(Model.SelectedSector.X, Model.SelectedSector.Y, Model.SelectedSubsector.Index!, Model.SelectedSector.Name!);
        Model.SelectedWorld = null;
    }

    async Task ApplySelectionFromParametersAsync()
    {
        if (m_LastMilieuCode == SelectedMilieuCode && m_LastSectorHex == SelectedSectorHex && m_LastWorldHex == SelectedWorldHex)
            return;

        m_LastMilieuCode = SelectedMilieuCode;
        m_LastSectorHex = SelectedSectorHex;
        m_LastWorldHex = SelectedWorldHex;

        try
        {
            m_IsApplyingRouteSelection = true;

            Model.SelectedMilieuCode = SelectedMilieuCode;
            await OnMilieuChangedAsync();

            if (string.IsNullOrWhiteSpace(SelectedSectorHex))
            {
                Model.SelectedSector = null;
                Model.SelectedSubsector = null;
                Model.SelectedWorld = null;
                Model.SubsectorList = Array.Empty<Subsector>();
                Model.WorldList = Array.Empty<World>();
                return;
            }

            Model.SelectedSectorHex = SelectedSectorHex;
            await OnSectorChangedAsync();

            if (string.IsNullOrWhiteSpace(SelectedWorldHex))
                return;

            var subsectorIndex = GetSubsectorIndex(SelectedWorldHex);
            if (subsectorIndex == null)
                return;

            Model.SelectedSubsectorIndex = subsectorIndex;
            await OnSubsectorChangedAsync();
            Model.SelectedWorldHex = SelectedWorldHex;
        }
        finally
        {
            m_IsApplyingRouteSelection = false;
        }
    }

    static string? GetSubsectorIndex(string worldHex)
    {
        if (worldHex.Length != 4)
            return null;

        if (!int.TryParse(worldHex[..2], out var hexX) || !int.TryParse(worldHex[2..], out var hexY))
            return null;

        if (hexX < 1 || hexX > 32 || hexY < 1 || hexY > 40)
            return null;

        var column = (hexX - 1) / 8;
        var row = (hexY - 1) / 10;
        return ((char)('A' + (row * 4) + column)).ToString();
    }
}
