using System.Numerics;

using ImGuiNET;

using Veldrid;

using WeaponZ.Game.Input;
using WeaponZ.Game.Render;
using WeaponZ.Game.Scene;

namespace WeaponZ.Game.Editor;

public class EditorDebugLayer
{
    private readonly ImGuiRenderer _imguiRenderer;

    private readonly IInputContext _inputContext;
    private readonly SceneGraph _sceneGraph;

    private ISceneObject? _selectedObject = null;
    private int _transformOptionSelection = 0;

    private const Key ToggleVisibility = Key.H;

    private bool _isVisible = true;

    public EditorDebugLayer(ImGuiRenderer imguiRenderer, IInputContext inputContext, SceneGraph sceneGraph)
    {
        _imguiRenderer = imguiRenderer;

        _inputContext = inputContext;
        _inputContext.KeyPressed += OnKeyPressed;

        _sceneGraph = sceneGraph;

        SetupImGuiStyles();
    }

    private void OnKeyPressed(object? sender, KeyboardEventArgs e)
    {
        if (!e.IsRepeatingEvent)
        {
            if (e.Key is ToggleVisibility)
            {
                _isVisible = !_isVisible;
            }
        }
    }

    public void Update(TimeSpan deltaTime, InputSnapshot inputSnapshot)
    {
        _imguiRenderer.Update((float)deltaTime.TotalSeconds, inputSnapshot);
    }

    public void Draw(GraphicsDevice graphicsDevice, Renderer renderer)
    {
        if (!_isVisible) return;

        SetupSceneGraphUi(_sceneGraph);

        if (_selectedObject is not null)
        {
            SetupTransformUi(_selectedObject.Transform);
        }

        _imguiRenderer.Render(graphicsDevice, renderer._commandList);
    }

    /// <summary>
    /// Draws the scene graph ui
    /// </summary>
    private void SetupSceneGraphUi(SceneGraph sceneGraph)
    {
        ImGui.Begin("Scene Graph");

        DrawSceneGraphUiNode(_sceneGraph.Root);

        if (ImGui.Button("Create New..."))
        {
            ImGui.OpenPopup("Create new");
        }

        if (ImGui.BeginPopupContextItem("Create new"))
        {
            ImGui.Selectable("Pawn");
            ImGui.Selectable("Camera");

            if (ImGui.Selectable("Light"))
            {
                ISceneObject parent = _selectedObject is null ? _sceneGraph.Root : _selectedObject;
                _sceneGraph.CreateLight(parent);
            }

            ImGui.EndPopup();
        }

        ImGui.End();
    }

    /// <summary>
    /// Draws the scene graph ui nodes
    /// </summary>
    private void DrawSceneGraphUiNode(ISceneObject sceneObject)
    {
        if (sceneObject is null) return;

        var treeNodeFlags = ImGuiTreeNodeFlags.OpenOnArrow;
        if (_selectedObject == sceneObject)
        {
            treeNodeFlags |= ImGuiTreeNodeFlags.Selected;
        }

        if (sceneObject.Children.Count < 1)
        {
            treeNodeFlags |= ImGuiTreeNodeFlags.Leaf;
        }

        bool nodeOpen = ImGui.TreeNodeEx(sceneObject.DisplayName, treeNodeFlags);

        if (ImGui.IsItemClicked())
        {
            _selectedObject = sceneObject;
        }

        if (nodeOpen)
        {
            foreach (var child in sceneObject.Children)
            {
                DrawSceneGraphUiNode(child);
            }
            ImGui.TreePop();
        }
    }

