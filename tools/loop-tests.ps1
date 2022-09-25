#!/usr/bin/env pwsh
param (
	[Parameter(Mandatory, Position = 0)]
	[string]
	$TestProject,

	[Parameter(ValueFromRemainingArguments = $true, Position = 1)]
	[string[]]
	$TestRunArguments,

	[Parameter()][int]
	$Count = 500
)

for ($i = 1; $i -le $Count; $i++) {
	Write-Host "`e[1m*** Loop $i (Mono) ***`e[0m"
	& mono src/$($TestProject)/bin/Release/net472/$($TestProject).exe $TestRunArguments
	if ($LASTEXITCODE -ne 0) {
		break
	}

	Write-Host "`e[1m*** Loop $i (.NET Core) ***`e[0m"
	& dotnet exec src/$($TestProject)/bin/Release/net6.0/$($TestProject).dll $TestRunArguments
	if ($LASTEXITCODE -ne 0) {
		break
	}
}
