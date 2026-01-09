using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Xunit.Runner.Common;
using Xunit.Sdk;

namespace Xunit.Internal;

/// <summary>
/// INTERNAL CLASS. DO NOT USE.
/// </summary>
public abstract class ResultMetadataMessageHandlerBase<TResultMetadata> : IMessageSink
	where TResultMetadata : ResultMetadataBase
{
	readonly ConcurrentDictionary<string, TResultMetadata> resultMetadataByAssemblyID = [];

	internal IReadOnlyCollection<TResultMetadata> ResultMetadataValues =>
		resultMetadataByAssemblyID.Values.CastOrToReadOnlyCollection();

	internal abstract TResultMetadata CreateMetadata();

	internal TResultMetadata GetOrAddResultMetadata(string assemblyUniqueID) =>
		resultMetadataByAssemblyID.GetOrAdd(assemblyUniqueID, _ => CreateMetadata());

	internal bool TryGetResultMetadata(string assemblyUniqueID, [NotNullWhen(true)] out TResultMetadata? metadata) =>
		resultMetadataByAssemblyID.TryGetValue(assemblyUniqueID, out metadata);

	void HandleTestAssemblyFinished(MessageHandlerArgs<ITestAssemblyFinished> args)
	{
		var message = args.Message;
		if (!TryGetResultMetadata(message.AssemblyUniqueID, out var resultMetadata))
			return;

		OnTestAssemblyFinished(message, resultMetadata);

		resultMetadata.MetadataCache.TryRemove(message);
	}

	void HandleTestAssemblyStarting(MessageHandlerArgs<ITestAssemblyStarting> args)
	{
		var message = args.Message;
		var resultMetadata = GetOrAddResultMetadata(message.AssemblyUniqueID);

		resultMetadata.MetadataCache.Set(message);

		OnTestAssemblyStarting(message, resultMetadata);
	}

	void HandleTestCaseFinished(MessageHandlerArgs<ITestCaseFinished> args)
	{
		var message = args.Message;
		if (!TryGetResultMetadata(message.AssemblyUniqueID, out var resultMetadata))
			return;

		OnTestCaseFinished(message, resultMetadata);

		resultMetadata.MetadataCache.TryRemove(message);
	}

	void HandleTestCaseStarting(MessageHandlerArgs<ITestCaseStarting> args)
	{
		var message = args.Message;
		if (!TryGetResultMetadata(message.AssemblyUniqueID, out var resultMetadata))
			return;

		resultMetadata.MetadataCache.Set(message);

		OnTestCaseStarting(message, resultMetadata);
	}

	void HandleTestClassFinished(MessageHandlerArgs<ITestClassFinished> args)
	{
		var message = args.Message;
		if (!TryGetResultMetadata(message.AssemblyUniqueID, out var resultMetadata))
			return;

		OnTestClassFinished(message, resultMetadata);

		resultMetadata.MetadataCache.TryRemove(message);
	}

	void HandleTestClassStarting(MessageHandlerArgs<ITestClassStarting> args)
	{
		var message = args.Message;
		if (!TryGetResultMetadata(message.AssemblyUniqueID, out var resultMetadata))
			return;

		resultMetadata.MetadataCache.Set(message);

		OnTestClassStarting(message, resultMetadata);
	}

	void HandleTestCollectionFinished(MessageHandlerArgs<ITestCollectionFinished> args)
	{
		var message = args.Message;
		if (!TryGetResultMetadata(message.AssemblyUniqueID, out var resultMetadata))
			return;

		OnTestCollectionFinished(message, resultMetadata);

		resultMetadata.MetadataCache.TryRemove(message);
	}

	void HandleTestCollectionStarting(MessageHandlerArgs<ITestCollectionStarting> args)
	{
		var message = args.Message;
		if (!TryGetResultMetadata(message.AssemblyUniqueID, out var resultMetadata))
			return;

		resultMetadata.MetadataCache.Set(message);

		OnTestCollectionStarting(message, resultMetadata);
	}

	void HandleTestFinished(MessageHandlerArgs<ITestFinished> args)
	{
		var message = args.Message;
		if (!TryGetResultMetadata(message.AssemblyUniqueID, out var resultMetadata))
			return;

		OnTestFinished(message, resultMetadata);

		resultMetadata.MetadataCache.TryRemove(message);
	}

	void HandleTestMethodFinished(MessageHandlerArgs<ITestMethodFinished> args)
	{
		var message = args.Message;
		if (!TryGetResultMetadata(message.AssemblyUniqueID, out var resultMetadata))
			return;

		OnTestMethodFinished(message, resultMetadata);

		resultMetadata.MetadataCache.TryRemove(message);
	}

	void HandleTestMethodStarting(MessageHandlerArgs<ITestMethodStarting> args)
	{
		var message = args.Message;
		if (!TryGetResultMetadata(message.AssemblyUniqueID, out var resultMetadata))
			return;

		resultMetadata.MetadataCache.Set(message);

		OnTestMethodStarting(message, resultMetadata);
	}

	void HandleTestStarting(MessageHandlerArgs<ITestStarting> args)
	{
		var message = args.Message;
		if (!TryGetResultMetadata(message.AssemblyUniqueID, out var resultMetadata))
			return;

		resultMetadata.MetadataCache.Set(message);

		OnTestStarting(message, resultMetadata);
	}

	/// <inheritdoc/>
	public virtual bool OnMessage(IMessageSinkMessage message)
	{
		message.DispatchWhen<ITestAssemblyFinished>(HandleTestAssemblyFinished);
		message.DispatchWhen<ITestAssemblyStarting>(HandleTestAssemblyStarting);
		message.DispatchWhen<ITestCaseFinished>(HandleTestCaseFinished);
		message.DispatchWhen<ITestCaseStarting>(HandleTestCaseStarting);
		message.DispatchWhen<ITestClassFinished>(HandleTestClassFinished);
		message.DispatchWhen<ITestClassStarting>(HandleTestClassStarting);
		message.DispatchWhen<ITestCollectionFinished>(HandleTestCollectionFinished);
		message.DispatchWhen<ITestCollectionStarting>(HandleTestCollectionStarting);
		message.DispatchWhen<ITestFinished>(HandleTestFinished);
		message.DispatchWhen<ITestMethodFinished>(HandleTestMethodFinished);
		message.DispatchWhen<ITestMethodStarting>(HandleTestMethodStarting);
		message.DispatchWhen<ITestStarting>(HandleTestStarting);

		return true;
	}

	internal virtual void OnTestAssemblyFinished(
		ITestAssemblyFinished message,
		TResultMetadata resultMetadata)
	{ }

	internal virtual void OnTestAssemblyStarting(
		ITestAssemblyStarting message,
		TResultMetadata resultMetadata)
	{ }

	internal virtual void OnTestCaseFinished(
		ITestCaseFinished message,
		TResultMetadata resultMetadata)
	{ }

	internal virtual void OnTestCaseStarting(
		ITestCaseStarting message,
		TResultMetadata resultMetadata)
	{ }

	internal virtual void OnTestClassFinished(
		ITestClassFinished message,
		TResultMetadata resultMetadata)
	{ }

	internal virtual void OnTestClassStarting(
		ITestClassStarting message,
		TResultMetadata resultMetadata)
	{ }

	internal virtual void OnTestCollectionFinished(
		ITestCollectionFinished message,
		TResultMetadata resultMetadata)
	{ }

	internal virtual void OnTestCollectionStarting(
		ITestCollectionStarting message,
		TResultMetadata resultMetadata)
	{ }

	internal virtual void OnTestFinished(
		ITestFinished message,
		TResultMetadata resultMetadata)
	{ }

	internal virtual void OnTestMethodFinished(
		ITestMethodFinished message,
		TResultMetadata resultMetadata)
	{ }

	internal virtual void OnTestMethodStarting(
		ITestMethodStarting message,
		TResultMetadata resultMetadata)
	{ }

	internal virtual void OnTestStarting(
		ITestStarting message,
		TResultMetadata resultMetadata)
	{ }
}
