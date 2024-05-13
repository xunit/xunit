---
layout: default
title: Getting Test Results in TeamCity
breadcrumb: Documentation
---
# Getting Test Results in TeamCity

xUnit.net 1.1 added support for [JetBrains' TeamCity](https://www.jetbrains.com/teamcity/). This support is automatically enabled, and requires no end-user configuration.

You can either use a TeamCity plugin, or use an external build runner (like MSBuild) when building your project in TeamCity.

## Using the plugin

Download and install the [plugin](https://github.com/carlpett/xUnit-TeamCity) as per [Teamcity documentation](https://confluence.jetbrains.com/display/TCD9/Installing+Additional+Plugins).

Select the xUnit runner for the build step, then select the version of xUnit your tests are written in, and finally a pattern to match your test binaries. Wildcards are supported, such as `**/*.Tests.dll`.

## Using an external runner

Select the appropriate runner to add calls to the MSBuild task, the Console runner, or the .NET Core runner. You cannot use TeamCity's SLN builder, because your SLN file does not have any references to running xUnit.net.

The runners in xUnit.net detect TeamCity is running through environment variables (specifically, it looks for `TEAMCITY_PROJECT_NAME`). If for some reason your build environment does not pass the TeamCity environment variables through to the runner, you can force TeamCity mode manually; on the console or DNX runner, add the `-teamcity` switch; on the MSBuild runner, add the property `Reporter="teamcity"` to your `<xunit>` task.

## Using `dotnet test`

If you are running your tests with `dotnet test`, by default the messages that TeamCity consumes will be hidden by VSTest, because of the default verbosity level.

In order to ensure that the TeamCity control messages show up, add `--logger "console;verbosity=detailed"` to your `dotnet test` command line.

You can test this locally by doing the following:

- Set environment variable `TEAMCITY_PROJECT_NAME` to any value
- Run your tests with `dotnet test <projectname> --logger "console;verbosity=detailed"`

You should see output lines that start with `##teamcity`, similar to:

```
[xUnit.net 00:00:00.10] ##teamcity[flowStarted timestamp='2023-06-18T00:02:48.362+0000' flowId='my.tests.dll']
```
