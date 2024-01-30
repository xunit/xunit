using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// 
/// </summary>
public sealed class CollectionFixtureMappingManager
{
	Dictionary<Type, object> _fixtureObjects = new();
	Dictionary<Type, Func<Type[], object?>> _fixtureFunctions = new();
	readonly ExceptionAggregator _aggregator;

	internal CollectionFixtureMappingManager(ExceptionAggregator aggregator)
	{
		_aggregator = aggregator;
	}

	internal void AddFixture(Type fixtureType, object value)
	{
		_fixtureObjects[fixtureType] = value;
	}

	internal void AddFixtureForType(
		Type fixtureType,
		IReadOnlyDictionary<Type, object> assemblyFixtureMappings)
	{
		Guard.ArgumentNotNull(fixtureType);
		Guard.ArgumentNotNull(assemblyFixtureMappings);

		var ctors =
			fixtureType
				.GetConstructors()
				.Where(ci => !ci.IsStatic && ci.IsPublic)
				.ToList();

		if (ctors.Count != 1)
		{
			_aggregator.Add(new TestClassException(string.Format(CultureInfo.CurrentCulture, "Collection fixture type '{0}' may only define a single public constructor.", fixtureType.FullName)));
			return;
		}

		var ctor = ctors[0];
		var missingParameters = new List<ParameterInfo>();
		var ctorArgs = ctor.GetParameters().Select(p =>
		{
			object? arg = null;
			if (p.ParameterType == typeof(_IMessageSink))
				arg = TestContext.Current?.DiagnosticMessageSink;
			else if (p.ParameterType == typeof(ITestContextAccessor))
				arg = TestContextAccessor.Instance;
			else if (!assemblyFixtureMappings.TryGetValue(p.ParameterType, out arg))
				missingParameters.Add(p);
			return arg;
		}).ToArray();

		if (missingParameters.Count > 0)
			_aggregator.Add(
				new TestClassException(
					string.Format(
						CultureInfo.CurrentCulture,
						"Collection fixture type '{0}' had one or more unresolved constructor arguments: {1}",
						fixtureType.FullName,
						string.Join(", ", missingParameters.Select(p => string.Format(CultureInfo.CurrentCulture, "{0} {1}", p.ParameterType.Name, p.Name)))
					)
				)
			);
		else if (fixtureType.ContainsGenericParameters)
			_fixtureFunctions[fixtureType.GetGenericTypeDefinition()] = (typeArgs) => OnGenericFixtureCreation(fixtureType.GetGenericTypeDefinition(), typeArgs, ctorArgs);
		else
			_aggregator.Run(() =>
			{
				try
				{
					_fixtureObjects[fixtureType] = ctor.Invoke(ctorArgs);
				}
				catch (Exception ex)
				{
					throw new TestClassException(string.Format(CultureInfo.CurrentCulture, "Collection fixture type '{0}' threw in its constructor", fixtureType.FullName), ex.Unwrap());
				}
			});
	}

	private object? OnGenericFixtureCreation(Type fixtureType, Type[] typeArgs, object?[]? ctorArgs)
	{
		// Assume validation has already been done to ensure only a single relevant constructor
		var newType = fixtureType.MakeGenericType(typeArgs);
		var ctor = newType
			.GetConstructors()
			.Where(ci => !ci.IsStatic && ci.IsPublic)
			.Single();

		_aggregator.Run(() =>
		{
			try
			{
				_fixtureObjects[newType] = ctor.Invoke(ctorArgs);
			}
			catch (Exception ex)
			{
				throw new TestClassException(string.Format(CultureInfo.CurrentCulture, "Collection fixture type '{0}' threw in its constructor", newType.FullName), ex.Unwrap());
			}
		});

		if (_fixtureObjects.TryGetValue(newType, out var newObj))
		{
			// Initialize the new object if required
			if (newObj is IAsyncLifetime toInit)
			{
				_aggregator.Run(async () =>
				{
					try
					{
						await toInit.InitializeAsync();
					}
					catch (Exception ex)
					{
						throw new TestClassException(string.Format(CultureInfo.CurrentCulture, "Collection fixture type '{0}' threw in InitializeAsync", toInit.GetType().FullName), ex.Unwrap());
					}
				});
			}
			return newObj;
		}

		return null;
	}

	internal async ValueTask OnFinished()
	{
		var disposeAsyncTasks =
			_fixtureObjects
				.Values
				.OfType<IAsyncDisposable>()
				.Select(fixture => _aggregator.RunAsync(async () =>
				{
					try
					{
						await fixture.DisposeAsync();
					}
					catch (Exception ex)
					{
						throw new TestFixtureCleanupException(string.Format(CultureInfo.CurrentCulture, "Collection fixture type '{0}' threw in DisposeAsync", fixture.GetType().FullName), ex.Unwrap());
					}
				}).AsTask())
				.ToList();

		await Task.WhenAll(disposeAsyncTasks);

		foreach (var fixture in _fixtureObjects.Values.OfType<IDisposable>())
			_aggregator.Run(() =>
			{
				try
				{
					fixture.Dispose();
				}
				catch (Exception ex)
				{
					throw new TestFixtureCleanupException(string.Format(CultureInfo.CurrentCulture, "Collection fixture type '{0}' threw in Dispose", fixture.GetType().FullName), ex.Unwrap());
				}
			});
	}

	internal bool TryGetValue(Type fixtureType, out object? value)
	{
		if (_fixtureObjects.TryGetValue(fixtureType, out value))
			return true;

		if (fixtureType.IsGenericType)
		{
			var genericFixtureType = fixtureType.GetGenericTypeDefinition();
			if (_fixtureFunctions.TryGetValue(genericFixtureType, out var generator))
			{
				value = generator(fixtureType.GetGenericArguments());
			}
			return value is not null;
		}

		return false;
	}

	internal ValueTask OnInitialize()
	{
		var initializeAsyncTasks =
			_fixtureObjects
				.Values
				.OfType<IAsyncLifetime>()
				.Select(
					fixture => _aggregator.RunAsync(async () =>
					{
						try
						{
							await fixture.InitializeAsync();
						}
						catch (Exception ex)
						{
							throw new TestClassException(string.Format(CultureInfo.CurrentCulture, "Collection fixture type '{0}' threw in InitializeAsync", fixture.GetType().FullName), ex.Unwrap());
						}
					}).AsTask()
				)
				.ToList();

		return new(Task.WhenAll(initializeAsyncTasks));
	}

	internal IEnumerable<object> Values => _fixtureObjects.Values;
}
