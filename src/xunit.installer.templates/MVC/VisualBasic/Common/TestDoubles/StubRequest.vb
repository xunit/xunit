Imports System.Web

Public Class StubRequest
    Inherits HttpRequestBase

    Private relativeUrl As String

    Public Sub New(ByVal relativeUrl As String)
        Me.relativeUrl = relativeUrl
    End Sub

    Public Overrides ReadOnly Property AppRelativeCurrentExecutionFilePath() As String
        Get
            Return relativeUrl
        End Get
    End Property

    Public Overrides ReadOnly Property PathInfo() As String
        Get
            Return ""
        End Get
    End Property
End Class