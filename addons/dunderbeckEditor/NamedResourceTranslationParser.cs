#if TOOLS
using System.Collections.Generic;
using Godot;

namespace Dunderbeck.addons.DunderbeckEditor;

[Tool]
public partial class NamedResourceTranslationParser : EditorTranslationParserPlugin
{
    public override string[] _GetRecognizedExtensions() => new[] { "tres", "res" };

    public override Godot.Collections.Array<string[]> _ParseFile(string path)
    {
        GdCol.Array<GdCol.Array<string>> ret = [];
        string[] propNames = ["Name", "Description"];
        
        if (!ResourceLoader.Exists(path)) return ret;
        
        Resource resource = ResourceLoader.Load(path);
        if (resource == null) return ret;

        foreach (string propName in propNames)
        {
            if (!resource.GetPropertyList().Any(p => p["name"].AsString() == propName)) continue;

            Variant stringVariant = resource.Get(propName);
            if (stringVariant.VariantType != Variant.Type.String) continue;

            string value = stringVariant.AsString();
            if (!string.IsNullOrWhiteSpace(value))
            {
                ret.Add(new GdCol.Array<string[]> { value });
            }
        }
        return ret;
    }
}
#endif