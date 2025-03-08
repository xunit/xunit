---
layout: default
title: "Code Coverage with Microsoft Testing Platform"
breadcrumb: Documentation
---

# {{ page.title }}

_Last updated: 2025 January 14_

With the new [Microsoft Testing Platform support](microsoft-testing-platform) (MTP) in xUnit.net v3, getting code coverage has changed when running with MTP. This document discusses how to enable code coverage for both the MTP native command line via `dotnet run` as well as when using `dotnet test`.

_The samples here are done with xUnit.net v3 `1.0.1` and .NET SDK `9.0.101`. Your output text or version numbers may look slightly different from the examples given here._

## Setting up a sample project

We're going to walk through a simple sample project so we can see code coverage in action with MTP. These quick instructions will assume you're comfortable using the command line .NET SDK tooling.

_**If you want to skip these steps and just download the sample project, it's [available as a ZIP file](/downloads/CodeCoverageSample.zip).**_

1. If you have not already installed the v3 templates, please do so now:

   ```
   $ dotnet new install xunit.v3.templates

   The following template packages will be installed:
     xunit.v3.templates

   Success: xunit.v3.templates::1.0.1 installed the following templates:
   Template Name                   Short Name        Language    Tags
   ------------------------------  ----------------  ----------  ----------
   xUnit.net v3 Extension Project  xunit3-extension  [C#],F#,VB  Test/xUnit
   xUnit.net v3 Test Project       xunit3            [C#],F#,VB  Test/xUnit
   ```

1. Create a folder for our sample project, and create our solution file:

   ```
   $ mkdir CodeCoverageSample

   $ cd CodeCoverageSample

   $ dotnet new sln

   The template "Solution File" was created successfully.
   ```

1. Create a class library and add it to the solution:

   ```
   $ dotnet new classlib -f net8.0 -o ClassLibrary

   The template "Class Library" was created successfully.

   Processing post-creation actions...
   Restoring ClassLibrary\ClassLibrary.csproj:
   Restore succeeded.

   $ dotnet sln add ClassLibrary

   Project `ClassLibrary\ClassLibrary.csproj` added to the solution.
   ```

   Replace the contents of `ClassLibrary\Class1.cs` with the following:

   ```csharp
   namespace ClassLibrary;

   public class Class1
   {
       public static int Add(int x, int y) =>
           x + y;

       public static int Subtract(int x, int y) =>
           x - y;
   }
   ```

   This will give us code for us to measure coverage against.

1. Create the unit test project, add it to the solution, and add a reference to the class library:

   ```
   $ dotnet new xunit3 -o Tests

   The template "xUnit.net v3 Test Project" was created successfully.

   Processing post-creation actions...
   Restoring Tests\Tests.csproj:
   Restore succeeded.

   $ dotnet sln add Tests

   Project `Tests\Tests.csproj` added to the solution.

   $ dotnet add Tests reference ClassLibrary

   Reference `..\ClassLibrary\ClassLibrary.csproj` added to the project.
   ```

   Replace the contents of `Tests\UnitTest1.cs` with the following:

   ```csharp
   using ClassLibrary;

   public class UnitTest1
   {
       [Fact]
       public void AddTest()
       {
           Assert.Equal(5, Class1.Add(2, 3));
       }
   }
   ```

   Finally, edit `Tests\Tests.csproj` and add the following items to the top `<PropertyGroup>`:

   ```xml
   <TestingPlatformDotnetTestSupport>true</TestingPlatformDotnetTestSupport>
   <UseMicrosoftTestingPlatformRunner>true</UseMicrosoftTestingPlatformRunner>
   ```

1. With all of this done, let's ensure that our tests are able to run, and that we're in MTP mode:

   ```
   $ dotnet run --project Tests

   xUnit.net v3 Microsoft.Testing.Platform Runner v1.0.1+654c6156d8 (64-bit .NET 8.0.12)

   Test run summary: Passed! - Tests\bin\Debug\net8.0\Tests.dll (net8.0|x64)
     total: 1
     failed: 0
     succeeded: 1
     skipped: 0
     duration: 163ms
   ```

