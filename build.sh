#!/usr/bin/env bash

if ! [ -x "$(command -v mono)" ]; then
	echo >&2 "Could not find 'mono' on the path. Please install Mono."
	exit 1
fi

if ! [ -x "$(command -v dotnet)" ]; then
	echo >&2 "Could not find 'dotnet' on the path. Please install .NET CLI."
	exit 1
fi

echo ""
echo "Restoring packages..."
echo ""

dotnet restore

if [ $? -ne 0 ]; then
	echo >&2 "Package restore has failed."
	exit 1
fi

echo ""
echo "Building..."
echo ""

dotnet build -c Release \
	src/xunit.assert \
	src/xunit.core \
	src/xunit.execution \
	src/xunit.runner.utility \
	src/xunit.runner.reporters \
	src/xunit.console \
	test/test.xunit.assert \
	test/test.xunit.execution \
	test/test.xunit.runner.utility \
	test/test.xunit.runner.reporters \
	test/test.xunit.console

if [ $? -ne 0 ]; then
	echo >&2 ""
	echo >&2 "The build has failed."
	exit 1
fi

#echo ""
#echo "Running unit tests..."
#echo ""

#mono src/xunit.console/bin/Release/net45/*/xunit.console.exe \
#	test/test.xunit.assert/bin/Release/net45/test.xunit.assert.dll \
#	test/test.xunit.execution/bin/Release/net45/test.xunit.execution.dll \
#	test/test.xunit.runner.utility/bin/Release/net45/test.xunit.runner.utility.dll \
#	test/test.xunit.runner.reporters/bin/Release/net45/test.xunit.runner.reporters.dll \
#	test/test.xunit.console/bin/Release/net45/test.xunit.console.dll \
#	-parallel all

#if [ $? -ne 0 ]; then
#	echo >&2 ""
#	echo >&2 "Unit tests have failed."
#	exit 1
#fi
