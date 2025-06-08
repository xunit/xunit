---
layout: default
title: Using testconfig.json with Microsoft Testing Platform
breadcrumb: Documentation
---

# Using `testconfig.json` with Microsoft Testing Platform

_Last updated: 2025 June 7_

Beginning with xUnit.net v3 version `3.0.0-pre.15`, when running tests in Microsoft Testing Platform mode, you can utilize [`testconfig.json`](https://learn.microsoft.com/dotnet/core/testing/microsoft-testing-platform-config#testconfigjson) to provide test project configuration.

{: .note }
Using `testconfig.json` is only supported when running tests in Microsoft Testing Platform mode. Running tests any other way (including using our first party runners or any non-Microsoft Testing Plateform third party runner) does not support `testconfig.json`, and you should rely on [xUnit.net's native JSON configuration files](configuration-files) instead. For more information about v3 and Microsoft Testing Platform, see [our documentation page](/docs/getting-started/v3/microsoft-testing-platform).

## Table of contents

* [Format of the `testconfig.json` file](#format-of-the-testconfigjson-file)
* [Supported configuration items](#supported-configuration-items)
  * [`assertEquivalentMaxDepth`](#assertEquivalentMaxDepth)
  * [`culture`](#culture)
  * [`diagnosticMessages`](#diagnosticMessages)
  * [`explicit`](#explicit)
  * [`failSkips`](#failSkips)
  * [`failWarns`](#failWarns)
  * [`internalDiagnosticMessages`](#internalDiagnosticMessages)
  * [`longRunningTestSeconds`](#longRunningTestSeconds)
  * [`maxParallelThreads`](#maxParallelThreads)
  * [`methodDisplay`](#methodDisplay)
  * [`methodDisplayOptions`](#methodDisplayOptions)
  * [`parallelAlgorithm`](#parallelAlgorithm)
  * [`parallelizeTestCollections`](#parallelizeTestCollections)
  * [`preEnumerateTheories`](#preEnumerateTheories)
  * [`printMaxEnumerableLength`](#printMaxEnumerableLength)
  * [`printMaxObjectDepth`](#printMaxObjectDepth)
  * [`printMaxObjectMemberCount`](#printMaxObjectMemberCount)
  * [`printMaxStringLength`](#printMaxStringLength)
  * [`seed`](#seed)
  * [`showLiveOutput`](#showLiveOutput)
  * [`stopOnFail`](#stopOnFail)

## Format of the `testconfig.json` file

The `testconfig.json` is a standard JSON file that lives in the root of your test project. xUnit.net configuration items are placed into a top-level object named `xUnit`. For example, to set the runtime culture and disable parallelization:

{% highlight json %}
{
  "xUnit": {
    "culture": "en-GB",
    "parallelizeTestCollections": false
  }
}
{% endhighlight %}

## Supported configuration items

<table class="table">
  <tr>
    <th>Key</th>
    <th>Supported Values</th>
  </tr>
  <tr>
    <th id="assertEquivalentMaxDepth"><code>assertEquivalentMaxDepth</code></th>
    <td class="wrapped-wide">
      <p>
        Set this value to limit the recursive depth <code>Assert.Equivalent</code>
        will use when comparing objects for equivalence.
      </p>
      <p>
        This can also be set by environment variable <code>XUNIT_ASSERT_EQUIVALENT_MAX_DEPTH</code>.
        A value in RunSettings will take precedence over the environment variable.
      </p>
      <p><em>
        Valid values: Any integer &gt;= 1<br />
        Default value: <code>50</code>
      </em></p>
    </td>
  </tr>
  <tr>
    <th id="culture"><code>culture</code></th>
    <td class="wrapped-wide">
      <p>
        Set this value to override the default culture used to run all unit tests in
        the assembly. You can pass <code>default</code> to use the default behavior
        (use the default culture of the operating system); you can pass <code>invariant</code>
        to use the invariant culture; or you can pass any string which describes a
        valid culture on the target operating system (see <a href="https://www.rfc-editor.org/info/bcp47">BCP
        47</a> for a description of how culture names are formatted; note that the list
        of supported cultures will vary by target operating system).
      </p>
      <p><em>
        Valid values: <code>default</code>, <code>invariant</code>, any OS supported culture<br />
        Default value: <code>default</code>
      </em></p>
    </td>
  </tr>
  <tr>
    <th id="diagnosticMessages"><code>diagnosticMessages</code></th>
    <td class="wrapped-wide">
      <p>
        Set this value to <code>true</code> to include diagnostic information during test
        discovery and execution. Each runner has a unique method of presenting diagnostic
        messages.
      </p>
      <p><em>
        Valid values: <code>true</code>, <code>false</code><br />
        Default value: <code>false</code>
      </em></p>
    </td>
  </tr>
  <tr>
    <th id="explicit"><code>explicit</code></th>
    <td class="wrapped-wide">
      <p>Change the way explicit tests are handled:</p>
      <p>
        <ul>
          <li><code>on</code> Run both explicit and non-explicit tests</li>
          <li><code>off</code> Run only non-explicit tests</li>
          <li><code>only</code> Run only explicit tests</li>
        </ul>
      </p>
      <p><em>
        Valid values: <code>on</code>, <code>off</code>, <code>only</code><br />
        Default value: <code>off</code>
      </em></p>
    </td>
  </tr>
  <tr>
    <th id="failSkips"><code>failSkips</code></th>
    <td class="wrapped-wide">
      <p>
        Set this to <code>true</code> to enable skipped tests to be treated as errors.
      </p>
      <p><em>
        Valid values: <code>true</code>, <code>false</code><br />
        Default value: <code>false</code>
      </em></p>
    </td>
  </tr>
  <tr>
    <th id="failWarns"><code>failWarns</code></th>
    <td class="wrapped-wide">
      <p>
        Set this to <code>true</code> to enable warned tests to be treated as errors.
        By default, warnings will be reported with a passing test result.
      </p>
      <p><em>
        Valid values: <code>true</code>, <code>false</code><br />
        Default value: <code>false</code>
      </em></p>
    </td>
  </tr>
  <tr>
    <th id="internalDiagnosticMessages"><code>internalDiagnosticMessages</code></th>
    <td class="wrapped-wide">
      <p>
        Set this value to <code>true</code> to include internals diagnostic information during test
        discovery and execution. Each runner has a unique method of presenting diagnostic
        messages. Internal diagnostics often include information that is only useful when debugging
        the test framework itself.
      </p>
      <p><em>
        Valid values: <code>true</code>, <code>false</code><br />
        Default value: <code>false</code>
      </em></p>
    </td>
  </tr>
  <tr>
    <th id="longRunningTestSeconds"><code>longRunningTestSeconds</code></th>
    <td class="wrapped-wide">
      <p>
        Set this value to enable long-running (hung) test detection. When the runner is
        idle waiting for tests to finished, it will report that fact once the timeout
        has passed. Use a value of <code>0</code> to disable the feature, or a positive
        integer value to enable the feature (time in seconds).
      </p>
      <p>
        <strong>NOTE:</strong> Long running test messages are diagnostic messages. You
        must enable diagnostic messages in order to see the long running test warnings.
      </p>
      <p><em>
        Valid values: Any integer &gt;= 0<br />
        Default value: <code>0</code> (disabled)
      </em></p>
    </td>
  </tr>
  <tr>
    <th id="maxParallelThreads"><code>maxParallelThreads</code></th>
    <td class="wrapped-wide">
      <p>
        Set this to override the maximum number of threads to be used when parallelizing
        tests within this assembly. Use a value of <code>0</code> to indicate that you would
        like the default behavior; use a value of <code>-1</code> to indicate that you do not
        wish to limit the number of threads used for parallelization.
      </p>
      <p>
        As of v2 Test Framework 2.8.0+ and <code>xunit.runner.visualstudio</code> 2.8.0+, you can
        also use the multiplier syntax, which is a multiplier of the number of CPU threads. For
        example, <code>2.0x</code> will give you double the CPU thread count. You may also use
        the string values <code>unlimited</code> and <code>default</code> in place of the integer
        values <code>-1</code> and <code>0</code>, respectively.
      </p>
      <p><em>
        Valid values: Any integer &gt;= -1, a multiplier, <code>unlimited</code>, or <code>default</code><br />
        Default value: the number of CPU threads in your PC
      </em></p>
    </td>
  </tr>
  <tr>
    <th id="methodDisplay"><code>methodDisplay</code></th>
    <td class="wrapped-wide">
      <p>
        Set this to override the default display name for test cases. If you set this
        to <code>method</code>, the display name will be just the method (without the
        class name); if this set this value to <code>classAndMethod</code>, the
        default display name will be the fully qualified class name and method name.
      </p>
      <p><em>
        Valid values: <code>method</code>, <code>classAndMethod</code><br />
        Default value: <code>classAndMethod</code>
      </em></p>
    </td>
  </tr>
  <tr>
    <th id="methodDisplayOptions"><code>methodDisplayOptions</code></th>
    <td class="wrapped-wide">
      <p>
        Set this to automatically perform transforms on default test names. This value
        can either be <code>all</code>, <code>none</code>, or a comma-separated
        combination of one or more of the following values:
      </p>
      <ul>
        <li><code>replaceUnderscoreWithSpace</code> Replaces all underscores with spaces</li>
        <li>
          <code>useOperatorMonikers</code> Replaces operator names with matching symbols
          <ul>
            <li><code>eq</code> becomes <code>=</code></li>
            <li><code>ne</code> becomes <code>!=</code></li>
            <li><code>lt</code> becomes <code>&lt;</code></li>
            <li><code>le</code> becomes <code>&lt;=</code></li>
            <li><code>gt</code> becomes <code>&gt;</code></li>
            <li><code>ge</code> becomes <code>&gt;=</code></li>
          </ul>
        </li>
        <li>
          <code>useEscapeSequences</code> Replaces escape sequences in the format <code>Xnn</code>
          or <code>Unnnn</code> with their ASCII or Unicode character equivalents. Examples:
          <ul>
            <li><code>X2C</code> becomes <code>,</code></li>
            <li><code>U1D0C</code> becomes <code>&#x1d0c;</code></li>
          </ul>
        </li>
        <li>
          <code>replacePeriodWithComma</code> Replaced periods with a comma and a space. This
          option is typically only useful if <code>methodDisplay</code> is <code>classAndMethod</code>.
        </li>
      </ul>
      <p><em>
        Valid values: <code>all</code>, <code>none</code>, or comma separated flags<br />
        Default value: <code>none</code>
      </em></p>
    </td>
  </tr>
  <tr>
    <th id="parallelAlgorithm"><code>parallelAlgorithm</code></th>
    <td class="wrapped-wide">
      <p>
        Set this to change the way tests are scheduled when they're running in parallel. For more
        information, see <a href="running-tests-in-parallel#algorithms">Running Tests in Parallel</a>.
        Note that the algorithm only applies when you have <a href="#ParallelizeTestCollections">enabled
        test collection parallelism</a>, and are using a limited <a href="#MaxParallelThreads">number of
        threads</a> (i.e., not <code>unlimited</code> or <code>-1</code>).
      </p>
      <p><em>
        Valid values: <code>conservative</code>, <code>aggressive</code><br />
        Default value: <code>conservative</code>
      </em></p>
    </td>
  </tr>
  <tr>
    <th id="parallelizeTestCollections"><code>parallelizeTestCollections</code></th>
    <td class="wrapped-wide">
      <p>
        Set this to <code>true</code> if the assembly is willing to run tests inside
        this assembly in parallel against each other. Tests in the same test collection
        will be run sequentially against each other, but tests in different test
        collections will be run in parallel against each other. Set this to
        <code>false</code> to disable all parallelization within this test assembly.
      </p>
      <p><em>
        Valid values: <code>true</code>, <code>false</code><br />
        Default value: <code>true</code>
      </em></p>
    </td>
  </tr>
  <tr>
    <th id="preEnumerateTheories"><code>preEnumerateTheories</code>
    <td class="wrapped-wide">
      <p>
        Set this to <code>true</code> to pre-enumerate theories so that there is an
        individual test case for each theory data row. Set this to <code>false</code>
        to return a single test case for each theory without pre-enumerating the
        data ahead of time (this is how xUnit.net v1 used to behave). This is
        most useful for developers running tests inside Visual Studio, who wish to
        have the Code Lens test runner icons on their theory methods, since Code
        Lens does not support multiple tests from a single method.
      </p>
      <p>
        This value does not have a default, because it's up to each individual test runner
        to decide what the best default behavior is. The Visual Studio adapter, for example,
        will default to <code>true</code>, while the console and MSBuild runners will default
        to <code>false</code>.
      </p>
      <p><em>
        Valid values: <code>true</code>, <code>false</code><br />
      </em></p>
    </td>
  </tr>
  <tr>
    <th id="printMaxEnumerableLength"><code>printMaxEnumerableLength</code></th>
    <td class="wrapped-wide">
      <p>
        Set this value to limit the number of items to print in a collection (followed by an
        ellipsis when the collection is longer). This is also used when printing into the
        middle of a collection with a mismatch index, which means the printing may also
        start with an ellipsis.
      </p>
      <p>
        Set this to <code>0</code> to always print the full collection.
      </p>
      <p>
        This can also be set by environment variable <code>XUNIT_PRINT_MAX_ENUMERABLE_LENGTH</code>.
        A value in RunSettings will take precedence over the environment variable.
      </p>
      <p><em>
        Valid values: Any integer &gt;= 0<br />
        Default value: <code>5</code>
      </em></p>
    </td>
  </tr>
  <tr>
    <th id="printMaxObjectDepth"><code>printMaxObjectDepth</code></th>
    <td class="wrapped-wide">
      <p>
        Set this value to limit the recursive depth when printing objects (followed by an
        ellipsis when the object depth is too deep).
      </p>
      <p>
        Set this to <code>0</code> to always print objects at all depths.
      </p>
      <p>
        <strong><em>Important warning: disabling this when printing objects with circular references
        could result in an infinite loop that will cause an <code>OutOfMemoryException</code> and crash
        the test runner process.</em></strong>
      </p>
      <p>
        This can also be set by environment variable <code>XUNIT_PRINT_MAX_OBJECT_DEPTH</code>.
        A value in RunSettings will take precedence over the environment variable.
      </p>
      <p><em>
        Valid values: Any integer &gt;= 0<br />
        Default value: <code>3</code>
      </em></p>
    </td>
  </tr>
  <tr>
    <th id="printMaxObjectMemberCount"><code>printMaxObjectMemberCount</code></th>
    <td class="wrapped-wide">
      <p>
        Set this value to limit the the number of members (fields and properties) to include
        when printing objects (followed by an ellipsis when there are more members).
      </p>
      <p>
        Set this to <code>0</code> to always print all members.
      </p>
      <p>
        This can also be set by environment variable <code>XUNIT_PRINT_MAX_OBJECT_MEMBER_COUNT</code>.
        A value in RunSettings will take precedence over the environment variable.
      </p>
      <p><em>
        Valid values: Any integer &gt;= 0<br />
        Default value: <code>5</code>
      </em></p>
    </td>
  </tr>
  <tr>
    <th id="printMaxStringLength"><code>printMaxStringLength</code></th>
    <td class="wrapped-wide">
      <p>
        Set this value to limit the number of characters to print in a string (followed by an
        ellipsis when the collection is longer). This is also used when printing into the
        middle of a string with a mismatch index, which means the printing may also
        start with an ellipsis.
      </p>
      <p>
        Set this to <code>0</code> to always print full strings.
      </p>
      <p>
        This can also be set by environment variable <code>XUNIT_PRINT_MAX_STRING_LENGTH</code>.
        A value in RunSettings will take precedence over the environment variable.
      </p>
      <p><em>
        Valid values: Any integer &gt;= 0<br />
        Default value: <code>50</code>
      </em></p>
    </td>
  </tr>
  <tr>
    <th id="seed"><code>seed</code></th>
    <td class="wrapped-wide">
      <p>
        Set this to set the seed used for randomization (affects how the test cases are randomized).
        This is only valid for v3.0+ test assemblies; it will be ignored for v1 or v2 assemblies.
        If the seed value isn't set, then the system will determine a reasonable seed (and print
        that seed when running the test assembly, to assist you in reproducing order-dependent
        failures).
      </p>
      <p><em>
        Valid values: between <code>0</code> and <code>2147483647</code><br />
        Default value: unset
      </em></p>
    </td>
  </tr>
  <tr>
    <th id="showLiveOutput"><code>showLiveOutput</code></th>
    <td class="wrapped-wide">
      <p>
        Set this to <code>true</code> to show messages from <code>ITestOutputHelper</code>
        live during the test run, in addition to showing them after the test has completed;
        set to <code>false</code> to only show test output messages after the test has
        completed. Note: when using <code>dotnet test</code> you may need to pass an extra
        command line option (<code>--logger "console;verbosity=normal"</code>) to be able
        to see messages from xUnit.net, as they are hidden by default.
      </p>
      <p><em>
        Valid values: <code>true</code>, <code>false</code><br />
        Default value: <code>false</code>
      </em></p>
    </td>
  </tr>
  <tr>
    <th id="stopOnFail"><code>stopOnFail</code></th>
    <td class="wrapped-wide">
      <p>
        Set this to <code>true</code> to stop running further tests once a test has failed.
        (Because of the asynchronous nature of test execution, this will not necessarily happen
        immediately; any test that is already in flight may complete, which may result in multiple
        test failures reported.)
      </p>
      <p><em>
        Valid values: <code>true</code>, <code>false</code><br />
        Default value: <code>false</code>
      </em></p>
    </td>
  </tr>
</table>
