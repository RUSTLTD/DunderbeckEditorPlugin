#if TOOLS
using Godot;

namespace Dunderbeck.addons.DunderbeckEditor;

[Tool]
public partial class Plugin : EditorPlugin
{
	private TagsInspectorPlugin _inspectorPlugin;
	private NamedResourceTranslationParser _translationParser;
	
	public override void _EnterTree()
	{
		// Initialization of the plugin goes here.
		_inspectorPlugin = new TagsInspectorPlugin();
		AddInspectorPlugin(_inspectorPlugin);

		_translationParser = new NamedResourceTranslationParser();
		AddTranslationParserPlugin(_translationParser);
	}

	public override void _ExitTree()
	{
		// Clean-up of the plugin goes here.
		RemoveInspectorPlugin(_inspectorPlugin);
		//RemoveExportPlugin(_exportVersion);
		RemoveTranslationParserPlugin(_translationParser);
	}

	public override string _GetPluginName()
	{
		return "Dunderbeck Editor";
	}
}
#endif
