---
layout: default
title: "Getting Started: Xamarin with the Devices Runner"
breadcrumb: Documentation
redirect_from:
  - /doc/getting-started-devices
---

# Getting Started with xUnit.net
## Xamarin Android and iOS with the Devices Runner

If you want to run your xUnit tests on a supported device, then the device runners are the way to do it. Running unit tests on a device is the surest way to ensure correct behavior, after things like Ahead of Time (AOT) compilation, .NET Native or other device-specific transformations happen. There's simply no substitute for running "on the metal."

## The general pattern

xUnit for Devices projects all follow a similar approach: create a new blank application to host the runner, add `xunit.runner.devices` and tell it which assemblies contain your unit tests. The unit tests may live directly within the application you created or in other referenced libraries, including PCLs.

_More content will be coming soon._