    /// <summary>
    /// Draws the transform UI
    /// </summary>
    private void SetupTransformUi(Transform transform)
    {
        if (_selectedObject is null) return;

        ImGui.Begin("Scene Object");

        Transform t = _transformOptionSelection == 0 ? _selectedObject.GlobalTransform : _selectedObject.Transform;

        ImGui.Text($"{_selectedObject.DisplayName} [{_selectedObject.Kind}]");

        ImGui.Separator();

        if (ImGui.TreeNodeEx("Transform"))
        {
            if (ImGui.TreeNodeEx("Position", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.DragFloat3("", ref t.Position, 0.02f);
                ImGui.TreePop();
            }

            if (ImGui.TreeNodeEx("Rotation", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.DragFloat3("", ref t.Rotation, 0.02f);
                ImGui.TreePop();
            }

            if (ImGui.TreeNodeEx("Scale", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.DragFloat3("", ref t.Scale, 0.0002f);
                ImGui.TreePop();
            }

            ImGui.RadioButton("Global", ref _transformOptionSelection, 0);
            ImGui.SameLine();
            ImGui.RadioButton("Local", ref _transformOptionSelection, 1);

            ImGui.TreePop();
        }

        if (_selectedObject.Kind is SceneObjectKind.Light && _selectedObject is LightSceneObject lightSceneObject)
        {
            DrawLightProperties(lightSceneObject);
        }

        ImGui.End();
    }

    private static void DrawLightProperties(LightSceneObject lightSceneObject)
    {
        if (ImGui.TreeNode("Light"))
        {
            Vector3 color = new(lightSceneObject.Light.Color.X, lightSceneObject.Light.Color.Y, lightSceneObject.Light.Color.Z);
            ImGui.ColorEdit3("Color", ref color);

            lightSceneObject.Light = new PointLight(
                new Vector3(lightSceneObject.Light.Position.X, lightSceneObject.Light.Position.Y, lightSceneObject.Light.Position.Z),
                new Vector3(color.X, color.Y, color.Z)
            );

            ImGui.TreePop();
        }
    }

    private static void SetupImGuiStyles()
    {
        ImGuiStylePtr style = ImGui.GetStyle();
        var colors = style.Colors;
        colors[(int)ImGuiCol.Text] = new Vector4(1.00f, 1.00f, 1.00f, 1.00f);
        colors[(int)ImGuiCol.TextDisabled] = new Vector4(0.50f, 0.50f, 0.50f, 1.00f);
        colors[(int)ImGuiCol.WindowBg] = new Vector4(0.10f, 0.10f, 0.10f, 1.00f);
        colors[(int)ImGuiCol.ChildBg] = new Vector4(0.00f, 0.00f, 0.00f, 0.00f);
        colors[(int)ImGuiCol.PopupBg] = new Vector4(0.19f, 0.19f, 0.19f, 0.92f);
        colors[(int)ImGuiCol.Border] = new Vector4(0.19f, 0.19f, 0.19f, 0.29f);
        colors[(int)ImGuiCol.BorderShadow] = new Vector4(0.00f, 0.00f, 0.00f, 0.24f);
        colors[(int)ImGuiCol.FrameBg] = new Vector4(0.05f, 0.05f, 0.05f, 0.54f);
        colors[(int)ImGuiCol.FrameBgHovered] = new Vector4(0.19f, 0.19f, 0.19f, 0.54f);
        colors[(int)ImGuiCol.FrameBgActive] = new Vector4(0.20f, 0.22f, 0.23f, 1.00f);
        colors[(int)ImGuiCol.TitleBg] = new Vector4(0.00f, 0.00f, 0.00f, 1.00f);
        colors[(int)ImGuiCol.TitleBgActive] = new Vector4(0.06f, 0.06f, 0.06f, 1.00f);
        colors[(int)ImGuiCol.TitleBgCollapsed] = new Vector4(0.00f, 0.00f, 0.00f, 1.00f);
        colors[(int)ImGuiCol.MenuBarBg] = new Vector4(0.14f, 0.14f, 0.14f, 1.00f);
        colors[(int)ImGuiCol.ScrollbarBg] = new Vector4(0.05f, 0.05f, 0.05f, 0.54f);
        colors[(int)ImGuiCol.ScrollbarGrab] = new Vector4(0.34f, 0.34f, 0.34f, 0.54f);
        colors[(int)ImGuiCol.ScrollbarGrabHovered] = new Vector4(0.40f, 0.40f, 0.40f, 0.54f);
        colors[(int)ImGuiCol.ScrollbarGrabActive] = new Vector4(0.56f, 0.56f, 0.56f, 0.54f);
        colors[(int)ImGuiCol.CheckMark] = new Vector4(0.33f, 0.67f, 0.86f, 1.00f);
        colors[(int)ImGuiCol.SliderGrab] = new Vector4(0.34f, 0.34f, 0.34f, 0.54f);
        colors[(int)ImGuiCol.SliderGrabActive] = new Vector4(0.56f, 0.56f, 0.56f, 0.54f);
        colors[(int)ImGuiCol.Button] = new Vector4(0.05f, 0.05f, 0.05f, 0.54f);
        colors[(int)ImGuiCol.ButtonHovered] = new Vector4(0.19f, 0.19f, 0.19f, 0.54f);
        colors[(int)ImGuiCol.ButtonActive] = new Vector4(0.20f, 0.22f, 0.23f, 1.00f);
        colors[(int)ImGuiCol.Header] = new Vector4(0.00f, 0.00f, 0.00f, 0.52f);
        colors[(int)ImGuiCol.HeaderHovered] = new Vector4(0.00f, 0.00f, 0.00f, 0.36f);
        colors[(int)ImGuiCol.HeaderActive] = new Vector4(0.20f, 0.22f, 0.23f, 0.33f);
        colors[(int)ImGuiCol.Separator] = new Vector4(0.28f, 0.28f, 0.28f, 0.29f);
        colors[(int)ImGuiCol.SeparatorHovered] = new Vector4(0.44f, 0.44f, 0.44f, 0.29f);
        colors[(int)ImGuiCol.SeparatorActive] = new Vector4(0.40f, 0.44f, 0.47f, 1.00f);
        colors[(int)ImGuiCol.ResizeGrip] = new Vector4(0.28f, 0.28f, 0.28f, 0.29f);
        colors[(int)ImGuiCol.ResizeGripHovered] = new Vector4(0.44f, 0.44f, 0.44f, 0.29f);
        colors[(int)ImGuiCol.ResizeGripActive] = new Vector4(0.40f, 0.44f, 0.47f, 1.00f);
        colors[(int)ImGuiCol.Tab] = new Vector4(0.00f, 0.00f, 0.00f, 0.52f);
        colors[(int)ImGuiCol.TabHovered] = new Vector4(0.14f, 0.14f, 0.14f, 1.00f);
        colors[(int)ImGuiCol.TabActive] = new Vector4(0.20f, 0.20f, 0.20f, 0.36f);
        colors[(int)ImGuiCol.TabUnfocused] = new Vector4(0.00f, 0.00f, 0.00f, 0.52f);
        colors[(int)ImGuiCol.TabUnfocusedActive] = new Vector4(0.14f, 0.14f, 0.14f, 1.00f);
        colors[(int)ImGuiCol.DockingPreview] = new Vector4(0.33f, 0.67f, 0.86f, 1.00f);
        colors[(int)ImGuiCol.DockingEmptyBg] = new Vector4(1.00f, 0.00f, 0.00f, 1.00f);
        colors[(int)ImGuiCol.PlotLines] = new Vector4(1.00f, 0.00f, 0.00f, 1.00f);
        colors[(int)ImGuiCol.PlotLinesHovered] = new Vector4(1.00f, 0.00f, 0.00f, 1.00f);
        colors[(int)ImGuiCol.PlotHistogram] = new Vector4(1.00f, 0.00f, 0.00f, 1.00f);
        colors[(int)ImGuiCol.PlotHistogramHovered] = new Vector4(1.00f, 0.00f, 0.00f, 1.00f);
        colors[(int)ImGuiCol.TableHeaderBg] = new Vector4(0.00f, 0.00f, 0.00f, 0.52f);
        colors[(int)ImGuiCol.TableBorderStrong] = new Vector4(0.00f, 0.00f, 0.00f, 0.52f);
        colors[(int)ImGuiCol.TableBorderLight] = new Vector4(0.28f, 0.28f, 0.28f, 0.29f);
        colors[(int)ImGuiCol.TableRowBg] = new Vector4(0.00f, 0.00f, 0.00f, 0.00f);
        colors[(int)ImGuiCol.TableRowBgAlt] = new Vector4(1.00f, 1.00f, 1.00f, 0.06f);
        colors[(int)ImGuiCol.TextSelectedBg] = new Vector4(0.20f, 0.22f, 0.23f, 1.00f);
        colors[(int)ImGuiCol.DragDropTarget] = new Vector4(0.33f, 0.67f, 0.86f, 1.00f);
        colors[(int)ImGuiCol.NavHighlight] = new Vector4(1.00f, 0.00f, 0.00f, 1.00f);
        colors[(int)ImGuiCol.NavWindowingHighlight] = new Vector4(1.00f, 0.00f, 0.00f, 0.70f);
        colors[(int)ImGuiCol.NavWindowingDimBg] = new Vector4(1.00f, 0.00f, 0.00f, 0.20f);
        colors[(int)ImGuiCol.ModalWindowDimBg] = new Vector4(1.00f, 0.00f, 0.00f, 0.35f);

        style.WindowPadding = new Vector2(8.00f, 8.00f);
        style.FramePadding = new Vector2(5.00f, 2.00f);
        style.CellPadding = new Vector2(6.00f, 6.00f);
        style.ItemSpacing = new Vector2(6.00f, 6.00f);
        style.ItemInnerSpacing = new Vector2(6.00f, 6.00f);
        style.TouchExtraPadding = new Vector2(0.00f, 0.00f);
        style.IndentSpacing = 25;
        style.ScrollbarSize = 15;
        style.GrabMinSize = 10;
        style.WindowBorderSize = 1;
        style.ChildBorderSize = 1;
        style.PopupBorderSize = 1;
        style.FrameBorderSize = 1;
        style.TabBorderSize = 1;
        style.WindowRounding = 7;
        style.ChildRounding = 4;
        style.FrameRounding = 3;
        style.PopupRounding = 4;
        style.ScrollbarRounding = 9;
        style.GrabRounding = 3;
        style.LogSliderDeadzone = 4;
        style.TabRounding = 4;
    }
}
