using System;
using System.Numerics;
using Dalamud.Interface;

namespace ImGuiNET;

public static partial class ImGuiEx
{
    private static bool sliderEnabled = false;
    private static bool sliderVertical = false;
    private static float sliderInterval = 0;
    private static int lastHitInterval = 0;
    private static Action<bool, bool, bool> sliderAction;

    public static void SetupSlider(bool vertical, float interval, Action<bool, bool, bool> action)
    {
        sliderEnabled = true;
        sliderVertical = vertical;
        sliderInterval = interval;
        lastHitInterval = 0;
        sliderAction = action;
    }

    public static void DoSlider()
    {
        if (!sliderEnabled) return;

        // You can blame ImGui for this
        var popupOpen = !ImGui.IsPopupOpen("_SLIDER") && ImGui.IsPopupOpen(null, ImGuiPopupFlags.AnyPopup);
        if (!popupOpen)
        {
            ImGuiHelpers.ForceNextWindowMainViewport();
            ImGui.SetNextWindowPos(new Vector2(-100));
            ImGui.OpenPopup("_SLIDER", ImGuiPopupFlags.NoOpenOverItems);
            if (!ImGui.BeginPopup("_SLIDER")) return;
        }

        var drag = sliderVertical ? ImGui.GetMouseDragDelta().Y : ImGui.GetMouseDragDelta().X;
        var dragInterval = (int)(drag / sliderInterval);
        var hit = false;
        var increment = false;
        if (dragInterval > lastHitInterval)
        {
            hit = true;
            increment = true;
        }
        else if (dragInterval < lastHitInterval)
            hit = true;

        var closing = !ImGui.IsMouseDown(ImGuiMouseButton.Left);

        if (lastHitInterval != dragInterval)
        {
            while (lastHitInterval != dragInterval)
            {
                lastHitInterval += increment ? 1 : -1;
                sliderAction(hit, increment, closing && lastHitInterval == dragInterval);
            }
        }
        else
            sliderAction(false, false, closing);

        if (closing)
            sliderEnabled = false;

        if (!popupOpen)
            ImGui.EndPopup();
    }
}