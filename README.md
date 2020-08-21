
# Excubo.Generators.Grouping

[![Nuget](https://img.shields.io/nuget/v/Excubo.Generators.Grouping)](https://www.nuget.org/packages/Excubo.Generators.Grouping/)
[![Nuget](https://img.shields.io/nuget/dt/Excubo.Generators.Grouping)](https://www.nuget.org/packages/Excubo.Generators.Grouping/)
[![GitHub](https://img.shields.io/github/license/excubo-ag/Generators.Grouping)](https://github.com/excubo-ag/Generators.Grouping)

Some APIs have a lot of methods.
When they all reside in the same object, finding the right API call can be a challenge to the users.
But replacing the structure of the API from a monolithic API to an API with intuitively named and smaller groups would break existing code.
The solution is to offer both, without having to write the grouping yourself.

## How to use

### 1. Install the nuget package Excubo.Generators.Grouping

Excubo.Generators.Grouping is distributed [via nuget.org](https://www.nuget.org/packages/Excubo.Generators.Grouping/).
[![Nuget](https://img.shields.io/nuget/v/Excubo.Generators.Grouping)](https://www.nuget.org/packages/Excubo.Generators.Grouping/)

#### Package Manager:
```ps
Install-Package Excubo.Generators.Grouping -Version 1.0.0
```

#### .NET Cli:
```cmd
dotnet add package Excubo.Generators.Grouping --version 1.0.0
```

#### Package Reference
```xml
<PackageReference Include="Excubo.Generators.Grouping" Version="1.0.0" />
```

## Example

Consider the API for drawing (example inspired by the HTML canvas API):

```cs
public class API
{
    // sort order alphabetical, as it would appear in most IDEs
    public void BezierCurveTo(...);
    public void DrawText(...);
    public void Fill();
    public void FillEllipse(...);
    public void FillPolygon(...);
    public void FillRectangle(...);
    public void FillTriangle(...);
    public void LineTo(...);
    public void MoveTo(...);
    public void SaveState(...);
    public void SetFont(...);
    public void SetTextAlign(...);
    public void Stroke();
    public void StrokeEllipse(...);
    public void StrokePolygon(...);
    public void StrokeRectangle(...);
    public void StrokeTriangle(...);
    public void RestoreState(...);
}
```

Usage could look like

```cs
api.SaveState();
api.SetFont("Comic Sans");
api.DrawText("This API is convoluted");
api.RestoreState();
api.StrokeRectangle(rect);
api.MoveTo(origin);
api.LineTo(chaos);
api.Stroke();
```


In this API, there are multiple concepts:
- Drawing shapes (filled or just with a stroke)
- State
- Text
- Paths

To find methods easier, we can create groups of methods, e.g.:

- Path methods:

```cs
public class API
{
    public struct _Paths
    {
        public void BezierCurveTo(...);
        public void Fill();
        public void LineTo(...);
        public void MoveTo(...);
        public void Stroke();
    }
```

- State management:

```cs
public class API
{
    public struct _State
    {
        public void Save(...);
        public void Restore(...);
    }
}
```

etc.

This library facilitates writing such groups, without interfering with the original API:

```cs
public partial class API
{
    // the groups we want to offer: Paths, State, Text, Shapes
    public partial struct _Paths {}
    public partial struct _Shapes {}
    public partial struct _State {}
    public partial struct _Text {}
    
    // Annotated methods which will be replicated in the groups
    [Group(typeof(_Paths))] public void BezierCurveTo(...);
    [Group(typeof(_Text), "Draw")] public void DrawText(...);
    [Group(typeof(_Paths))] public void Fill();
    [Group(typeof(_Shapes))] public void FillEllipse(...);
    [Group(typeof(_Shapes))] public void FillPolygon(...);
    [Group(typeof(_Shapes))] public void FillRectangle(...);
    [Group(typeof(_Shapes))] public void FillTriangle(...);
    [Group(typeof(_Paths))] public void LineTo(...);
    [Group(typeof(_Paths))] public void MoveTo(...);
    [Group(typeof(_State), "Save")] public void SaveState(...);
    [Group(typeof(_Text))] public void SetFont(...);
    [Group(typeof(_Text))] public void SetTextAlign(...);
    [Group(typeof(_Paths))] public void Stroke();
    [Group(typeof(_Shapes))] public void StrokeEllipse(...);
    [Group(typeof(_Shapes))] public void StrokePolygon(...);
    [Group(typeof(_Shapes))] public void StrokeRectangle(...);
    [Group(typeof(_Shapes))] public void StrokeTriangle(...);
    [Group(typeof(_State), "Restore")] public void RestoreState(...);
}
```

The generated code then enables usage like this:

```cs
api.State.Save();
api.Text.SetFont("Helvetica Neue");
api.Text.Draw("This API is intuitive");
api.State.Restore();
api.Shapes.StrokeRectangle(rect);
api.Paths.MoveTo(origin);
api.Paths.LineTo(order);
api.Paths.Stroke();
```

## Nested groups

Groups can even be nested:

```cs
public partial class API
{
    // the groups we want to offer: Paths, State, Text, Shapes
    public partial struct _Shapes 
    {
        public partial struct _Ellipse {}
        public partial struct _Rectangle {}
    }
    
    // Annotated methods which will be replicated in the groups
    [Group(typeof(_Shapes._Ellipse), "Fill")] public void FillEllipse(...);
    [Group(typeof(_Shapes._Rectangle), "Fill")] public void FillRectangle(...);
    [Group(typeof(_Shapes._Ellipse), "Stroke")] public void StrokeEllipse(...);
    [Group(typeof(_Shapes._Rectangle), "Stroke")] public void StrokeRectangle(...);
}
```

which would be used as 

```cs
api.Shapes.Ellipse.Fill();
api.Shapes.Rectangle.Stroke();
```

## Multiple groups

A method can be in multiple different groups:

```cs
public partial class API
{
    // the groups we want to offer: Shapes, Fill
    public partial struct _Shapes {}
    public partial struct _Fill {}
    
    // Annotated methods which will be replicated in the groups
    [Group(typeof(_Shapes)), Group(typeof(_Fill), "Ellipse")] public void FillEllipse(...);
    [Group(typeof(_Shapes)), Group(typeof(_Fill), "Rectangle")] public void FillRectangle(...);
}
```


