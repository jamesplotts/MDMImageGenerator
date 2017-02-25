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

        Private spriteFont As SpriteFont
        Private spriteFontPosition() As Vector2 = {New Vector2(5, 5), New Vector2(5, 20), New Vector2(5, 35), New Vector2(5, 50), New Vector2(5, 65)}
        Private text() As String = {"Press 'CTRL-O' to load an *.STL object", "Use Numpad 8/6/4/2 for NESW views.", "", "", ""}


        Private _total_frames As Int32 = 0
        Private _elapsed_time As Double = 0.0F
        Private _fps As Int32 = 0

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
            'worldMatrix = Matrix.CreateWorld(camTarget, Vector3.Forward, Vector3.Up)
            worldMatrix = Matrix.CreateRotationX(MathHelper.ToRadians(90.0F))

            ' BasicEffect
            BasicEffect = New BasicEffect(GraphicsDevice)
            BasicEffect.Alpha = 1.0F

            ' Want to see the colors of the vertices, this needs to be on
            BasicEffect.VertexColorEnabled = True

            ' Lighting requires normal information which VertexPositionColor does not have
            ' If you want to use lighting and VPC you need to create a custom def
            BasicEffect.EnableDefaultLighting()


            spriteBatch = New SpriteBatch(GraphicsDevice)

            spriteFont = Content.Load(Of SpriteFont)("SpriteFont1")

        End Sub

        Public Enum eDir
            North
            East
            South
            West
            Unspecified
        End Enum

        Private CurDir As eDir = eDir.North
        Private MoveIncrement As Single = 10.0F
        ''' <summary>
        ''' Allows the game to run logic such as updating the world,
        ''' checking for collisions, gathering input, and playing audio.
        ''' </summary>
        ''' <param name="gameTime">Provides a snapshot of timing values.</param>
        Protected Overrides Sub Update(gameTime As GameTime)
            ' FPS logic here
            ' update
            _elapsed_time += gameTime.ElapsedGameTime.TotalMilliseconds
            ' 1 second has passed
            If (_elapsed_time > 1000.0F) Then
                _fps = _total_frames
                _total_frames = 0
                _elapsed_time -= 1000.0F
            End If


            ' Keyboard Input Processing Here
            Dim state As KeyboardState = Keyboard.GetState()
            With state
                Select Case CurDir
                    Case eDir.North
                        If (.IsKeyDown(Input.Keys.NumPad6)) Then
                            Dim rotationMatrix As Matrix = Matrix.CreateRotationY(MathHelper.ToRadians(90.0F))
                            camPosition = Vector3.Transform(camPosition, rotationMatrix)
                            CurDir = eDir.East
                        End If
                        If (.IsKeyDown(Input.Keys.NumPad4)) Then
                            Dim rotationMatrix As Matrix = Matrix.CreateRotationY(MathHelper.ToRadians(-90.0F))
                            camPosition = Vector3.Transform(camPosition, rotationMatrix)
                            CurDir = eDir.West
                        End If
                        If (.IsKeyDown(Input.Keys.NumPad2)) Then
                            Dim rotationMatrix As Matrix = Matrix.CreateRotationY(MathHelper.ToRadians(180.0F))
                            camPosition = Vector3.Transform(camPosition, rotationMatrix)
                            CurDir = eDir.South
                        End If
                    Case eDir.East
                        If (.IsKeyDown(Input.Keys.NumPad4)) Then
                            Dim rotationMatrix As Matrix = Matrix.CreateRotationY(MathHelper.ToRadians(180.0F))
                            camPosition = Vector3.Transform(camPosition, rotationMatrix)
                            CurDir = eDir.West
                        End If
                        If (.IsKeyDown(Input.Keys.NumPad2)) Then
                            Dim rotationMatrix As Matrix = Matrix.CreateRotationY(MathHelper.ToRadians(90.0F))
                            camPosition = Vector3.Transform(camPosition, rotationMatrix)
                            CurDir = eDir.South
                        End If
                        If (.IsKeyDown(Input.Keys.NumPad8)) Then
                            Dim rotationMatrix As Matrix = Matrix.CreateRotationY(MathHelper.ToRadians(-90.0F))
                            camPosition = Vector3.Transform(camPosition, rotationMatrix)
                            CurDir = eDir.North
                        End If
                    Case eDir.South
                        If (.IsKeyDown(Input.Keys.NumPad8)) Then
                            Dim rotationMatrix As Matrix = Matrix.CreateRotationY(MathHelper.ToRadians(180.0F))
                            camPosition = Vector3.Transform(camPosition, rotationMatrix)
                            CurDir = eDir.North
                        End If
                        If (.IsKeyDown(Input.Keys.NumPad4)) Then
                            Dim rotationMatrix As Matrix = Matrix.CreateRotationY(MathHelper.ToRadians(90.0F))
                            camPosition = Vector3.Transform(camPosition, rotationMatrix)
                            CurDir = eDir.West
                        End If
                        If (.IsKeyDown(Input.Keys.NumPad6)) Then
                            Dim rotationMatrix As Matrix = Matrix.CreateRotationY(MathHelper.ToRadians(-90.0F))
                            camPosition = Vector3.Transform(camPosition, rotationMatrix)
                            CurDir = eDir.East
                        End If
                    Case eDir.West
                        If (.IsKeyDown(Input.Keys.NumPad8)) Then
                            Dim rotationMatrix As Matrix = Matrix.CreateRotationY(MathHelper.ToRadians(90.0F))
                            camPosition = Vector3.Transform(camPosition, rotationMatrix)
                            CurDir = eDir.North
                        End If
                        If (.IsKeyDown(Input.Keys.NumPad2)) Then
                            Dim rotationMatrix As Matrix = Matrix.CreateRotationY(MathHelper.ToRadians(-90.0F))
                            camPosition = Vector3.Transform(camPosition, rotationMatrix)
                            CurDir = eDir.South
                        End If
                        If (.IsKeyDown(Input.Keys.NumPad6)) Then
                            Dim rotationMatrix As Matrix = Matrix.CreateRotationY(MathHelper.ToRadians(180.0F))
                            camPosition = Vector3.Transform(camPosition, rotationMatrix)
                            CurDir = eDir.East
                        End If
                End Select
                If (.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Escape)) Then End

                'If (.IsKeyDown(Input.Keys.W)) Then camTarget.Y += MoveIncrement
                'If (.IsKeyDown(Input.Keys.S)) Then camTarget.Y -= MoveIncrement
                'If (.IsKeyDown(Input.Keys.A)) Then camTarget.X += MoveIncrement
                'If (.IsKeyDown(Input.Keys.D)) Then camTarget.X -= MoveIncrement
                'If (.IsKeyDown(Input.Keys.Q)) Then camTarget.Z += MoveIncrement
                'If (.IsKeyDown(Input.Keys.Z)) Then camTarget.Z -= MoveIncrement
                'If (.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Right)) Then camPosition.X -= MoveIncrement
                'If (.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Left)) Then camPosition.X += MoveIncrement
                'If (.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Down)) Then camPosition.Y -= MoveIncrement
                'If (.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Up)) Then camPosition.Y += MoveIncrement
                'If (.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.OemPlus)) Then camPosition.Z += MoveIncrement
                'If (.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.OemMinus)) Then camPosition.Z -= MoveIncrement
                'If (.IsKeyDown(Input.Keys.R)) Then
                '    Static bolRunOnce As Boolean
                '    If Not bolRunOnce Then
                '        Rotate(90, BasicEffect)
                '        bolRunOnce = True
                '    End If
                'End If

                If (.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Space)) Then orbit = Not orbit

                If (.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.O) AndAlso (.IsKeyDown(Input.Keys.LeftControl) OrElse .IsKeyDown(Input.Keys.RightControl))) Then
                    If loadthreadrunning = False Then
                        loadthreadrunning = True
                        Dim thread As New Thread(AddressOf BackgroundLoader)
                        thread.SetApartmentState(ApartmentState.STA)
                        thread.Start()
                    End If

                End If
            End With

            If (orbit) Then
                Dim rotationMatrix As Matrix = Matrix.CreateRotationY(MathHelper.ToRadians(90.0F))
                camPosition = Vector3.Transform(camPosition, rotationMatrix)
                CurDir = CType(CurDir + eDir.North, eDir)
                If CurDir > eDir.West Then CurDir = CType(CurDir - eDir.Unspecified, eDir)
                orbit = False
            End If
            Dim v3 As Vector3 = New Vector3(0.0F, 1.0F, 0.0F)
            If camPosition.Y < 0 Then v3 = New Vector3(0.0F, -1.0F, 0.0F)
            viewMatrix = Matrix.CreateLookAt(camPosition, camTarget, v3)
            MyBase.Update(gameTime)
        End Sub

        'Public Sub Rotate(ByVal angle As Single, ByVal effect As BasicEffect)
        '    Dim current_view As Matrix = effect.Projection
        '    current_view *= Matrix.CreateRotationZ(angle)
        '    effect.Projection = current_view
        'End Sub


        ''' <summary>
        ''' This is called when the game should draw itself.
        ''' </summary>
        ''' <param name="gameTime">Provides a snapshot of timing values.</param>
        Protected Overrides Sub Draw(gameTime As GameTime)
            _total_frames += 1
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


                If verticesloaded = True Then GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, vertices, 0, NumFacets, VertexPositionColorNormal.VertexDeclaration)

            Next

            spriteBatch.Begin(, , , DepthStencilState.Default)
            text(2) = "FPS=" + _fps.ToString
            With camPosition
                text(3) = "Cam Pos: X=" + .X.ToString + ", Y=" + .Y.ToString + ", Z=" + .Z.ToString
            End With
            With camTarget
                text(4) = "Cam Targ: X=" + .X.ToString + ", Y=" + .Y.ToString + ", Z=" + .Z.ToString
            End With
            For i As Int32 = 0 To 4
                spriteBatch.DrawString(spriteFont, text(i), spriteFontPosition(i), Color.Black)
            Next
            spriteBatch.End()


            MyBase.Draw(gameTime)
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
                                vertices(i * 3) = New VertexPositionColorNormal(New Vector3(.x, .y, .z), Color.DarkGray, vn)
                                vertices(i * 3).Normal.Normalize()
                            End With
                        End With
                        With .Facets(i)
                            With .normal
                                vn = New Vector3(.x, .y, .z)
                            End With
                            With .v2
                                vertices(i * 3 + 1) = New VertexPositionColorNormal(New Vector3(.x, .y, .z), Color.DarkGray, vn)
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


                camPosition = New Vector3(100.0F, 100.0F, 100.0F)
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


    End Class
End Namespace