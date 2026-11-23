using Godot;

namespace PVZHE.ModEditor.ResourceEditors.GUI;

public sealed class XWInlineTextProperty
{
	public Resource Owner { get; set; }

	public StringName Property { get; set; }

	public Variant.Type VariantType { get; set; }

	public PropertyHint Hint { get; set; }

	public string HintString { get; set; } = "";

	public string Category { get; set; } = "General";

	public string OwnerType { get; set; } = "Resource";

	public string Label { get; set; } = "";

	public XWInlineTextRole Role { get; set; }

	public string ClassificationReason { get; set; } = "";

	public bool IsSemantic
	{
		get
		{
			XWInlineTextRole role = Role;
			if ((uint)role <= 4u)
			{
				return true;
			}
			return false;
		}
	}

	public bool IsTechnical => !IsSemantic;

	public bool IsTextEnum
	{
		get
		{
			bool flag = Hint == PropertyHint.Enum;
			if (flag)
			{
				Variant.Type variantType = VariantType;
				bool flag2 = ((variantType == Variant.Type.String || variantType == Variant.Type.StringName) ? true : false);
				flag = flag2;
			}
			return flag;
		}
	}

	public bool IsNodePath => VariantType == Variant.Type.NodePath;

	public string CoverageKey => $"{OwnerType}.{Property}";
}
