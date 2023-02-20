using System;
using System.Numerics;

namespace ImGuiNET;

public static partial class ImGuiEx
{
    public sealed class IDBlock : IDisposable
    {
        public IDBlock(int id) => ImGui.PushID(id);
        public IDBlock(nint id) => ImGui.PushID(id);
        public IDBlock(string id) => ImGui.PushID(id);
        public void Dispose() => ImGui.PopID();
    }

    public sealed class StyleVarBlock : IDisposable
    {
        public StyleVarBlock(ImGuiStyleVar idx, float val) => ImGui.PushStyleVar(idx, val);
        public StyleVarBlock(ImGuiStyleVar idx, Vector2 val) => ImGui.PushStyleVar(idx, val);
        public void Dispose() => ImGui.PopStyleVar();
    }

    public sealed class StyleColorBlock : IDisposable
    {
        public StyleColorBlock(ImGuiCol idx, uint val) => ImGui.PushStyleColor(idx, val);
        public StyleColorBlock(ImGuiCol idx, Vector4 val) => ImGui.PushStyleColor(idx, val);
        public void Dispose() => ImGui.PopStyleColor();
    }

    public sealed class IndentBlock : IDisposable
    {
        private readonly float? value;

        public IndentBlock() => ImGui.Indent();

        public IndentBlock(float indent)
        {
            value = indent;
            ImGui.Indent(indent);
        }

        public void Dispose()
        {
            if (value.HasValue)
                ImGui.Unindent(value.Value);
            else
                ImGui.Unindent();
        }
    }

    public sealed class FontBlock : IDisposable
    {
        public FontBlock(ImFontPtr font) => ImGui.PushFont(font);
        public void Dispose() => ImGui.PopFont();
    }

    public sealed class GroupBlock : IDisposable
    {
        public GroupBlock() => ImGui.BeginGroup();
        public void Dispose() => ImGui.EndGroup();
    }

    public sealed class ClipRectBlock : IDisposable
    {
        public ClipRectBlock(Vector2 min, Vector2 max, bool overlap = true) => ImGui.PushClipRect(min, max, overlap);
        public void Dispose() => ImGui.PopClipRect();
    }

    public sealed class TooltipBlock : IDisposable
    {
        public TooltipBlock() => ImGui.BeginTooltip();
        public void Dispose() => ImGui.EndTooltip();
    }

    public sealed class DisabledBlock : IDisposable
    {
        public DisabledBlock(bool disable = true) => ImGui.BeginDisabled(disable);
        public void Dispose() => ImGui.EndDisabled();
    }

    public sealed class AllowKeyboardFocusBlock : IDisposable
    {
        public AllowKeyboardFocusBlock(bool allow = false) => ImGui.PushAllowKeyboardFocus(allow);
        public void Dispose() => ImGui.PopAllowKeyboardFocus();
    }

    public sealed class ButtonRepeatBlock : IDisposable
    {
        public ButtonRepeatBlock(bool repeat = true) => ImGui.PushButtonRepeat(repeat);
        public void Dispose() => ImGui.PopButtonRepeat();
    }

    public sealed class ItemWidthBlock : IDisposable
    {
        public ItemWidthBlock(float width) => ImGui.PushItemWidth(width);
        public void Dispose() => ImGui.PopItemWidth();
    }

    public sealed class TextWrapPosBlock : IDisposable
    {
        public TextWrapPosBlock() => ImGui.PushTextWrapPos();
        public TextWrapPosBlock(float posX) => ImGui.PushTextWrapPos(posX);
        public void Dispose() => ImGui.PopTextWrapPos();
    }
}