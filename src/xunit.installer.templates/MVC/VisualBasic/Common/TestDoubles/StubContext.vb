Imports System.Web

Public Class StubContext
    Inherits HttpContextBase

    Private stubRequest As StubRequest

    Public Sub New(ByVal relativeUrl As String)
        stubRequest = New StubRequest(relativeUrl)
    End Sub

    Public Overrides ReadOnly Property Request() As HttpRequestBase
        Get
            Return stubRequest
        End Get
    End Property
End Class