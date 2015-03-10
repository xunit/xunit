#!/bin/bash

if ! [ -x "$(command -v mono)" ]; then
  echo >&2 "Could not find 'mono' on the path."
  exit 1
fi

if ! [ -x "$(command -v curl)" ]; then
  echo >&2 "Could not find 'curl' on the path."
  exit 1
fi

if ! [ -d .nuget ]; then
  mkdir .nuget
fi

if ! [ -x .nuget/nuget.exe ]; then
  echo ""
  echo "Downloading nuget.exe..."
  echo ""

  curl https://api.nuget.org/downloads/nuget.exe -o .nuget/nuget.exe -L
  if [ $? -ne 0 ]; then
    echo >&2 ""
    echo >&2 "The download of nuget.exe has failed."
    exit 1
  fi

  chmod 755 .nuget/nuget.exe
fi

echo ""
echo "Restoring NuGet packages..."
echo ""

mono .nuget/nuget.exe restore xunit.xbuild.sln
if [ $? -ne 0 ]; then
  echo >&2 "NuGet package restore has failed."
  exit 1
fi

echo ""
echo "Building..."
echo ""

xbuild xunit.xbuild.sln /property:Configuration=Release
if [ $? -ne 0 ]; then
  echo >&2 ""
  echo >&2 "The build has failed."
  exit 1
fi

echo ""
echo "Running xUnit v1 tests..."
echo ""

mono src/xunit.console/bin/Release/xunit.console.exe test/test.xunit1/bin/Release/test.xunit1.dll
if [ $? -ne 0 ]; then
  echo >&2 ""
  echo >&2 "The xUnit v1 tests have failed."
  exit 1
fi

echo ""
echo "Running xUnit v2 tests..."
echo ""

mono src/xunit.console/bin/Release/xunit.console.exe test/test.xunit.assert/bin/Release/test.xunit.assert.dll test/test.xunit.console/bin/Release/test.xunit.console.dll test/test.xunit.execution/bin/Release/test.xunit.execution.dll test/test.xunit.runner.tdnet/bin/Release/test.xunit.runner.tdnet.dll test/test.xunit.runner.utility/bin/Release/test.xunit.runner.utility.dll -parallel all
if [ $? -ne 0 ]; then
  echo >&2 ""
  echo >&2 "The xUnit v2 tests have failed."
  exit 1
fi
