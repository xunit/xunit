Imports System.Web.Mvc
Imports Xunit

Public Class HomeControllerFacts
    Public Class Index
        <Fact()> Public Sub ReturnsViewResultWithDefaultViewName()
            ' Arrange
            Dim controller = New HomeController()

            ' Act
            Dim result = controller.Index()

            ' Assert
            Dim viewResult = Assert.IsType(Of ViewResult)(result)
            Assert.Empty(viewResult.ViewName)
        End Sub

        <Fact()> Public Sub SetsViewDataWithNoModel()
            ' Arrange
            Dim controller = New HomeController()

            ' Act
            Dim result = controller.Index()

            ' Assert
            Dim viewResult = Assert.IsType(Of ViewResult)(result)
            Assert.Equal("Welcome to ASP.NET MVC!", viewResult.ViewData("Message"))
            Assert.Null(viewResult.ViewData.Model)
        End Sub
    End Class

    Public Class About
        <Fact()> Public Sub ReturnsViewResultWithDefaultViewName()
            ' Arrange
            Dim controller = New HomeController()

            ' Act
            Dim result = controller.About()

            ' Assert
            Dim viewResult = Assert.IsType(Of ViewResult)(result)
            Assert.Empty(viewResult.ViewName)
        End Sub

        <Fact()> Public Sub SetsViewDataWithNoModel()
            ' Arrange
            Dim controller = New HomeController()

            ' Act
            Dim result = controller.Index()

            ' Assert
            Dim viewResult = Assert.IsType(Of ViewResult)(result)
            Assert.Null(viewResult.ViewData.Model)
        End Sub
    End Class
End Class
