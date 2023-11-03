using Dalamud.Interface.Utility;
using DelvCD.Helpers;
using DelvCD.UIElements;
using ImGuiNET;
using Newtonsoft.Json;
using System.Numerics;

namespace DelvCD.Config
{
    public class GroupConfig : IConfigPage
    {
        [JsonIgnore] private float _scale => ImGuiHelpers.GlobalScale;
        [JsonIgnore] private static Vector2 _screenSize = ImGui.GetMainViewport().Size;

        public string Name => "Group";

        public Vector2 Position = new Vector2(0, 0);
        public Vector2 DynamicBuffer = new Vector2(0, 0);

        [JsonIgnore] private Vector2 _iconSize = new Vector2(40, 40);
        [JsonIgnore] private float _mX = 1f;
        [JsonIgnore] private float _mY = 1f;
        [JsonIgnore] private bool _recusiveResize = false;
        [JsonIgnore] private bool _conditionsResize = false;
        [JsonIgnore] private bool _positionOnly = false;
        [JsonIgnore] public int _elementCount = 0;
        [JsonIgnore] public bool _dynamicConditions = false;
        public bool _keepsorted = false;
        public bool IsDynamic = false;
        public bool IsParentDynamic = false;

        public IConfigPage GetDefault() => new GroupConfig();

        public void DrawConfig(IConfigurable parent, Vector2 size, float padX, float padY)
        {
            if (ImGui.BeginChild("##GroupConfig", new Vector2(size.X, size.Y), true))
            {
                if (parent is not DelvCDConfig)
                {
                    ImGui.Checkbox("Allow Dynamic Group Repositioning from Parent Group", ref IsDynamic);
                    if (ImGui.IsItemHovered())
                    {
                        ImGui.SetTooltip("Affects this group as a whole");
                    }
                    if (IsParentDynamic && !IsDynamic) { ImGui.TextDisabled("Dynamic repositioning is enabled in the Parent Group's settings, but this element will not be repositioned."); }
                    if (!IsParentDynamic && IsDynamic) { ImGui.TextDisabled("Dynamic repositioning is disallowed in the Parent Group's settings."); }
                }
                if (IsParentDynamic && IsDynamic)
                {
                    ImGui.TextDisabled("Postion is currently controlled by the Parent Folder's group settings.");
                } else {
                    ImGui.DragFloat2("Group Position", ref Position);
                }

                ImGui.NewLine();
                ImGui.DragFloat2("Reposition Buffer##Positioning", ref DynamicBuffer, 1, -_screenSize.Y, _screenSize.Y);
                ImGui.Checkbox("Reposition Contents", ref _keepsorted);
                if (ImGui.IsItemHovered())
                {
                    ImGui.SetTooltip("Check to dynamically position visible elements in this Group. " + "If unchecked, elements will stay in their last known position");
                }
                ImGui.SameLine();
                ImGui.Checkbox("Conditions##Reposition", ref _dynamicConditions);
                if (ImGui.IsItemHovered())
                {
                    ImGui.SetTooltip("Check to reposition conditions");
                }
                ImGui.SameLine();

                float padWidth2 = ImGui.CalcItemWidth() - ImGui.GetCursorPosX() - 60 + padX;
                ImGui.SetCursorPosX(ImGui.GetCursorPosX() + padWidth2);
                if (ImGui.Button("Convert", new Vector2(60 * _scale, 0)))
                {
                    if (parent is Group g)
                    {
                        g.MakeDynamic(_dynamicConditions);
                    }
                }
                if (ImGui.IsItemHovered())
                {
                    ImGui.SetTooltip("Makes all elements in group dynamic, also checks conditions. " + "Elements can opt out from the element editor");
                }
                if (_keepsorted)
                {
                    if (parent is Group g)
                    {
                        g.RepositionElements(DynamicBuffer, _dynamicConditions, _elementCount);
                        // Notify elements that the group's status is dynamic
                        g.MarkParentDynamic(_dynamicConditions, _keepsorted);
                    }
                } else
                {
                    if (parent is Group g)
                    {
                        // Notify elements that the group's status is NOT dynamic
                        g.MarkParentDynamic(_dynamicConditions, _keepsorted);
                    }

                }

                ImGui.NewLine();
                ImGui.Text("Resize Icons");
                ImGui.DragFloat2("Icon Size##Size", ref _iconSize, 1, 0, _screenSize.Y);
                ImGui.Checkbox("Recursive##Size", ref _recusiveResize);
                if (ImGui.IsItemHovered())
                {
                    ImGui.SetTooltip("Check to recursively resize icons in sub-groups");
                }

                ImGui.SameLine();
                ImGui.Checkbox("Conditions##Size", ref _conditionsResize);
                if (ImGui.IsItemHovered())
                {
                    ImGui.SetTooltip("Check to resize conditions");
                }

                ImGui.SameLine();
                float padWidth = ImGui.CalcItemWidth() - ImGui.GetCursorPosX() - 60 + padX;
                ImGui.SetCursorPosX(ImGui.GetCursorPosX() + padWidth);
                if (ImGui.Button("Resize", new Vector2(60 * _scale, 0)))
                {
                    if (parent is Group g)
                    {
                        g.ResizeIcons(_iconSize, _recusiveResize, _conditionsResize);
                    }
                }

                if (parent is Group group)
                {
                    ImGui.NewLine();
                    ImGui.Text("Scale Resolution (BACK UP YOUR CONFIG FIRST!)");
                    ImGui.DragFloat("X Multiplier", ref _mX, 0.01f, 0.01f, 100f);
                    ImGui.DragFloat("Y Multiplier", ref _mY, 0.01f, 0.01f, 100f);
                    ImGui.Checkbox("Scale positions only", ref _positionOnly);
                    ImGui.SameLine();
                    padWidth = ImGui.CalcItemWidth() - ImGui.GetCursorPosX() - 60 * _scale + padX;
                    ImGui.SetCursorPosX(ImGui.GetCursorPosX() + padWidth);
                    if (ImGui.Button("Scale", new Vector2(60 * _scale, 0)))
                    {
                        group.ScaleResolution(new(_mX, _mY), _positionOnly);
                    }
                }


                ImGui.EndChild();
            }
        }
    }
}
