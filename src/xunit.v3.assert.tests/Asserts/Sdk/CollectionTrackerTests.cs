using System;
using System.Collections.Generic;
using Xunit;
using Xunit.Sdk;

public class CollectionTrackerTests
{
	public class FormatIndexedMismatch_IEnumerable
	{
		[Fact]
		public static void ExceededDepth()
		{
			var tracker = new CollectionTracker<int>(new[] { 42, 2112 });

			var result = tracker.FormatIndexedMismatch(2600, out var pointerIndent, ArgumentFormatter.MAX_DEPTH);

			Assert.Equal("[···]", result);
			//            -^
			Assert.Equal(1, pointerIndent);
		}

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

	public class FormatIndexedMismatch_Span
	{
		[Fact]
		public static void ExceededDepth()
		{
			var span = new[] { 42, 2112 }.AsSpan();

			var result = CollectionTracker<int>.FormatIndexedMismatch(span, 2600, out var pointerIndent, ArgumentFormatter.MAX_DEPTH);

			Assert.Equal("[···]", result);
			//            -^
			Assert.Equal(1, pointerIndent);
		}

		[Fact]
		public static void SmallCollection_Start()
		{
			var span = new[] { 42, 2112 }.AsSpan();

			var result = CollectionTracker<int>.FormatIndexedMismatch(span, 0, out var pointerIndent);

			Assert.Equal("[42, 2112]", result);
			//            -^
			Assert.Equal(1, pointerIndent);
		}

		[Fact]
		public static void LargeCollection_Start()
		{
			var span = new[] { 1, 2, 3, 4, 5, 6, 7 }.AsSpan();

			var result = CollectionTracker<int>.FormatIndexedMismatch(span, 1, out var pointerIndent);

			Assert.Equal("[1, 2, 3, 4, 5, ···]", result);
			//            ----^
			Assert.Equal(4, pointerIndent);
		}

		[Fact]
		public static void LargeCollection_Mid()
		{
			var span = new[] { 1, 2, 3, 4, 5, 6, 7 }.AsSpan();

			var result = CollectionTracker<int>.FormatIndexedMismatch(span, 3, out var pointerIndent);

			Assert.Equal("[···, 2, 3, 4, 5, 6, ···]", result);
			//            ----|----|--^
			Assert.Equal(12, pointerIndent);
		}

		[Fact]
		public static void LargeCollection_End()
		{
			var span = new[] { 1, 2, 3, 4, 5, 6, 7 }.AsSpan();

			var result = CollectionTracker<int>.FormatIndexedMismatch(span, 6, out var pointerIndent);

			Assert.Equal("[···, 3, 4, 5, 6, 7]", result);
			//            ----|----|----|---^
			Assert.Equal(18, pointerIndent);
		}
	}

	public class FormatStart_IEnumerable_Tracked
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
			var tracker = new CollectionTracker<object>(new object[] { 1, 2.3M, "Hello, world!" });

			Assert.Equal($"[1, {2.3M}, \"Hello, world!\"]", tracker.FormatStart());
		}

		[CulturedFact]
		public static void Long()
		{
			var tracker = new CollectionTracker<object>(new object[] { 1, 2.3M, "Hello, world!", 42, 2112, new object() });

			Assert.Equal($"[1, {2.3M}, \"Hello, world!\", 42, 2112, ···]", tracker.FormatStart());
		}
	}

	public class FormatStart_IEnumerable_Untracked
	{
		[Fact]
		public static void Empty()
		{
			IEnumerable<object> collection = Array.Empty<object>();

			Assert.Equal("[]", CollectionTracker<object>.FormatStart(collection));
		}

		[Fact]
		public static void ExceededDepth()
		{
			IEnumerable<object> collection = Array.Empty<object>();

			Assert.Equal("[···]", CollectionTracker<object>.FormatStart(collection, ArgumentFormatter.MAX_DEPTH));
		}

		[CulturedFact]
		public static void Short()
		{
			IEnumerable<object> collection = new object[] { 1, 2.3M, "Hello, world!" };

			Assert.Equal($"[1, {2.3M}, \"Hello, world!\"]", CollectionTracker<object>.FormatStart(collection));
		}

		[CulturedFact]
		public static void Long()
		{
			IEnumerable<object> collection = new object[] { 1, 2.3M, "Hello, world!", 42, 2112, new object() };

			Assert.Equal($"[1, {2.3M}, \"Hello, world!\", 42, 2112, ···]", CollectionTracker<object>.FormatStart(collection));
		}
	}

	public class FormatStart_Span
	{
		[Fact]
		public static void Empty()
		{
			var span = Array.Empty<object>().AsSpan();

			Assert.Equal("[]", CollectionTracker<object>.FormatStart(span));
		}

		[Fact]
		public static void ExceededDepth()
		{
			var span = Array.Empty<object>().AsSpan();

			Assert.Equal("[···]", CollectionTracker<object>.FormatStart(span, ArgumentFormatter.MAX_DEPTH));
		}

		[CulturedFact]
		public static void Short()
		{
			var span = new object[] { 1, 2.3M, "Hello, world!" }.AsSpan();

			Assert.Equal($"[1, {2.3M}, \"Hello, world!\"]", CollectionTracker<object>.FormatStart(span));
		}

		[CulturedFact]
		public static void Long()
		{
			var span = new object[] { 1, 2.3M, "Hello, world!", 42, 2112, new object() }.AsSpan();

			Assert.Equal($"[1, {2.3M}, \"Hello, world!\", 42, 2112, ···]", CollectionTracker<object>.FormatStart(span));
		}
	}
}
