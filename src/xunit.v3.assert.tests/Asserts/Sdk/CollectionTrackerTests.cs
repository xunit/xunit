using System;
using Xunit;
using Xunit.Sdk;

public class CollectionTrackerTests
{
	public class FormatIndexedMismatch
	{
		[Fact]
		public static void SmallCollection_Start()
		{
			var tracker = new CollectionTracker<int>(new[] { 42, 2112 });

			var result = tracker.FormatIndexedMismatch(0, out var pointerIndent);

			Assert.Equal("[42, 2112]", result);
			//            -^
			Assert.Equal(1, pointerIndent);
		}

		[Fact]
		public static void LargeCollection_Start()
		{
			var tracker = new CollectionTracker<int>(new[] { 1, 2, 3, 4, 5, 6, 7 });

			var result = tracker.FormatIndexedMismatch(1, out var pointerIndent);

			Assert.Equal("[1, 2, 3, 4, 5, ···]", result);
			//            ----^
			Assert.Equal(4, pointerIndent);
		}

		[Fact]
		public static void LargeCollection_Mid()
		{
			var tracker = new CollectionTracker<int>(new[] { 1, 2, 3, 4, 5, 6, 7 });

			var result = tracker.FormatIndexedMismatch(3, out var pointerIndent);

			Assert.Equal("[···, 2, 3, 4, 5, 6, ···]", result);
			//            ----|----|--^
			Assert.Equal(12, pointerIndent);
		}

		[Fact]
		public static void LargeCollection_End()
		{
			var tracker = new CollectionTracker<int>(new[] { 1, 2, 3, 4, 5, 6, 7 });

			var result = tracker.FormatIndexedMismatch(6, out var pointerIndent);

			Assert.Equal("[···, 3, 4, 5, 6, 7]", result);
			//            ----|----|----|---^
			Assert.Equal(18, pointerIndent);
		}
	}

	public class FormatStart
	{
		[Fact]
		public static void Empty()
		{
			var tracker = new CollectionTracker<object>(Array.Empty<object>());

			Assert.Equal("[]", tracker.FormatStart());
		}

		[Fact]
		public static void ExceededDepth()
		{
			var tracker = new CollectionTracker<object>(Array.Empty<object>());

			Assert.Equal("[···]", tracker.FormatStart(ArgumentFormatter.MAX_DEPTH));
		}

		[CulturedFact]
		public static void Short()
		{
			var expected = $"[1, {2.3M}, \"Hello, world!\"]";
			var tracker = new CollectionTracker<object>(new object[] { 1, 2.3M, "Hello, world!" });

			Assert.Equal(expected, tracker.FormatStart());
		}

		[CulturedFact]
		public static void Long()
		{
			var expected = $"[1, {2.3M}, \"Hello, world!\", 42, 2112, ···]";
			var tracker = new CollectionTracker<object>(new object[] { 1, 2.3M, "Hello, world!", 42, 2112, new object() });

			Assert.Equal(expected, tracker.FormatStart());
		}
	}
}
