Imports System.Windows.Forms

Imports Microsoft.Xna.Framework
Imports Microsoft.Xna.Framework.Graphics
Imports Microsoft.Xna.Framework.Input
Imports Microsoft.Xna.Framework.Content
Imports System.Threading
Imports System
Imports System.IO
Imports System.Diagnostics
Imports System.Drawing


Namespace OpenForge.Development
    ''' <summary>
    ''' This is the main type for your game.
    ''' </summary>
    Public Class MDMImageGenerator
        Inherits Game
        Private graphics As GraphicsDeviceManager
        Private spriteBatch As SpriteBatch
        'Private camTarget As Vector3
        'Private camPosition As Vector3
        'Private orbit As Boolean
        Private projectionMatrix As Matrix
        Private worldMatrix As Matrix
        Private triangleVertices() As VertexPositionColor
        Private BasicEffect As BasicEffect
        Private vertexBuffer As VertexBuffer
        Private screenshot As RenderTarget2D
        Private spriteFont As SpriteFont
        ' size of the screenshot images
        Private width As Int32 = 300
        Private height As Int32 = 300
        Private ScreenHeight As Int32 = 500
        Private ScreenWidth As Int32 = 500
        Private spriteFontPosition() As Vector2 = {New Vector2(5, 5), New Vector2(5, 20), New Vector2(5, 35), New Vector2(5, 50), New Vector2(5, 65)}
        Private spriteFontPosition2() As Vector2 = {New Vector2(5, ScreenHeight - 15 - 5), New Vector2(5, ScreenHeight - 15 - 20), New Vector2(5, ScreenHeight - 15 - 35), New Vector2(5, ScreenHeight - 15 - 50), New Vector2(5, ScreenHeight - 15 - 65)}
        Private text() As String = {"Press 'F' to load an *.STL object", _
                                    "Use arrow keys for NSEW views.", _
                                    "WASD to center object, mouse scroll to zoom.", _
                                    "'SpaceBar' saves screenshot.", _
                                    ""}
        Private Text2() As String = {"Top", "North", "East", "South", "West"}

        Private _total_frames As Int32 = 0
        Private _elapsed_time As Double = 0.0F
        Private _fps As Int32 = 0
        Private SavePath As String


        Private CurDir As eDir = eDir.North
        Private MoveIncrement As Single = 5.0F
        Private GetScreen As Boolean = False
        Private ScaleValue As Single = 3.0F
        Private ScrollValue As Int32
        Private Scales As New Vector3(3, 3, 3)
        Private ObjectCenter As Matrix = Matrix.Identity
        Private OutputGenerated(5) As Boolean

        Public Sub New()

            graphics = New GraphicsDeviceManager(Me)
            Content.RootDirectory = "Content"
            graphics.PreferredBackBufferWidth = 500
            graphics.PreferredBackBufferHeight = 500
        End Sub

        ''' <summary> 
        ''' Allows the game to perform any initialization it needs to before starting to run.
        ''' This is where it can query for any required services and load any non-graphic
        ''' related content.  Calling base.Initialize will enumerate through any components
        ''' and initialize them as well.
        ''' </summary>
        Protected Overrides Sub Initialize()
            MyBase.Initialize()

            projectionMatrix = Matrix.CreateOrthographic(500, 500, 1.0F, 10000.0F)
            worldMatrix = Matrix.CreateScale(Scales) * Matrix.CreateRotationX(MathHelper.ToRadians(90.0F))

            ' BasicEffect
            BasicEffect = New BasicEffect(GraphicsDevice)
            BasicEffect.Alpha = 1.0F
            BasicEffect.VertexColorEnabled = True
            BasicEffect.EnableDefaultLighting()

            spriteBatch = New SpriteBatch(GraphicsDevice)
            spriteFont = Content.Load(Of SpriteFont)("SpriteFont1")



        End Sub

        Public Enum eDir
            North
            East
            South
            West
            Top
            Unspecified
        End Enum

        Private RotateY As Matrix = Matrix.Identity
        Private RotateTop As Matrix = Matrix.Identity
        Private bolRotateToggle As Boolean = True

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
            Dim m As MouseState = Mouse.GetState()
            Static bolScrollStart As Boolean
            If Not bolScrollStart Then
                bolScrollStart = True
                ScrollValue = m.ScrollWheelValue
            End If
            Dim scrollticks As Int32 = m.ScrollWheelValue
            If Not scrollticks = ScrollValue Then
                ScaleValue += 0.001F * (scrollticks - ScrollValue)
                Scales = New Vector3(ScaleValue, ScaleValue, ScaleValue)

                ScrollValue = scrollticks
            End If
            ' Keyboard Input Processing Here
            Dim state As KeyboardState = Keyboard.GetState()
            With state
                If .IsKeyDown(Input.Keys.Up) Then
                    RotateY = Matrix.Identity
                    CurDir = eDir.North
                End If
                If .IsKeyDown(Input.Keys.Down) Then
                    RotateY = Matrix.CreateRotationY(MathHelper.ToRadians(180.0F))
                    CurDir = eDir.South
                End If
                If .IsKeyDown(Input.Keys.Left) Then
                    RotateY = Matrix.CreateRotationY(MathHelper.ToRadians(-90.0F))
                    CurDir = eDir.West
                End If
                If .IsKeyDown(Input.Keys.Right) Then
                    RotateY = Matrix.CreateRotationY(MathHelper.ToRadians(90.0F))
                    CurDir = eDir.East
                End If
                If .IsKeyDown(Input.Keys.Delete) Then

                    
                    bolRotateToggle = Not bolRotateToggle
                End If
                
                If (.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Escape)) Then End
                If (.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Space)) Then
                    Dim tb As Boolean = True
                    For i As Int32 = 0 To 4
                        tb = tb And OutputGenerated(i)
                    Next
                    If tb = True OrElse verticesloaded = False Then
                        If loadthreadrunning = False Then
                            loadthreadrunning = True
                            Dim thread As New Thread(AddressOf BackgroundLoader)
                            thread.SetApartmentState(ApartmentState.STA)
                            thread.Start()
                        End If
                    Else
                        GetScreen = True
                    End If
                End If
                If (.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.A)) Then
                    FocusPoint.X += MoveIncrement
                    CameraOffset.X += MoveIncrement
                    FocusPoint.Z -= MoveIncrement
                    CameraOffset.Z -= MoveIncrement
                End If
                If (.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.D)) Then
                    FocusPoint.X -= MoveIncrement
                    CameraOffset.X -= MoveIncrement
                    FocusPoint.Z += MoveIncrement
                    CameraOffset.Z += MoveIncrement
                End If
                If (.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.W)) Then
                    FocusPoint.X += MoveIncrement
                    CameraOffset.X += MoveIncrement
                    FocusPoint.Z += MoveIncrement
                    CameraOffset.Z += MoveIncrement
                End If
                If (.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.S)) Then
                    FocusPoint.X -= MoveIncrement
                    CameraOffset.X -= MoveIncrement
                    FocusPoint.Z -= MoveIncrement
                    CameraOffset.Z -= MoveIncrement
                End If

                'If (.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Space)) Then orbit = Not orbit

                If (.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.F)) Then
                    If loadthreadrunning = False Then
                        loadthreadrunning = True
                        Dim thread As New Thread(AddressOf BackgroundLoader)
                        thread.SetApartmentState(ApartmentState.STA)
                        thread.Start()
                    End If

                End If
            End With

            'If (orbit) Then
            '    Dim rotationMatrix As Matrix = Matrix.CreateRotationY(MathHelper.ToRadians(90.0F))
            '    camPosition = Vector3.Transform(camPosition, rotationMatrix)
            '    CurDir = CType(CurDir + eDir.North, eDir)
            '    If CurDir > eDir.West Then CurDir = CType(CurDir - eDir.Unspecified, eDir)
            '    orbit = False
            'End If
            MyBase.Update(gameTime)
        End Sub



        ''' <summary>
        ''' This is called when the game should draw itself.
        ''' </summary>
        ''' <param name="gameTime">Provides a snapshot of timing values.</param>
        Protected Overrides Sub Draw(gameTime As GameTime)
            _total_frames += 1

            BasicEffect.Projection = projectionMatrix
            BasicEffect.View = ViewMatrix
            BasicEffect.World = worldMatrix

            ' Turn off culling so we see both sides of our rendered triangle
            Dim RasterizerState As New RasterizerState()
            RasterizerState.CullMode = CullMode.None

            GraphicsDevice.Clear(Microsoft.Xna.Framework.Color.CornflowerBlue)
            GraphicsDevice.SetVertexBuffer(vertexBuffer)
            GraphicsDevice.RasterizerState = RasterizerState

            If bolRotateToggle Then
                RotateTop = Matrix.CreateRotationZ(MathHelper.ToRadians(55.0F)) * Matrix.CreateRotationY(MathHelper.ToRadians(135.0F))
            Else
                RotateTop = Matrix.Identity
            End If
            worldMatrix = ObjectCenter * Matrix.CreateScale(Scales) * Matrix.CreateRotationX(MathHelper.ToRadians(90.0F)) * RotateY * RotateTop

            ' Draw all the triangles for the currently loaded object
            For Each pass As EffectPass In BasicEffect.CurrentTechnique.Passes
                pass.Apply()
                If verticesloaded = True Then GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, vertices, 0, NumFacets, VertexPositionColorNormal.VertexDeclaration)
            Next

            ' Draw the text output
            spriteBatch.Begin(, , , DepthStencilState.Default)
            text(4) = "FPS=" + _fps.ToString + ", Triangle Count=" + NumFacets.ToString + ", Dir=" + CurDir.ToString
            For i As Int32 = 0 To 4
                spriteBatch.DrawString(spriteFont, text(i), spriteFontPosition(i), Microsoft.Xna.Framework.Color.Black)
            Next
            For i As Int32 = 0 To 4
                If OutputGenerated(i) = True Then
                    spriteBatch.DrawString(spriteFont, Text2(i), spriteFontPosition2(4 - i), Microsoft.Xna.Framework.Color.Black) '  
                Else
                    spriteBatch.DrawString(spriteFont, "*" + Text2(i), spriteFontPosition2(4 - i), Microsoft.Xna.Framework.Color.Red) '
                End If
            Next
            spriteBatch.End()


            ' Screenshot code 
            If GetScreen = True AndAlso verticesloaded = True Then
                Static bolAlreadyRun As Boolean
                If bolAlreadyRun = False Then
                    bolAlreadyRun = True
                    screenshot = New RenderTarget2D(GraphicsDevice, width, height, False, SurfaceFormat.Color, DepthFormat.Depth24)
                    ' rendering to the render target
                    GraphicsDevice.SetRenderTarget(screenshot)
                    GraphicsDevice.Clear(Microsoft.Xna.Framework.Color.CornflowerBlue)
                    For Each pass As EffectPass In BasicEffect.CurrentTechnique.Passes
                        pass.Apply()
                        If verticesloaded = True Then GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, vertices, 0, NumFacets, VertexPositionColorNormal.VertexDeclaration)
                    Next
                    GraphicsDevice.SetRenderTarget(Nothing) ' finished with render target
                    Dim b As System.Drawing.Bitmap
                    Using fs As New MemoryStream
                        ' save render target to stream
                        screenshot.SaveAsPng(fs, width, height)
                        ' read image from stream
                        b = New Bitmap(fs)
                    End Using
                    ' Make Background Transparent
                    b.MakeTransparent(System.Drawing.Color.CornflowerBlue)
                    ' save it!
                    Dim s As String = Path.GetDirectoryName(SavePath) + "\" + Path.GetFileNameWithoutExtension(SavePath) + "."
                    If bolRotateToggle Then
                        s += "Top"
                    Else
                        s += CurDir.ToString
                    End If
                    s += ".png"
                    'text(3) = s
                    b.Save(s, System.Drawing.Imaging.ImageFormat.Png)

                    ' launch it in system viewer
                    'Process.Start(s)
                    If bolRotateToggle Then
                        CurDir = eDir.North
                        RotateY = Matrix.Identity
                        bolRotateToggle = False
                        OutputGenerated(0) = True
                    Else
                        OutputGenerated(CurDir + 1) = True
                        CurDir = CType(CurDir + 1, eDir)
                        If CurDir > eDir.West Then CurDir = eDir.North
                        Select Case CurDir
                            Case eDir.North
                                RotateY = Matrix.Identity
                            Case eDir.South
                                RotateY = Matrix.CreateRotationY(MathHelper.ToRadians(180.0F))
                            Case eDir.West
                                RotateY = Matrix.CreateRotationY(MathHelper.ToRadians(-90.0F))
                            Case eDir.East
                                RotateY = Matrix.CreateRotationY(MathHelper.ToRadians(90.0F))
                        End Select
                    End If

                    worldMatrix = ObjectCenter * Matrix.CreateScale(Scales) * Matrix.CreateRotationX(MathHelper.ToRadians(90.0F)) * RotateY * RotateTop
                    bolAlreadyRun = False
                End If
                GetScreen = False
            End If
            If verticesloaded = False Then GetScreen = False
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
            fd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            dlgres = fd.ShowDialog()
            If dlgres = DialogResult.OK Then
                Dim stl As STLDefinition.STLObject
                SavePath = fd.FileName
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
                                vertices(i * 3) = New VertexPositionColorNormal(New Vector3(.x, .y, .z), Microsoft.Xna.Framework.Color.DarkGray, vn)
                            End With
                        End With
                        With .Facets(i)
                            With .normal
                                vn = New Vector3(.x, .y, .z)
                            End With
                            With .v2
                                vertices(i * 3 + 1) = New VertexPositionColorNormal(New Vector3(.x, .y, .z), Microsoft.Xna.Framework.Color.DarkGray, vn)
                            End With
                        End With
                        With .Facets(i)
                            With .normal
                                vn = New Vector3(.x, .y, .z)
                            End With
                            With .v3
                                vertices(i * 3 + 2) = New VertexPositionColorNormal(New Vector3(.x, .y, .z), Microsoft.Xna.Framework.Color.DarkGray, vn)
                            End With
                        End With
                    Next
                    ObjectCenter = Matrix.CreateTranslation(-.STLHeader.XCenter, -.STLHeader.YCenter, .STLHeader.ZCenter)
                End With
                verticesloaded = True
                For i As Int32 = 0 To 4
                    OutputGenerated(i) = False
                Next
                bolRotateToggle = True
                CurDir = eDir.North
                RotateY = Matrix.Identity
            End If
            loadthreadrunning = False
        End Sub




        ''' <summary>
        ''' LoadContent will be called once per game and is the place to load
        ''' all of your content.
        ''' </summary>
        Protected Overrides Sub LoadContent()
            ' Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = New SpriteBatch(GraphicsDevice)
        End Sub

        ''' <summary>
        ''' UnloadContent will be called once per game and is the place to unload
        ''' game-specific content.
        ''' </summary>
        Protected Overrides Sub UnloadContent()
            ' TODO: Unload any non ContentManager content here
            If Not screenshot Is Nothing Then screenshot.Dispose()
        End Sub

        Private FocusPoint As Vector3 = Vector3.Zero
        Private CameraOffset As Vector3 = New Vector3(1000.0F, 1000.0F, 1000.0F)

        Private ReadOnly Property ViewMatrix() As Matrix
            Get
                Return Matrix.CreateLookAt(CameraOffset, FocusPoint, Vector3.Up)
            End Get
        End Property


    End Class




End Namespace