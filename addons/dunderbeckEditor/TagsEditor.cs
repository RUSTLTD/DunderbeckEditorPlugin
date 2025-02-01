using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Godot;

#if TOOLS
namespace Dunderbeck.addons.DunderbeckEditor;

[Tool]
public partial class TagsEditor : EditorProperty
{
    private LineEdit _searchInput;
    private ItemList _searchResults;
    private VBoxContainer _existingItems;

    public TagsEditor()
    {
        Control control = ResourceLoader.Load<PackedScene>("res://addons/DunderbeckEditor/TagsEditor.tscn")
            .Instantiate<Control>();
        AddChild(control);

        _searchInput = control.GetNode<LineEdit>("SearchInput");
        _searchResults = control.GetNode<ItemList>("ScrollContainer/SearchResults");
        _existingItems = control.GetNode<VBoxContainer>("ExistingItems");

        _searchInput.Connect(LineEdit.SignalName.TextChanged, new Callable(this, MethodName.DoSearch));
        _searchResults.Connect(ItemList.SignalName.ItemSelected, new Callable(this, MethodName.AddTag));
    }

    public override void _UpdateProperty()
    {
        Redraw();
    }

    private void Redraw()
    {
        // Check what tags are assigned to the item currently
        var tags = GetEditedObject().Get(GetEditedProperty()).AsStringArray();

        var itemControl = _existingItems.GetChild<Control>(0);

        if (tags.Length > 0)
        {
            int existingChildren = _existingItems.GetChildCount();
            int iterations = Mathf.Max(tags.Length, existingChildren);
            for (int i = 0; i < iterations; i++)
            {
                if (i >= tags.Length)
                {
                    _existingItems.GetChild(i).QueueFree();
                    continue;
                }

                Control item;
                if (i >= existingChildren)
                {
                    item = (Control)itemControl.Duplicate();
                    _existingItems.AddChild(item);
                }
                else item = _existingItems.GetChild<Control>(i);

                Button removeButton = item.GetNode<Button>("RemoveButton");
                removeButton.Visible = true;
                Label itemLabel = item.GetNode<Label>("Label");

                itemLabel.Text = tags[i];

                ClearSignals(removeButton, BaseButton.SignalName.Pressed);

                removeButton.Connect(BaseButton.SignalName.Pressed, new Callable(this, MethodName.RemoveTag));
            }
        }
        else
        {
            Button removeButton = itemControl.GetNode<Button>("RemoveButton");
            Label itemLabel = itemControl.GetNode<Label>("Label");
            removeButton.Visible = false;
            itemLabel.Text = "None";
        }

        DoSearch("");
    }

    private void DoSearch(string text)
    {
        while (_searchResults.ItemCount > 0) _searchResults.RemoveItem(0);

        var tags = GetEditedObject().Get(GetEditedProperty()).AsStringArray();

        TagsAttribute attribute = GetAttribute(GetEditedObject(), GetEditedProperty());
        foreach (var tag in Enum.GetNames(attribute.TagsEnumType))
        {
            if (!tag.ToLower().Contains(text.ToLower())) continue;
            if (tags.Contains(tag)) continue;
            _searchResults.AddItem(tag);
        }
    }

    private void AddTag(long selectionIndex)
    {
        string tagToAdd = _searchResults.GetItemText((int)selectionIndex);

        var tags = GetEditedObject().Get(GetEditedProperty()).AsStringArray();
        var withNewTag = tags.Concat(new[] { tagToAdd }).ToArray();
        var newValue = Variant.From(withNewTag);

        EmitChanged(GetEditedProperty(), newValue);
    }

    private void RemoveTag()
    {
        Control button = GetViewport().GuiGetFocusOwner();
        string tagToRemove = button.GetNode<Label>("../Label").Text;

        var tags = GetEditedObject().Get(GetEditedProperty()).AsStringArray();
        var withoutOldTag = tags.Where(t => t != tagToRemove).ToArray();

        var newValue = Variant.From(withoutOldTag);
        GetEditedObject().Set(GetEditedProperty(), newValue);

        EmitChanged(GetEditedProperty(), newValue);
        button.ReleaseFocus();
    }

    private void ClearSignals(GodotObject obj, StringName signalName)
    {
        var signals = obj.GetSignalConnectionList(signalName);
        foreach (var signal in signals)
        {
            obj.Disconnect(signalName, signal["callable"].AsCallable());
        }
    }

    public static TagsAttribute GetAttribute(GodotObject obj, string name)
    {
        if (obj.GetScript().Obj is not CSharpScript script) return null;
        Type scriptType = ByName(Path.GetFileNameWithoutExtension(script.ResourcePath));
        var member = scriptType.GetMember(name, (BindingFlags)62);
        if (member.Length == 0) return null;
        if (Attribute.GetCustomAttribute(member[0], typeof(TagsAttribute)) is not TagsAttribute attribute) return null;
        return attribute;
    }

    private static Type ByName(string name)
    {
        return Assembly.GetExecutingAssembly().GetTypes().First(t => t.Name == name);
    }
}
#endif