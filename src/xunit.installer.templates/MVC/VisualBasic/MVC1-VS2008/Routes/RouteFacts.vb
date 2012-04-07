Imports System.Web
Imports System.Web.Routing
Imports Xunit

Public Class RouteFacts
    <Fact()> Public Sub RouteWithControllerNoActionNoId()
        ' Arrange
        Dim context = New StubContext("~/controller1")
        Dim routes = New RouteCollection()
        MvcApplication.RegisterRoutes(routes)

        ' Act
        Dim routeData = routes.GetRouteData(context)

        ' Assert
        Assert.NotNull(routeData)
        Assert.Equal("controller1", routeData.Values("controller"))
        Assert.Equal("Index", routeData.Values("action"))
        Assert.Equal("", routeData.Values("id"))
    End Sub

    <Fact()> Public Sub RouteWithControllerWithActionNoId()
        ' Arrange
        Dim context = New StubContext("~/controller1/action2")
        Dim routes = New RouteCollection()
        MvcApplication.RegisterRoutes(routes)

        ' Act
        Dim routeData = routes.GetRouteData(context)

        ' Assert
        Assert.NotNull(routeData)
        Assert.Equal("controller1", routeData.Values("controller"))
        Assert.Equal("action2", routeData.Values("action"))
        Assert.Equal("", routeData.Values("id"))
    End Sub

    <Fact()> Public Sub RouteWithControllerWithActionWithId()
        ' Arrange
        Dim context = New StubContext("~/controller1/action2/id3")
        Dim routes = New RouteCollection()
        MvcApplication.RegisterRoutes(routes)

        ' Act
        Dim routeData = routes.GetRouteData(context)

        ' Assert
        Assert.NotNull(routeData)
        Assert.Equal("controller1", routeData.Values("controller"))
        Assert.Equal("action2", routeData.Values("action"))
        Assert.Equal("id3", routeData.Values("id"))
    End Sub

    <Fact()> Public Sub RouteWithTooManySegments()
        ' Arrange
        Dim context = New StubContext("~/a/b/c/d")
        Dim routes = New RouteCollection()
        MvcApplication.RegisterRoutes(routes)

        ' Act
        Dim routeData = routes.GetRouteData(context)

        ' Assert
        Assert.Null(routeData)
    End Sub

    <Fact()> Public Sub RouteForEmbeddedResource()
        ' Arrange
        Dim context = New StubContext("~/foo.axd/bar/baz/biff")
        Dim routes = New RouteCollection()
        MvcApplication.RegisterRoutes(routes)

        ' Act
        Dim routeData = routes.GetRouteData(context)

        ' Assert
        Assert.NotNull(routeData)
        Assert.IsAssignableFrom(Of StopRoutingHandler)(routeData.RouteHandler)
    End Sub
End Class