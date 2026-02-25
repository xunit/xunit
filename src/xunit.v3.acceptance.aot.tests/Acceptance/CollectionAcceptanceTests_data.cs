using Xunit;

public partial class CollectionAcceptanceTests
{
	[Collection("Explicit Collection")]
#if XUNIT_AOT
	public
#endif
	class ClassInExplicitCollection
	{
		[Fact]
		public void Passing() { }
	}

#if XUNIT_AOT
	public
#endif
	class ClassInDefaultCollection
	{
		[Fact]
		public void Passing() { }
	}
}
