using System;
using System.Collections.Generic;
using System.Numerics;

namespace ImGuiNET;

public static partial class ImGuiEx
{
    public record GroupBoxOptions
    {
        public bool Collapsible { get; init; } = false;
        public uint HeaderTextColor { get; init; } = ImGui.GetColorU32(ImGuiCol.Text);
        public Action HeaderTextAction { get; init; } = null;
        public uint BorderColor { get; init; } = ImGui.GetColorU32(ImGuiCol.Border);
        public Vector2 BorderPadding { get; init; } = ImGui.GetStyle().WindowPadding;
        public float BorderRounding { get; init; } = ImGui.GetStyle().FrameRounding;
        public ImDrawFlags DrawFlags { get; init; } = ImDrawFlags.None;
        public float BorderThickness { get; init; } = 2f;
    }

    private static readonly Stack<GroupBoxOptions> groupBoxOptionsStack = new();
    public static bool BeginGroupBox(string id = null, float minimumWindowPercent = 1.0f, GroupBoxOptions options = null)
    {
        options ??= new GroupBoxOptions();
        groupBoxOptionsStack.Push(options);
        ImGui.BeginGroup();

        var open = true;
        if (!string.IsNullOrEmpty(id))
        {
            if (!options.Collapsible)
            {
                ImGui.TextColored(ImGui.ColorConvertU32ToFloat4(options.HeaderTextColor), id);
            }
            else
            {
                ImGui.PushStyleColor(ImGuiCol.Text, options.HeaderTextColor);
                open = ImGui.TreeNodeEx(id, ImGuiTreeNodeFlags.NoTreePushOnOpen | ImGuiTreeNodeFlags.DefaultOpen);
                ImGui.PopStyleColor();
            }

            options.HeaderTextAction?.Invoke();

            // This prevents rounding issues caused by ImGui flooring the cursor position after items
            ImGui.Indent();
            ImGui.Unindent();
        }

        ImGui.BeginGroup();
        var style = ImGui.GetStyle();
        var spacing = style.ItemSpacing.X * (1 - minimumWindowPercent);
        var width = Math.Max((ImGui.GetWindowContentRegionMax().X - style.WindowPadding.X) * minimumWindowPercent - spacing, 1);
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, Vector2.Zero);
        ImGui.Dummy(options.BorderPadding with { X = width });
        ImGui.PopStyleVar();
        ImGui.Indent(Math.Max(options.BorderPadding.X, 0.01f));
        ImGui.PushItemWidth((width - options.BorderPadding.X) / 2);
        if (open) return true;

        ImGui.TextDisabled(". . .");
        EndGroupBox();
        return false;
    }

    public static bool BeginGroupBox(string text, GroupBoxOptions options) => BeginGroupBox(text, 1.0f, options);

    public static bool BeginGroupBox(uint borderColor, float minimumWindowPercent = 1.0f) => BeginGroupBox(null, minimumWindowPercent, new GroupBoxOptions { BorderColor = borderColor });

    public static void EndGroupBox()
    {
        var options = groupBoxOptionsStack.Pop();
        ImGui.PopItemWidth();
        ImGui.Unindent(Math.Max(options.BorderPadding.X, 0.01f));
        ImGui.SetCursorPosY(ImGui.GetCursorPosY() - ImGui.GetStyle().ItemSpacing.Y);
        ImGui.Dummy(options.BorderPadding with { X = 0 });
        ImGui.EndGroup();

        var min = ImGui.GetItemRectMin();
        var max = ImGui.GetItemRectMax();

        // Rect with text corner missing
        /*ImGui.PushClipRect(min with { Y = min.Y + options.BorderRounding }, max, true);
        ImGui.GetWindowDrawList().AddRect(min, max, options.BorderColor, options.BorderRounding, options.DrawFlags, options.BorderThickness);
        ImGui.PopClipRect();
        ImGui.PushClipRect(min with { X = (min.X + max.X) / 2 }, max with { Y = min.Y + options.BorderRounding }, true);
        ImGui.GetWindowDrawList().AddRect(min, max, options.BorderColor, options.BorderRounding, options.DrawFlags, options.BorderThickness);
        ImGui.PopClipRect();*/

        // [ ] Brackets
        /*ImGui.PushClipRect(min, max with { X = (min.X * 2 + max.X) / 3 }, true);
        ImGui.GetWindowDrawList().AddRect(min, max, options.BorderColor, options.BorderRounding, options.DrawFlags, options.BorderThickness);
        ImGui.PopClipRect();
        ImGui.PushClipRect(min with { X = (min.X + max.X * 2) / 3 }, max, true);
        ImGui.GetWindowDrawList().AddRect(min, max, options.BorderColor, options.BorderRounding, options.DrawFlags, options.BorderThickness);
        ImGui.PopClipRect();*/

        // Horizontal brackets
        /*ImGui.PushClipRect(min, max with { Y = (min.Y * 2 + max.Y) / 3 }, true);
        ImGui.GetWindowDrawList().AddRect(min, max, options.BorderColor, options.BorderRounding, options.DrawFlags, options.BorderThickness);
        ImGui.PopClipRect();
        ImGui.PushClipRect(min with { Y = (min.Y + max.Y * 2) / 3 }, max, true);
        ImGui.GetWindowDrawList().AddRect(min, max, options.BorderColor, options.BorderRounding, options.DrawFlags, options.BorderThickness);
        ImGui.PopClipRect();*/

        ImGui.GetWindowDrawList().AddRect(min, max, options.BorderColor, options.BorderRounding, options.DrawFlags, options.BorderThickness);

        ImGui.EndGroup();
    }
}