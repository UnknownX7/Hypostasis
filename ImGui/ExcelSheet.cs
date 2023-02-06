using System;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Interface;
using Lumina.Excel;

namespace ImGuiNET;

public static partial class ImGuiEx
{
    public class ExcelSheetOptions<T> where T : ExcelRow
    {
        public Func<T, string> FormatRow { get; init; } = row => row.ToString();
        public Func<T, string, bool> SearchPredicate { get; init; } = null;
        public Func<T, bool, bool> DrawSelectable { get; init; } = null;
        public IEnumerable<T> FilteredSheet { get; init; } = null;
        public Vector2? Size { get; init; } = null;
    }

    public class ExcelSheetComboOptions<T> : ExcelSheetOptions<T> where T : ExcelRow
    {
        public Func<T, string> GetPreview { get; init; } = null;
        public ImGuiComboFlags ComboFlags { get; init; } = ImGuiComboFlags.None;
    }

    public class ExcelSheetPopupOptions<T> : ExcelSheetOptions<T> where T : ExcelRow
    {
        public ImGuiWindowFlags WindowFlags { get; init; } = ImGuiWindowFlags.None;
        public bool CloseOnSelection { get; init; } = false;
    }

    private static string search = string.Empty;
    private static HashSet<ExcelRow> filtered;

    public static bool ExcelSheetCombo<T>(string id, ref uint selectedRow, ExcelSheetComboOptions<T> options = null) where T : ExcelRow
    {
        options ??= new ExcelSheetComboOptions<T>();
        var sheet = DalamudApi.DataManager.GetExcelSheet<T>();
        if (sheet == null) return false;

        var getPreview = options.GetPreview ?? options.FormatRow;
        if (!ImGui.BeginCombo(id, sheet.GetRow(selectedRow) is { } r ? getPreview(r) : selectedRow.ToString(), options.ComboFlags | ImGuiComboFlags.HeightLargest)) return false;

        if (ImGui.IsWindowAppearing() && ImGui.IsWindowFocused() && !ImGui.IsAnyItemActive())
        {
            search = string.Empty;
            filtered = null;
            ImGui.SetKeyboardFocusHere(0);
        }

        if (ImGui.InputTextWithHint("##ExcelSheetSearch", "Search", ref search, 128))
            filtered = null;

        ImGui.BeginChild("ExcelSheetSearchList", options.Size ?? new Vector2(0, 200 * ImGuiHelpers.GlobalScale), true);

        var filteredSheet = options.FilteredSheet ?? sheet;
        var searchPredicate = options.SearchPredicate ?? ((row, s) => options.FormatRow(row).Contains(s, StringComparison.CurrentCultureIgnoreCase));
        filtered ??= filteredSheet.Where(s => searchPredicate(s, search)).Select(s => (ExcelRow)s).ToHashSet();

        var i = 0;
        var ret = false;
        var drawSelectable = options.DrawSelectable ?? ((row, selected) => ImGui.Selectable(options.FormatRow(row), selected));
        foreach (var row in filtered.Cast<T>())
        {
            ImGui.PushID(i++);
            if (drawSelectable(row, selectedRow == row.RowId))
            {
                selectedRow = row.RowId;
                ret = true;
                break;
            }
            ImGui.PopID();
        }

        // ImGui issue #273849, children keep combos from closing automatically
        if (ret)
            ImGui.CloseCurrentPopup();

        ImGui.EndChild();
        ImGui.EndCombo();
        return ret;
    }

    public static bool ExcelSheetPopup<T>(string id, out uint selectedRow, ExcelSheetPopupOptions<T> options = null) where T : ExcelRow
    {
        options ??= new ExcelSheetPopupOptions<T>();
        var sheet = options.FilteredSheet ?? DalamudApi.DataManager.GetExcelSheet<T>();
        selectedRow = 0;
        if (sheet == null) return false;

        ImGui.SetNextWindowSize(options.Size ?? new Vector2(0, 250 * ImGuiHelpers.GlobalScale));
        if (!ImGui.BeginPopup(id, options.WindowFlags)) return false;

        if (ImGui.IsWindowAppearing() && ImGui.IsWindowFocused() && !ImGui.IsAnyItemActive())
        {
            search = string.Empty;
            filtered = null;
            ImGui.SetKeyboardFocusHere(0);
        }

        if (ImGui.InputTextWithHint("##ExcelSheetSearch", "Search", ref search, 128))
            filtered = null;

        ImGui.BeginChild("ExcelSheetSearchList", Vector2.Zero, true);

        var searchPredicate = options.SearchPredicate ?? ((row, s) => options.FormatRow(row).Contains(s, StringComparison.CurrentCultureIgnoreCase));
        filtered ??= sheet.Where(s => searchPredicate(s, search)).Select(s => (ExcelRow)s).ToHashSet();

        var i = 0;
        var ret = false;
        var selectableFlags = options.CloseOnSelection ? ImGuiSelectableFlags.None : ImGuiSelectableFlags.DontClosePopups;
        var drawSelectable = options.DrawSelectable ?? ((row, _) => ImGui.Selectable(options.FormatRow(row), false, selectableFlags));
        foreach (var row in filtered.Cast<T>())
        {
            ImGui.PushID(i++);
            if (drawSelectable(row, false))
            {
                selectedRow = row.RowId;
                ret = true;
            }
            ImGui.PopID();
        }

        ImGui.EndChild();
        ImGui.EndPopup();
        return ret;
    }
}