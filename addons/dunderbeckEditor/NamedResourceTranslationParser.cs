#if TOOLS
using System.Collections.Generic;
using Godot;

namespace Dunderbeck.addons.DunderbeckEditor;

[Tool]
public partial class NamedResourceTranslationParser : EditorTranslationParserPlugin
{
    public override string[] _GetRecognizedExtensions() => new[] { "tres" };

    public override GdCol.Array<string[]> _ParseFile(string path)
    {
        Godot.Collections.Array<string[]> ret = [];
        HashSet<string> propNames = ["Name", "Description"];
        Resource resource = ResourceLoader.Load(path);
        foreach (string propName in propNames)
        {
            Variant stringVariant = resource.Get(propName);
            if (stringVariant.VariantType != Variant.Type.String) continue;
            ret.Add([stringVariant.AsString()]);
        }
        return ret;
    }
}
#endif