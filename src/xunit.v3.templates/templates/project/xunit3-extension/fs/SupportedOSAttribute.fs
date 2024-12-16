namespace ExtensionProject

open System
open System.Reflection
open System.Runtime.InteropServices
open System.Threading.Tasks
open Xunit.v3

(*
The intended usage of this sample attribute is as an extra attribute on a unit test method. For example:

    module TestClass

    open Xunit

    [<Fact>]
    [<SupportedOS(SupportedOS.Linux, SupportedOS.macOS)>]
    let TestMethod() =
        Assert.True(false)

TestMethod will only run when executed on Linux or macOS; it will not run on Windows or FreeBSD, and will be
dynamically skipped instead with a message about the current OS not being supported.
*)

type SupportedOSAttribute([<ParamArray>]supportedOSes: SupportedOS[]) =
    inherit BeforeAfterTestAttribute()

    static let osMappings = Map [
        SupportedOS.FreeBSD, OSPlatform.Create("FreeBSD")
        SupportedOS.Linux, OSPlatform.Linux
        SupportedOS.macOS, OSPlatform.OSX
        SupportedOS.Windows, OSPlatform.Windows
    ]

    override _.Before(_: MethodInfo, _: IXunitTest) =
        let executionEnvironmentMatches (x: SupportedOS) =
            match osMappings |> Map.tryFind x with
            | None -> invalidArg (nameof supportedOSes) $"Supported OS value '{x}' is not a known OS"
            | Some target -> RuntimeInformation.IsOSPlatform target

        let canRunHere = supportedOSes |> Seq.exists executionEnvironmentMatches
        if not canRunHere then
           // We use the dynamic skip exception message pattern to turn this into a skipped test
           // when it's not running on one of the targeted OSes
           failwith $"$XunitDynamicSkip$This test is not supported on {RuntimeInformation.OSDescription}"
