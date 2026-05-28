using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Godot;

#if true
namespace Dunderbeck.addons.DunderbeckEditor;

[Tool]
public partial class TagsEditor : EditorProperty
{
    private LineEdit _searchInput;
    private ItemList _searchResults;
    private VBoxContainer _existingItems;
    private PackedScene _itemTemplateScene;

    public TagsEditor()
    {
        Control control = ResourceLoader.Load<PackedScene>("res://addons/dunderbeckEditor/TagsEditor.tscn")
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
        var tags = GetEditedObject().Get(GetEditedProperty()).AsStringArray();

        // Fetch UI template
        var itemControl = _existingItems.GetChild<Control>(0);
        if (itemControl == null) return;

        if (tags is {Length: >0})
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
                    // Duplicate template if we need more rows
                    item = (Control)itemControl.Duplicate();
                    _existingItems.AddChild(item);
                }
                else 
                {
                    item = _existingItems.GetChild<Control>(i);
                }

                Button removeButton = item.GetNode<Button>("RemoveButton");
                Label itemLabel = item.GetNode<Label>("Label");

                removeButton.Visible = true;
                itemLabel.Text = tags[i];

                // Clear previous connections
                ClearSignals(removeButton, BaseButton.SignalName.Pressed);

                // Pass the specific tag text directly
                string currentTag = tags[i];
                removeButton.Connect(BaseButton.SignalName.Pressed, 
                    Callable.From(() => RemoveTag(currentTag)));
            }
        }
        else
        {
            // Reset to "None"
            Button removeButton = itemControl.GetNode<Button>("RemoveButton");
            Label itemLabel = itemControl.GetNode<Label>("Label");
            
            removeButton.Visible = false;
            itemLabel.Text = "None";
            
            ClearSignals(removeButton, BaseButton.SignalName.Pressed);
            
            // Clean up any extra duplicated rows
            while (_existingItems.GetChildCount() > 1)
            {
                _existingItems.GetChild(1).QueueFree();
            }
        }

        DoSearch(_searchInput.Text);
    }

    private void ClearSignals(GodotObject obj, StringName signalName)
    {
        var signals = obj.GetSignalConnectionList(signalName);
        foreach (var signal in signals)
        {
            obj.Disconnect(signalName, signal["callable"].AsCallable());
        }
    }
    private static Type ByName(string name)
    {
        return Assembly.GetExecutingAssembly().GetTypes().First(t => t.Name == name);
        return Attribute.GetCustomAttribute(member[0], typeof(TagsAttribute)) as TagsAttribute;
    }
    
    private void DoSearch(string text)
    {
        _searchResults.Clear();

        var tags = GetEditedObject().Get(GetEditedProperty()).AsStringArray();
        TagsAttribute attribute = GetAttribute(GetEditedObject(), GetEditedProperty());
        
        if (attribute == null) return;

        foreach (var tag in Enum.GetNames(attribute.TagsEnumType))
        {
            if (!string.IsNullOrEmpty(text) && !tag.Contains(text, StringComparison.OrdinalIgnoreCase)) continue;
            if (tags.Contains(tag)) continue;
            _searchResults.AddItem(tag);
        }
    }

    private void AddTag(long selectionIndex)
    {
        string tagToAdd = _searchResults.GetItemText((int)selectionIndex);
        var tags = GetEditedObject().Get(GetEditedProperty()).AsStringArray();
        
        if (tags.Contains(tagToAdd)) return;

        var withNewTag = tags.Concat(new[] { tagToAdd }).ToArray();
        EmitChanged(GetEditedProperty(), Variant.From(withNewTag));
    }

    private void RemoveTag(string tagToRemove)
    {
        var tags = GetEditedObject().Get(GetEditedProperty()).AsStringArray();
        var withoutOldTag = tags.Where(t => t != tagToRemove).ToArray();

        var newValue = Variant.From(withoutOldTag);
        GetEditedObject().Set(GetEditedProperty(), newValue);

        EmitChanged(GetEditedProperty(), newValue);
    }

    public static TagsAttribute GetAttribute(GodotObject obj, string name)
    {
        if (obj.GetScript().Obj is not CSharpScript script) return null;
        
        var scriptType = script.New().GetType();
        var member = scriptType.GetMember(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        
        if (member.Length == 0) return null;
        return Attribute.GetCustomAttribute(member[0], typeof(TagsAttribute)) as TagsAttribute;
    }
}
#endif