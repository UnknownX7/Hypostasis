using System;
using System.Numerics;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Dalamud.Interface;

namespace ImGuiNET;

public static partial class ImGuiEx
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetItemTooltip(string s, ImGuiHoveredFlags flags = ImGuiHoveredFlags.None)
    {
        if (ImGui.IsItemHovered(flags))
            ImGui.SetTooltip(s);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsItemDoubleClicked(ImGuiMouseButton button = ImGuiMouseButton.Left, ImGuiHoveredFlags flags = ImGuiHoveredFlags.None) =>
        ImGui.IsMouseDoubleClicked(button) && ImGui.IsItemHovered(flags);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsItemReleased(ImGuiMouseButton button = ImGuiMouseButton.Left, ImGuiHoveredFlags flags = ImGuiHoveredFlags.None) =>
        ImGui.IsMouseReleased(button) && ImGui.IsItemHovered(flags);

    // Why is this not a basic feature of ImGui...
    private static readonly Stack<float> fontScaleStack = new();
    private static float curScale = 1;
    public static void PushFontScale(float scale)
    {
        ImGui.SetWindowFontScale(scale);
        fontScaleStack.Push(curScale);
        curScale = scale;
    }

    public static void PopFontScale()
    {
        curScale = fontScaleStack.Pop();
        ImGui.SetWindowFontScale(curScale);
    }

    public static void PushFontSize(float size) => PushFontScale(size / ImGui.GetFont().FontSize);

    public static void PopFontSize() => PopFontScale();

    public static float GetFontScale() => curScale;

    public static float GetFontSize() => curScale * ImGui.GetFont().FontSize;

    private static readonly Stack<float> indentStack = new();
    public static void PushIndent(float indent = 0f)
    {
        ImGui.Indent(indent);
        indentStack.Push(indent);
    }

    public static void PopIndent() => ImGui.Unindent(indentStack.Pop());

    public static void ClampWindowPosToViewport()
    {
        var viewport = ImGui.GetWindowViewport();
        if (ImGui.IsWindowAppearing() || viewport.ID != ImGuiHelpers.MainViewport.ID) return;

        var pos = viewport.Pos;
        ClampWindowPos(pos, pos + viewport.Size);
    }

    public static void ClampWindowPos(Vector2 max) => ClampWindowPos(Vector2.Zero, max);

    public static void ClampWindowPos(Vector2 min, Vector2 max)
    {
        var pos = ImGui.GetWindowPos();
        var size = ImGui.GetWindowSize();
        var x = Math.Min(Math.Max(pos.X, min.X), max.X - size.X);
        var y = Math.Min(Math.Max(pos.Y, min.Y), max.Y - size.Y);
        ImGui.SetWindowPos(new Vector2(x, y));
    }

    public static bool IsWindowInMainViewport() => ImGui.GetWindowViewport().ID == ImGuiHelpers.MainViewport.ID;

    public static bool ShouldDrawInViewport() => IsWindowInMainViewport() || Util.IsWindowFocused;

    public static void ShouldDrawInViewport(out bool b) => b = ShouldDrawInViewport();

    // Helper function for displaying / hiding windows outside of the main viewport when the game isn't focused, returns the bool to allow using it in if statements to reduce code
    public static bool SetBoolOnGameFocus(ref bool b)
    {
        if (!b)
            b = Util.IsWindowFocused;
        return b;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string GetClipboardTextOrDefault(string def = "")
    {
        try { return ImGui.GetClipboardText(); }
        catch { return def; }
    }

    // ?????????
    public static void PushClipRectFullScreen() => ImGui.GetWindowDrawList().PushClipRectFullScreen();

    public static void TextCopyable(string text)
    {
        ImGui.TextUnformatted(text);

        if (!ImGui.IsItemHovered()) return;
        ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
        if (ImGui.IsItemClicked())
            ImGui.SetClipboardText(text);
    }

    public static void TextCopyable(Vector4 color, string text)
    {
        ImGui.TextColored(color, text);

        if (!ImGui.IsItemHovered()) return;
        ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
        if (ImGui.IsItemClicked())
            ImGui.SetClipboardText(text);
    }

    public static bool FontButton(string label, ImFontPtr font)
    {
        ImGui.PushFont(font);
        var ret = ImGui.Button(label);
        ImGui.PopFont();
        return ret;
    }

    public static bool FontButton(string label, ImFontPtr font, Vector2 size)
    {
        ImGui.PushFont(font);
        var ret = ImGui.Button(label, size);
        ImGui.PopFont();
        return ret;
    }

    public static bool DeleteConfirmationButton(Vector2 size = default)
    {
        using var _ = FontBlock.Begin(UiBuilder.IconFont);
        ImGui.Button(FontAwesomeIcon.Times.ToIconString(), size);
        if (IsItemReleased(ImGuiMouseButton.Right)) return true;

        using var __ = StyleVarBlock.Begin(ImGuiStyleVar.PopupBorderSize, 1);
        if (!ImGui.BeginPopupContextItem(null, ImGuiPopupFlags.MouseButtonLeft)) return false;
        var ret = ImGui.Selectable(FontAwesomeIcon.TrashAlt.ToIconString());
        ImGui.EndPopup();
        return ret;
    }

    // No way to block the title bar
    public static void BlockWindowDrag()
    {
        var io = ImGui.GetIO();
        var prev = io.ConfigWindowsMoveFromTitleBarOnly;
        io.ConfigWindowsMoveFromTitleBarOnly = true;
        DalamudApi.Framework.RunOnTick(() => io.ConfigWindowsMoveFromTitleBarOnly = prev);
    }

    private static void AddTextCentered(Vector2 pos, string text, uint color)
    {
        var textSize = ImGui.CalcTextSize(text);
        ImGui.GetWindowDrawList().AddText(pos - textSize / 2, color, text);
    }

    public static void Prefix(string prefix = "└") // └◇└
    {
        var dummySize = new Vector2(ImGui.GetFrameHeight());
        ImGui.Dummy(dummySize);
        AddTextCentered(ImGui.GetItemRectMin() + dummySize / 2, prefix, ImGui.GetColorU32(ImGuiCol.Text));
        ImGui.SameLine();
    }

    public static bool RadioBox(string label, ref int v, string[] optionsArray, bool vertical)
    {
        if (!BeginGroupBox(label, 0)) return false;

        var ret = false;
        var numOptions = optionsArray.Length;

        ImGui.PushID(label);
        for (int i = 0; i < numOptions; i++)
        {
            var option = optionsArray[i];
            var selected = v == i;
            ret |= ImGui.RadioButton(vertical ? option : $"##{i}", ref v, i) && !selected;
            if (vertical) continue;
            SetItemTooltip(option);
            if (i != numOptions - 1)
                ImGui.SameLine();
        }
        ImGui.PopID();


        if (!vertical && v >= 0 && v < numOptions)
        {
            ImGui.SameLine();
            ImGui.TextUnformatted(optionsArray[v]);
        }

        ImGui.SameLine();
        ImGui.Dummy(Vector2.Zero);

        EndGroupBox();
        return ret;
    }

    public static bool RadioBox(string label, ref int v, string options, bool vertical) => RadioBox(label, ref v, options.Split('\0'), vertical);

    public static bool CheckboxTristate(string label, ref bool? v)
    {
        bool ret;

        var unset = !v.HasValue;
        if (unset)
        {
            var _ = false;
            ret = ImGui.Checkbox(label, ref _);
            if (ret)
                v = true;

            var size = ImGui.GetFrameHeight();
            var padSize = Math.Max(MathF.Floor(size / 4), 1);
            var padding = new Vector2(padSize);
            var min = ImGui.GetItemRectMin();
            var max = min + new Vector2(size);
            ImGui.GetWindowDrawList().AddRect(min + padding, max - padding, ImGui.GetColorU32(ImGuiCol.CheckMark), ImGui.GetStyle().FrameRounding, ImDrawFlags.None, 3 * ImGuiHelpers.GlobalScale);
        }
        else
        {
            var value = v.Value;
            var isFalse = !value;

            if (isFalse)
                ImGui.PushStyleColor(ImGuiCol.CheckMark, Vector4.Zero);

            ret = ImGui.Checkbox(label, ref value);
            if (ret)
                v = value ? null : false;

            if (isFalse)
                ImGui.PopStyleColor();
        }

        return ret;
    }
}