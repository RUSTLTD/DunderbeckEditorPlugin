#if TOOLS
using System.Collections.Generic;
using Godot;

namespace Dunderbeck.addons.DunderbeckEditor;

[Tool]
public partial class NamedResourceTranslationParser : EditorTranslationParserPlugin
{
    public override string[] _GetRecognizedExtensions() => new[] { "tres" };

    public override void _ParseFile(string path, GdCol.Array<string> msgids, GdCol.Array<GdCol.Array> msgidsContextPlural)
    {
        Resource resource = ResourceLoader.Load(path);

        HashSet<string> propNames = ["Name", "Description"];
        foreach (string propName in propNames)
        {
            Variant stringVariant = resource.Get(propName);
            if (stringVariant.VariantType != Variant.Type.String) return;
            msgids.Add(stringVariant.AsString());
        }
    }
}
#endif
