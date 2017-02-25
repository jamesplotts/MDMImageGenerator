Imports System.Windows.Forms

Imports Microsoft.Xna.Framework
Imports Microsoft.Xna.Framework.Graphics
Imports Microsoft.Xna.Framework.Input
Imports Microsoft.Xna.Framework.Content
Imports System.Threading
Imports System

Namespace OpenForge.Development
    ''' <summary>
    ''' This is the main type for your game.
    ''' </summary>
    Public Class MDMImageGenerator
        Inherits Game
        Private graphics As GraphicsDeviceManager
        Private spriteBatch As SpriteBatch
        Private camTarget As Vector3
        Private camPosition As Vector3
        Private orbit As Boolean
        Private viewMatrix As Matrix
        Private projectionMatrix As Matrix
        Private worldMatrix As Matrix
        Private triangleVertices() As VertexPositionColor
        Private BasicEffect As BasicEffect
        Private vertexBuffer As VertexBuffer



        Public Sub New()
            graphics = New GraphicsDeviceManager(Me)
            Content.RootDirectory = "Content"
        End Sub

        ''' <summary>
        ''' Allows the game to perform any initialization it needs to before starting to run.
        ''' This is where it can query for any required services and load any non-graphic
        ''' related content.  Calling base.Initialize will enumerate through any components
        ''' and initialize them as well.
        ''' </summary>
        Protected Overrides Sub Initialize()
            ' TODO: Add your initialization logic here

            MyBase.Initialize()

            camTarget = New Vector3(0.0F, 0.0F, 0.0F)
            camPosition = New Vector3(0.0F, -1000.0F, -1000.0F)

            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45.0F), GraphicsDevice.DisplayMode.AspectRatio, 1.0F, 10000.0F)

            viewMatrix = Matrix.CreateLookAt(camPosition, camTarget, New Vector3(0.0F, 1.0F, 0.0F)) ' Y up
            worldMatrix = Matrix.CreateWorld(camTarget, Vector3.Forward, Vector3.Up)

            ' BasicEffect
            BasicEffect = New BasicEffect(GraphicsDevice)
            BasicEffect.Alpha = 1.0F

            ' Want to see the colors of the vertices, this needs to be on
            BasicEffect.VertexColorEnabled = True

            ' Lighting requires normal information which VertexPositionColor does not have
            ' If you want to use lighting and VPC you need to create a custom def
            BasicEffect.LightingEnabled = False

            ' Geometry  - a simple triangle about the origin



            'vertexBuffer = New VertexBuffer(GraphicsDevice, GetType(VertexPositionColor), triangleVertices.Length, BufferUsage.WriteOnly)
            'vertexBuffer.SetData(Of VertexPositionColor)(triangleVertices)



        End Sub

        ''' <summary>
        ''' LoadContent will be called once per game and is the place to load
        ''' all of your content.
        ''' </summary>
        Protected Overrides Sub LoadContent()
            ' Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = New SpriteBatch(GraphicsDevice)

            ' TODO: use this.Content to load your game content here
        End Sub

        ''' <summary>
        ''' UnloadContent will be called once per game and is the place to unload
        ''' game-specific content.
        ''' </summary>
        Protected Overrides Sub UnloadContent()
            ' TODO: Unload any non ContentManager content here
        End Sub


        Dim MoveIncrement As Single = 10.0F
        ''' <summary>
        ''' Allows the game to run logic such as updating the world,
        ''' checking for collisions, gathering input, and playing audio.
        ''' </summary>
        ''' <param name="gameTime">Provides a snapshot of timing values.</param>
        Protected Overrides Sub Update(gameTime As GameTime)
            'If GamePad.GetState(PlayerIndex.One).Buttons.Back = ButtonState.Pressed OrElse Keyboard.GetState().IsKeyDown(Keys.Escape) Then
            '	[Exit]()
            'End If

            ' TODO: Add your update logic here
            If (GamePad.GetState(PlayerIndex.One).Buttons.Back = Microsoft.Xna.Framework.Input.ButtonState.Pressed Or Keyboard.GetState().IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Escape)) Then End

            If (Keyboard.GetState().IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Left)) Then

                camPosition.X -= MoveIncrement
                camTarget.X -= MoveIncrement
            End If
            If (Keyboard.GetState().IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Right)) Then

                camPosition.X += MoveIncrement
                camTarget.X += MoveIncrement
            End If
            If (Keyboard.GetState().IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Up)) Then

                camPosition.Y -= MoveIncrement
                camTarget.Y -= MoveIncrement
            End If
            If (Keyboard.GetState().IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Down)) Then

                camPosition.Y += MoveIncrement
                camTarget.Y += MoveIncrement
            End If
            If (Keyboard.GetState().IsKeyDown(Microsoft.Xna.Framework.Input.Keys.OemPlus)) Then camPosition.Z += MoveIncrement

            If (Keyboard.GetState().IsKeyDown(Microsoft.Xna.Framework.Input.Keys.OemMinus)) Then camPosition.Z -= MoveIncrement

            If (Keyboard.GetState().IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Space)) Then orbit = Not orbit

            If (Keyboard.GetState().IsKeyDown(Microsoft.Xna.Framework.Input.Keys.L)) Then
                If loadthreadrunning = False Then
                    loadthreadrunning = True
                    Dim thread As New Thread(AddressOf BackgroundLoader)
                    thread.SetApartmentState(ApartmentState.STA)
                    thread.Start()
                End If

            End If

            If (orbit) Then

                Dim rotationMatrix As Matrix = Matrix.CreateRotationY(MathHelper.ToRadians(5.0F))
                camPosition = Vector3.Transform(camPosition, rotationMatrix)
            End If
            viewMatrix = Matrix.CreateLookAt(camPosition, camTarget, Vector3.Up)
            MyBase.Update(gameTime)
        End Sub

        Dim NumFacets As Int32 = 0
        Dim verticesloaded As Boolean = False
        Dim vertices() As VertexPositionColorNormal
        Dim loadthreadrunning As Boolean = False
        <STAThreadAttribute> _
        Sub BackgroundLoader()
            Dim fd As New OpenFileDialog
            Dim dlgres As DialogResult
            fd.ShowReadOnly = True
            fd.Title = "Open *.STL File"
            fd.Multiselect = False
            fd.Filter = "STL Files (*.stl)|*.stl"
            verticesloaded = False
            dlgres = fd.ShowDialog()
            If dlgres = DialogResult.OK Then
                Dim stl As STLDefinition.STLObject
                stl = STLDefinition.LoadSTL(fd.OpenFile())
                NumFacets = CInt(stl.STLHeader.nfacets)
                Dim vn As Vector3
                ReDim vertices(NumFacets * 3)
                With stl
                    For i As Int32 = 0 To NumFacets - 1
                        With .Facets(i)
                            With .normal
                                vn = New Vector3(.x, .y, .z)
                            End With
                            With .v1
                                vertices(i * 3) = New VertexPositionColorNormal(New Vector3(.x, .y, .z), Color.Gray, vn)
                                vertices(i * 3).Normal.Normalize()
                            End With
                        End With
                        With .Facets(i)
                            With .normal
                                vn = New Vector3(.x, .y, .z)
                            End With
                            With .v2
                                vertices(i * 3 + 1) = New VertexPositionColorNormal(New Vector3(.x, .y, .z), Color.LightGray, vn)
                                vertices(i * 3 + 1).Normal.Normalize()
                            End With
                        End With
                        With .Facets(i)
                            With .normal
                                vn = New Vector3(.x, .y, .z)
                            End With
                            With .v3
                                vertices(i * 3 + 2) = New VertexPositionColorNormal(New Vector3(.x, .y, .z), Color.DarkGray, vn)
                                vertices(i * 3 + 2).Normal.Normalize()
                            End With

                        End With
                    Next
                End With
                camPosition.X = 2 * CalcMax(stl.STLHeader.xmax, stl.STLHeader.xmin)
                camPosition.Y = 0.0F
                camPosition.Z = 4 * CalcMax(stl.STLHeader.zmax, stl.STLHeader.zmin)


                'camPosition = New Vector3(0.0F, -1000.0F, -1000.0F)
                viewMatrix = Matrix.CreateLookAt(camPosition, camTarget, Vector3.Up) ' Y up

                verticesloaded = True
            End If


            loadthreadrunning = False
        End Sub


        Private Function CalcMax(ByVal sval As Single, ByVal tval As Single) As Single
            Dim s As Single = Math.Abs(sval)
            Dim t As Single = Math.Abs(tval)
            Dim u As Single = Math.Sign(sval)
            If s < t Then
                s = t
                u = Math.Sign(tval)
            End If
            Return s * u
        End Function

        ''' <summary>
        ''' This is called when the game should draw itself.
        ''' </summary>
        ''' <param name="gameTime">Provides a snapshot of timing values.</param>
        Protected Overrides Sub Draw(gameTime As GameTime)
            GraphicsDevice.Clear(Color.CornflowerBlue)

            ' TODO: Add your drawing code here
            BasicEffect.Projection = projectionMatrix
            BasicEffect.View = viewMatrix
            BasicEffect.World = worldMatrix

            GraphicsDevice.Clear(Color.CornflowerBlue)
            GraphicsDevice.SetVertexBuffer(vertexBuffer)

            ' Turn off culling so we see both sides of our rendered triangle
            Dim RasterizerState As New RasterizerState()
            RasterizerState.CullMode = CullMode.None
            GraphicsDevice.RasterizerState = RasterizerState

            For Each pass As EffectPass In BasicEffect.CurrentTechnique.Passes

                pass.Apply()
                '    GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, 3)
                If verticesloaded = True Then GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, vertices, 0, NumFacets, VertexPositionColorNormal.VertexDeclaration)

            Next



            MyBase.Draw(gameTime)
        End Sub
    End Class
End Namespace