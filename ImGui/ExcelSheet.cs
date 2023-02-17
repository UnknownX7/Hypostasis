using System;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Interface;
using Lumina.Excel;
using System.Reflection;

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

    private static string sheetSearchText = string.Empty;
    private static HashSet<ExcelRow> filteredSearchSheet;
    private static string prevSearchID = string.Empty;

    private static void ExcelSheetSearchInput<T>(string id, IEnumerable<T> filteredSheet, Func<T, string, bool> searchPredicate) where T : ExcelRow
    {
        if (ImGui.IsWindowAppearing() && ImGui.IsWindowFocused() && !ImGui.IsAnyItemActive())
        {
            if (id != prevSearchID)
            {
                sheetSearchText = string.Empty;
                filteredSearchSheet = null;
                prevSearchID = id;
            }

            ImGui.SetKeyboardFocusHere(0);
        }

        if (ImGui.InputTextWithHint("##ExcelSheetSearch", "Search", ref sheetSearchText, 128, ImGuiInputTextFlags.AutoSelectAll))
            filteredSearchSheet = null;

        filteredSearchSheet ??= filteredSheet.Where(s => searchPredicate(s, sheetSearchText)).Select(s => (ExcelRow)s).ToHashSet();
    }

    public static bool ExcelSheetCombo<T>(string id, ref uint selectedRow, ExcelSheetComboOptions<T> options = null) where T : ExcelRow
    {
        options ??= new ExcelSheetComboOptions<T>();
        var sheet = DalamudApi.DataManager.GetExcelSheet<T>();
        if (sheet == null) return false;

        var getPreview = options.GetPreview ?? options.FormatRow;
        if (!ImGui.BeginCombo(id, sheet.GetRow(selectedRow) is { } r ? getPreview(r) : selectedRow.ToString(), options.ComboFlags | ImGuiComboFlags.HeightLargest)) return false;

        ExcelSheetSearchInput(id, options.FilteredSheet ?? sheet, options.SearchPredicate ?? ((row, s) => options.FormatRow(row).Contains(s, StringComparison.CurrentCultureIgnoreCase)));

        ImGui.BeginChild("ExcelSheetSearchList", options.Size ?? new Vector2(0, 200 * ImGuiHelpers.GlobalScale), true);

        var i = 0;
        var ret = false;
        var drawSelectable = options.DrawSelectable ?? ((row, selected) => ImGui.Selectable(options.FormatRow(row), selected));
        foreach (var row in filteredSearchSheet.Cast<T>())
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

        ExcelSheetSearchInput(id, sheet, options.SearchPredicate ?? ((row, s) => options.FormatRow(row).Contains(s, StringComparison.CurrentCultureIgnoreCase)));

        ImGui.BeginChild("ExcelSheetSearchList", Vector2.Zero, true);

        var i = 0;
        var ret = false;
        var selectableFlags = options.CloseOnSelection ? ImGuiSelectableFlags.None : ImGuiSelectableFlags.DontClosePopups;
        var drawSelectable = options.DrawSelectable ?? ((row, _) => ImGui.Selectable(options.FormatRow(row), false, selectableFlags));
        foreach (var row in filteredSearchSheet.Cast<T>())
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

    public static void ExcelSheetTable<T>(string id) where T : ExcelRow
    {
        var properties = typeof(T).GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public);
        var columns = properties.Length;
        if (columns == 0 || !ImGui.BeginTable(id, columns + 1, ImGuiTableFlags.Borders | ImGuiTableFlags.ScrollX | ImGuiTableFlags.ScrollY)) return;

        ImGui.TableSetupScrollFreeze(1, 1);
        ImGui.TableSetupColumn("Row");
        foreach (var propertyInfo in properties)
            ImGui.TableSetupColumn(propertyInfo.Name);
        ImGui.TableHeadersRow();

        var sheet = DalamudApi.DataManager.GetExcelSheet<T>();
        if (sheet != null)
        {
            using var clipper = new ListClipper((int)sheet.RowCount);
            foreach (var i in clipper.Rows)
            {
                var row = sheet.GetRow((uint)i);
                if (row == null) continue;

                ImGui.TableNextRow();
                ImGui.TableNextColumn();

                ImGui.TextUnformatted(row.RowId.ToString());
                ImGui.TableNextColumn();

                for (int j = 0; j < properties.Length; j++)
                {
                    var propertyInfo = properties[j];
                    var value = propertyInfo.GetValue(row);
                    var valueStr = value switch
                    {
                        ILazyRow lazyRow => lazyRow.Row.ToString(),
                        _ => value?.ToString() ?? string.Empty
                    };

                    ImGui.TextUnformatted(valueStr);
                    if (j != properties.Length - 1)
                        ImGui.TableNextColumn();
                }
            }
        }

        ImGui.EndTable();
    }
}