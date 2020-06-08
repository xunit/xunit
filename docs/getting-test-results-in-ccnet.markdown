---
layout: default
title: Getting Test Results in CruiseControl.NET
breadcrumb: Documentation
---
# Getting Test Results in CruiseControl.NET

If you're using CruiseControl.NET for continuous integration services, you can see the unit test results for xUnit.net on the build summary page.

## To install support for CCnet
* Configure the xUnit.net MSBuild task to output with the `Xml=` attribute, as shown below:

{% highlight xml %}
<Project DefaultTargets="Test" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">

  <UsingTask
    AssemblyFile="xunit.runner.msbuild.dll"
    TaskName="Xunit.Runner.MSBuild.xunit"/>

  <Target Name="Test">
    <xunit
        Assemblies="test.xunit\bin\Debug\test.xunit.dll"
        Xml="test.xunit.dll.xml"/>
  </Target>

</Project>
{% endhighlight %}

* Edit your CCnet build task (in `C:\Program Files\CruiseControl.NET\server\ccnet.config`) to merge your XML files into the CCnet build output, as shown below:

{% highlight xml %}
  <project>
    <name>MyProject</name>
    [...]
    <publishers>
      <merge>
        <files>
          <file>D:\Builds\MyProject\Test.*.xml</file>
        </files>
      </merge>
      <xmllogger />
      <statistics />
    </publishers>
    [...]
  </project>
{% endhighlight %}

* Download the [xUnitSummary.xsl](https://raw.githubusercontent.com/xunit/xunit/master/tools/xUnitSummary.xsl) file and save it to `C:\Program Files\CruiseControl.NET\webdashboard\xsl`
* Edit `C:\Program Files\CruiseControl.NET\webdashboard\dashboard.config` and add a line for the summary, as shown below (highlighted in yellow):

![](../images/getting-test-results-in-ccnet/dashboard-config.png){: .border }

* Reset the IIS server that's running CCnet
* Force a build and see the results! They should look similar to the following:

![](../images/getting-test-results-in-ccnet/sample-output.png){: .border }
