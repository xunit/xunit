---
layout: default
title: Getting Test Results in Azure DevOps Pipelines
breadcrumb: Documentation
redirect_from:
  - /docs/getting-test-results-in-vsts
---

# Getting Test Results in Azure DevOps Pipelines

## Referencing the Visual Studio test runner

* Using NuGet Package Manager (or Package Manager Console), add `xunit.runner.visualstudio` to at least one of your test projects.
`install-package xunit.runner.visualstudio`

## Configure Azure DevOps

* Add a build step of type `Visual Studio Test`.

These instructions are written for `Version 2.*` of the `Visual Studio Test` task.

### Locate test assemblies

* Under `Test selection/Test files`, point to your generated test assemblies, for example: 
```
**\bin\$(BuildConfiguration)\**\*test*.dll
!**\obj\**
!**\xunit.runner.visualstudio.testadapter.dll
!**\xunit.runner.visualstudio.dotnetcore.testadapter.dll
```
(The third and fourth line excludes the xUnit test adapters from being considered a test assembly.)
Note: it is important to make sure the file matching pattern is valid.  `testVerAssemblyVer2` must be expressed with newlines. [vsts-doc issue 1580](https://github.com/MicrosoftDocs/vsts-docs/issues/1580)

### Enable Code Coverage (optional)

* Select the `Execution Options/Code Coverage Enabled` checkbox.
* Add `/InIsolation` under `Execution options/Other console options`.
(This will suppress a warning otherwise generated.)

## Viewing the results

After a successful build, the test results are available on the main view of the build. It should look something like this:

![](../images/getting-test-results-in-vsts/test-results.png)

## Comments

* You no longer need to manually configure the test adapter path, because NuGet restored test adapters are automatically searched for.

* Sometimes you get this error one or many times
`Warning: System.AppDomainUnloadedException: Attempted to access an unloaded AppDomain. This can happen if the test(s) started a thread but did not stop it. Make sure that all the threads started by the test(s) are stopped before completion.`
It is not related to Azure DevOps and does not affect the results. For more information, see [xUnit Issue 490](https://github.com/xunit/xunit/issues/490) and [MS Connect](https://connect.microsoft.com/VisualStudio/feedback/details/797525/unexplained-appdomainunloadedexception-when-running-a-unit-test-on-tfs-build-server).