## Enabling code coverage

With xUnit.net v3 and MTP, there is a solution for code coverage that's supported directly by Microsoft.

One of the big new features for MTP is for the ability of developers to write "extensions" that can extend the functionality of unit testing frameworks by mixing in functionality via custom command line options. The [documentation page for MTP extensions](https://learn.microsoft.com/dotnet/core/testing/unit-testing-platform-extensions) is a good place to start when looking for these extensions.

Today we're focusing on the [Microsoft Code Coverage extension](https://learn.microsoft.com/dotnet/core/testing/unit-testing-platform-extensions-code-coverage#microsoft-code-coverage). To use this extension, we must add a package reference like this:

```
$ dotnet add Tests package Microsoft.Testing.Extensions.CodeCoverage

Build succeeded in 0.4s
info : X.509 certificate chain validation will use the default trust store selected by .NET for code signing.
info : X.509 certificate chain validation will use the default trust store selected by .NET for timestamping.
info : Adding PackageReference for package 'Microsoft.Testing.Extensions.CodeCoverage' into project 'Tests\Tests.csproj'.
info :   CACHE https://api.nuget.org/v3/registration5-gz-semver2/microsoft.testing.extensions.codecoverage/index.json
info : Restoring packages for Tests\Tests.csproj...
info :   CACHE https://api.nuget.org/v3/vulnerabilities/index.json
info :   CACHE https://api.nuget.org/v3-vulnerabilities/2025.01.10.05.21.07/vulnerability.base.json
info :   CACHE https://api.nuget.org/v3-vulnerabilities/2025.01.10.05.21.07/2025.01.14.18.46.31/vulnerability.update.json
info : Package 'Microsoft.Testing.Extensions.CodeCoverage' is compatible with all the specified frameworks in project 'Tests\Tests.csproj'.
info : PackageReference for package 'Microsoft.Testing.Extensions.CodeCoverage' version '17.13.1' added to file 'Tests\Tests.csproj'.
info : Generating MSBuild file Tests\obj\Tests.csproj.nuget.g.props.
info : Generating MSBuild file Tests\obj\Tests.csproj.nuget.g.targets.
info : Writing assets file to disk. Path: Tests\obj\project.assets.json
log  : Restored\Tests\Tests.csproj (in 125 ms).
```

Once this reference has been added, we can see that four new command line switches have become available to us:

```
$ dotnet run --project Tests -- -?

[...]
    --coverage
        Collect the code coverage using dotnet-coverage tool

    --coverage-output
        Output file

    --coverage-output-format
        Output file format. Supported values: 'coverage', 'xml' and 'cobertura'

    --coverage-settings
        XML code coverage settings
[...]
```

Passing `--coverage` is the minimum requirement for enabling code coverage; the other three switches influence how the coverage information is reported.

## Generating code coverage XML (with `dotnet run`)

Let's run our test project with coverage enabled, and ensure that it's generating XML formatted results for our report generator.

```
$ dotnet run --project Tests -- --coverage --coverage-output-format xml --coverage-output coverage.xml

xUnit.net v3 Microsoft.Testing.Platform Runner v1.0.1+654c6156d8 (64-bit .NET 8.0.12)

  In process file artifacts produced:
    - Tests\bin\Debug\net8.0\TestResults\coverage.xml

Test run summary: Passed! - Tests\bin\Debug\net8.0\Tests.dll (net8.0|x64)
  total: 1
  failed: 0
  succeeded: 1
  skipped: 0
  duration: 234ms
```

The output now shows the XML file that's been generated with code coverage information. If we peek at the first few lines of the XML file we should see coverage information like this:

```xml
[...]
  <modules>
    <module id="45E4B746EFBC114093524BF7E1A51B0FDAB54FCD" name="ClassLibrary.dll" path="ClassLibrary.dll" block_coverage="50.00" line_coverage="50.00" blocks_covered="1" blocks_not_covered="1" lines_covered="1" lines_partially_covered="0" lines_not_covered="1">
      <functions>
        <function id="8272" token="0x6000001" name="Add(int, int)" namespace="ClassLibrary" type_name="Class1" block_coverage="100.00" line_coverage="100.00" blocks_covered="1" blocks_not_covered="0" lines_covered="1" lines_partially_covered="0" lines_not_covered="0">
          <ranges>
            <range source_id="0" start_line="6" end_line="6" start_column="13" end_column="18" covered="yes" />
          </ranges>
        </function>
        <function id="8277" token="0x6000002" name="Subtract(int, int)" namespace="ClassLibrary" type_name="Class1" block_coverage="0.00" line_coverage="0.00" blocks_covered="0" blocks_not_covered="1" lines_covered="0" lines_partially_covered="0" lines_not_covered="1">
          <ranges>
            <range source_id="0" start_line="9" end_line="9" start_column="13" end_column="18" covered="no" />
          </ranges>
        </function>
      </functions>
      <source_files>
        <source_file id="0" path="ClassLibrary\Class1.cs" checksum_type="SHA256" checksum="08BEB1C8936879885D6559FC1664CA2A931A3435173A2F355B85F4DD38ABEE30" />
      </source_files>
      <skipped_functions />
    </module>
  </modules>
[...]
```

## Generating code coverage XML (with `dotnet test`)

Generating coverage XML is very similar to using `dotnet run`, except that we pass the extra arguments to `dotnet test` instead:

```
$ dotnet test -- --coverage --coverage-output-format xml --coverage-output coverage.xml

Restore complete (0.2s)
  ClassLibrary succeeded (0.1s) → ClassLibrary\bin\Debug\net8.0\ClassLibrary.dll
  Tests succeeded (0.4s) → Tests\bin\Debug\net8.0\Tests.dll
  Tests test succeeded (0.8s)

Test summary: total: 1, failed: 0, succeeded: 1, skipped: 0, duration: 0.7s
Build succeeded in 1.9s
```

Note that we don't see the name of the coverage output file, which is why we use `--coverage-output coverage.xml` so that the report always has a known filename.

If you run `dotnet test` and it runs multiple test projects, each test project's `TestResults` folder will have a `coverage.xml` file with the coverage results from that particular test run.

## Using ReportGenerator to create HTML from the XML

The XML isn't particularly pretty to look at, so for human consumption we'd like to see HTML reports.

We're using a tool called [ReportGenerator](https://github.com/danielpalme/ReportGenerator) to convert the XML coverage into HTML reports. If you don't already have this installed, you can run this command:

```
$ dotnet tool install --global dotnet-reportgenerator-globaltool

You can invoke the tool using the following command: reportgenerator
Tool 'dotnet-reportgenerator-globaltool' (version '5.4.3') was successfully installed.
```

Now we just need to generate the HTML report:

```
$ ReportGenerator -reports:Tests\bin\Debug\net8.0\TestResults\coverage.xml -targetdir:CoverageReport

2025-01-14T16:47:36: Arguments
2025-01-14T16:47:36:  -reports:Tests\bin\Debug\net8.0\TestResults\coverage.xml
2025-01-14T16:47:36:  -targetdir:CoverageReport
2025-01-14T16:47:36: Writing report file 'CoverageReport\index.html'
2025-01-14T16:47:36: Report generation took 0.1 seconds
```

If you open the `index.html` file in your browser you should see a report that looks something like this:

[![Code Coverage Report, Main Page](/images/getting-started/v3/code-coverage-report-1.png){: .border .full-width }](/images/getting-started/v3/code-coverage-report-1.png)

You can click on the `ClassLibrary.Class1` to dive into the coverage details:

[![Code Coverage Report, Detail Page](/images/getting-started/v3/code-coverage-report-2.png){: .border .full-width }](/images/getting-started/v3/code-coverage-report-2.png)

Code coverage is showing us that our test have only covered the `Add` method, and not the `Subtract` method, which is correct given that our unit test sample only included a single test for `Add`.

At this point, you should be able to write a second test for `Subtract`, re-run the tests (with coverage), re-generate the report, and see the updated code coverage results.
