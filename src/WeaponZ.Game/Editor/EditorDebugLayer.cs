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

    private const Key ToggleVisibility = Key.V;

    private bool _isVisible = true;

    public EditorDebugLayer(ImGuiRenderer imguiRenderer, IInputContext inputContext, SceneGraph sceneGraph)
    {
        _imguiRenderer = imguiRenderer;

        _inputContext = inputContext;
        _inputContext.KeyPressed += OnKeyPressed;

        _sceneGraph = sceneGraph;
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
            ImGui.Selectable("Light");

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
                ImGui.DragFloat3("", ref t.Position);
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
}
