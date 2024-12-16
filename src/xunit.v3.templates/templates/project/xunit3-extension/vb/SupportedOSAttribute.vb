Imports System.Reflection
Imports System.Runtime.InteropServices
Imports Xunit.v3

' The intended usage of this sample attribute is as an extra attribute on a unit test method. For example:
'
'     Public Class TestClass
'
'         <Fact>
'         <SupportedOS(SupportedOS.Linux, SupportedOS.macOS)>
'         Public Sub TestMethod()
'         End Sub
'
'     End Class
'
' TestMethod will only run when executed on Linux or macOS; it will not run on Windows or FreeBSD, and will be
' dynamically skipped instead with a message about the current OS not being supported.

Public Class SupportedOSAttribute
    Inherits BeforeAfterTestAttribute

    Private ReadOnly supportedOSes() As SupportedOS

    Public Sub New(ParamArray supportedOSes() As SupportedOS)
        Me.supportedOSes = supportedOSes
    End Sub

    Public Overrides Sub Before(methodUnderTest As MethodInfo, test As IXunitTest)
        Static osMappings As New Dictionary(Of SupportedOS, OSPlatform) From
        {
            {SupportedOS.FreeBSD, OSPlatform.Create("FreeBSD")},
            {SupportedOS.Linux, OSPlatform.Linux},
            {SupportedOS.macOS, OSPlatform.OSX},
            {SupportedOS.Windows, OSPlatform.Windows}
        }

        Dim match = False

        For Each supportedOS In supportedOSes
            Dim osPlatform As OSPlatform

            If Not osMappings.TryGetValue(supportedOS, osPlatform) Then
                Throw New ArgumentException($"Supported OS value '{supportedOS}' is not a known OS", NameOf(supportedOSes))
            End If

            If RuntimeInformation.IsOSPlatform(osPlatform) Then
                match = True
                Exit For
            End If
        Next

        ' We use the dynamic skip exception message pattern to turn this into a skipped test
        ' when it's not running on one of the targeted OSes
        If Not match Then
            Throw New Exception($"$XunitDynamicSkip$This test is not supported on {RuntimeInformation.OSDescription}")
        End If
    End Sub
End Class
