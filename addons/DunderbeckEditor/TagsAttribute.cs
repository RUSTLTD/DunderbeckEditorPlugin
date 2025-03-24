using System;

namespace Dunderbeck.addons.DunderbeckEditor;

public class TagsAttribute : Attribute
{
    public Type TagsEnumType { get; }

    public TagsAttribute(Type tagsEnumType)
    {
        TagsEnumType = tagsEnumType;
    }
}
