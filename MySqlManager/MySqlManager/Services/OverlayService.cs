using Microsoft.AspNetCore.Components;

namespace MySqlManager.Services;

public class OverlayService
{
    public bool OverlayVisible { get; private set; }
    public string? OverlayHeader { get; private set; }
    public RenderFragment? OverlayBody { get; private set; }
    
    public event Action? OnOverlayChanged;

    public void SetOverlay(string header, RenderFragment body)
    {
        OverlayHeader = header;
        OverlayBody = body;
        // OnOverlayChanged?.Invoke();
    }

    public void ShowOverlay()
    {
        OverlayVisible = true;
        OnOverlayChanged?.Invoke();
    }
    
    public void HideOverlay()
    {
        OverlayVisible = false;
        OnOverlayChanged?.Invoke();
    }
    
}