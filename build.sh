#!/usr/bin/env bash

tmpfile=$(mktemp /tmp/xunit-build.XXXXXX)
exec 3>"${tmpfile}"
rm "${tmpfile}"

if test -t 1; then
	if [ -n `which tput` ]; then
        COLOR_RED="$(tput setaf 1)"
        COLOR_WHITE="$(tput setaf 7)"
        COLOR_BOLD="$(tput bold)"
        COLOR_RESET="$(tput sgr0)"
    fi
fi

build_step() {
	local MSG="$1"
	echo -e "${COLOR_WHITE}${COLOR_BOLD}==> ${MSG}${COLOR_RESET}"
}

fatal() {
	local MSG="$1"
	echo -e "${COLOR_RED}${COLOR_BOLD}Error:${COLOR_RESET} ${MSG}"
	echo
	cat "${tmpfile}"
	exit 1
}

require() {
	local BINARY="$1"
	local NAME="$2"
	[[ -n `which ${BINARY}` ]] || fatal "Could not find '${BINARY}' on the path. Please install ${NAME}."
}

require mono 'Mono'
require dotnet '.NET CLI'

build_step "Restoring NuGet packages"

	dotnet restore >"${tmpfile}" 2>&1 || fatal "Package restore has failed."

build_step "Compiling binaries"

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
		test/test.xunit.console >"${tmpfile}" 2>&1 \
			|| fatal "The build has failed."

build_step "Running unit tests\n"

	# TODO: execution & runner.utility tests fail on Mono
	mono src/xunit.console/bin/Release/net45/**/xunit.console.exe \
		test/test.xunit.assert/bin/Release/net45/test.xunit.assert.dll \
		test/test.xunit.console/bin/Release/net45/test.xunit.console.dll \
		test/test.xunit.runner.reporters/bin/Release/net45/test.xunit.runner.reporters.dll \
		-noappdomain -parallel none \
			|| fatal "Unit tests have failed."
