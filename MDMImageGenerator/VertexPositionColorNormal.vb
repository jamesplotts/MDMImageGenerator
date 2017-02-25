Imports Microsoft.Xna.Framework.Graphics
Imports Microsoft.Xna.Framework

' Thanks to Riemer's XNA Tutorials for this format.  http://www.riemers.net/eng/Tutorials/XNA/Csharp/Series1/Lighting_basics.php

Public Structure VertexPositionColorNormal

    Public Position As Vector3
    Public Color As Color
    Public Normal As Vector3

    Public Sub New(ByVal vPos As Vector3, ByVal vCol As Color, ByVal vNor As Vector3)
        Position = vPos
        Color = vCol
        Normal = vNor
    End Sub

    Public Shared ReadOnly Property VertexDeclaration() As VertexDeclaration
        Get
            Return New VertexDeclaration({ _
                New VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0), _
                New VertexElement(12, VertexElementFormat.Color, VertexElementUsage.Color, 0), _
                New VertexElement(16, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0)})
        End Get
    End Property

End Structure
