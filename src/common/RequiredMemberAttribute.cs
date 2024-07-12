#if !NET7_0_OR_GREATER

namespace System.Runtime.CompilerServices
{
	/// <summary/>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
	internal sealed class RequiredMemberAttribute : Attribute
	{ }

	/// <summary/>
	[AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = false)]
	internal sealed class CompilerFeatureRequiredAttribute(string featureName) :
		Attribute
	{
		/// <summary/>
		public string FeatureName { get; } = featureName;

		/// <summary/>
		public bool IsOptional { get; set; }
	}
}

namespace System.Diagnostics.CodeAnalysis
{
	/// <summary/>
	[AttributeUsage(AttributeTargets.Constructor, AllowMultiple = false, Inherited = false)]
	internal sealed class SetsRequiredMembersAttribute : Attribute
	{ }
}

#endif
