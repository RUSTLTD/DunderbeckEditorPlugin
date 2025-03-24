#if TOOLS
using Godot;

namespace Dunderbeck.addons.DunderbeckEditor;

[Tool]
public partial class TagsInspectorPlugin : EditorInspectorPlugin
{
    // Receive _ParseProperty callbacks for objects of any type
    public override bool _CanHandle(GodotObject obj) => true;

    public override bool _ParseProperty(GodotObject obj, Variant.Type type, string name, PropertyHint hintType,
        string hintString, PropertyUsageFlags usageFlags, bool wide)
    {
        // Tags are stored as string[] so anything not string[] isn't tags
        if (type != Variant.Type.PackedStringArray) return false;

        // Do some reflection to determine what the enum type of the field is
        var attribute = TagsEditor.GetAttribute(obj, name);
        if (attribute == null) return false;

        // Ok we should be all good to use this property in the inspector now.
        AddPropertyEditor(name, new TagsEditor());
        return true;
    }


}
#endif