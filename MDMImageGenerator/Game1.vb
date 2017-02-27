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
        Private spriteFontPosition() As Vector2 = {New Vector2(5, 5), New Vector2(5, 20), New Vector2(5, 35), New Vector2(5, 50), New Vector2(5, 65), New Vector2(5, 80), New Vector2(5, 95), New Vector2(5, 110), New Vector2(5, 125)}
        Private spriteFontPosition2() As Vector2 = {New Vector2(5, ScreenHeight - 15 - 5), New Vector2(5, ScreenHeight - 15 - 20), New Vector2(5, ScreenHeight - 15 - 35), New Vector2(5, ScreenHeight - 15 - 50), New Vector2(5, ScreenHeight - 15 - 65)}
        Private text() As String = {"Press 'F' to load an *.STL object, C to set color", _
                                    "Use arrow keys for NSEW views.", _
                                    "WASD to center object, mousewheel to scale.", _
                                    "'SpaceBar' saves screenshot & advances view.", _
                                    "", "", "", "", ""}
        Private Text2() As String = {"Top", "North", "East", "South", "West"}
        Private _total_frames As Int32 = 0
        Private _elapsed_time As Double = 0.0F
        Private _fps As Int32 = 0
        Private SavePath As String


        Private CurDir As eDir = eDir.North
        Private MoveIncrement As Single = 3.0F
        Private GetScreen As Boolean = False
        Private ScaleValue As Single = 3.0F
        Private ScrollValue As Int32
        Private Scales As New Vector3(3, 3, 3)
        Private ObjectCenter As Matrix = Matrix.Identity
        Private OutputGenerated(5) As Boolean
        Private ObjectColor As Microsoft.Xna.Framework.Color = Microsoft.Xna.Framework.Color.DarkGray
        Private colorthreadrunning As Boolean

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
        Private NumFacets As Int32 = 0
        Private verticesloaded As Boolean = False
        Private vertices() As VertexPositionColorNormal
        Private loadthreadrunning As Boolean = False
        Private SpaceDelay As Boolean = False


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
            BasicEffect = New BasicEffect(GraphicsDevice)
            BasicEffect.Alpha = 1.0F
            BasicEffect.VertexColorEnabled = True
            With BasicEffect
                .LightingEnabled = True '// turn on the lighting subsystem.
                .DirectionalLight0.DiffuseColor = New Vector3(0.5F, 0.5F, 0.5F) '// a red light
                .DirectionalLight0.Direction = New Vector3(0, -1, 0) '// coming along the x-axis
                .DirectionalLight0.SpecularColor = New Vector3(1, 1, 1) '// with green highlights
                .AmbientLightColor = New Vector3(0.2F, 0.2F, 0.2F)
                .EmissiveColor = New Vector3(0.7F, 0.7F, 0.7F)
            End With
            spriteBatch = New SpriteBatch(GraphicsDevice)
            spriteFont = Content.Load(Of SpriteFont)("SpriteFont1")

        End Sub


        ''' <summary>
        ''' Allows the game to run logic such as updating the world,
        ''' checking for collisions, gathering input, and playing audio.
        ''' </summary>
        ''' <param name="gameTime">Provides a snapshot of timing values.</param>
        Protected Overrides Sub Update(gameTime As GameTime)
            ' FPS logic here
            _elapsed_time += gameTime.ElapsedGameTime.TotalMilliseconds
            If (_elapsed_time > 1000.0F) Then ' 1 second has passed
                _fps = _total_frames
                _total_frames = 0
                _elapsed_time -= 1000.0F
            End If

            ' Timer to handle long keypresses on spacebar
            If SpaceDelay Then
                Static spacecount As Double
                spacecount += gameTime.ElapsedGameTime.TotalMilliseconds
                If spacecount > 250.0F Then
                    SpaceDelay = False
                    spacecount = 0
                End If
            End If

            ' Object scaling here
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
                If .IsKeyDown(Input.Keys.C) Then
                    If colorthreadrunning = False Then
                        colorthreadrunning = True
                        Dim thread As New Thread(AddressOf SetColor)
                        thread.SetApartmentState(ApartmentState.STA)
                        thread.Start()
                    End If
                End If

                If (.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Escape)) Then End
                If (.IsKeyDown(Input.Keys.R)) Then
                    For i As Int32 = 0 To 4
                        OutputGenerated(i) = False
                    Next
                    bolRotateToggle = True
                    FocusPoint.Z = 0
                    FocusPoint.X = 0
                    CameraOffset.X = 1000
                    CameraOffset.Z = 1000
                End If
                If (.IsKeyDown(Input.Keys.T)) Then
                    bolRotateToggle = True
                    FocusPoint.Z = 0
                    FocusPoint.X = 0
                    CameraOffset.X = 1000
                    CameraOffset.Z = 1000
                End If
                If (.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Space) And Not SpaceDelay) Then
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
                If (.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.F)) Then
                    If loadthreadrunning = False Then
                        loadthreadrunning = True
                        Dim thread As New Thread(AddressOf BackgroundLoader)
                        thread.SetApartmentState(ApartmentState.STA)
                        thread.Start()
                    End If
                End If
            End With
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
            text(5) = "xmin=" + xMin.ToString + ", xmax=" + xMax.ToString
            text(6) = "ymin=" + yMin.ToString + ", ymax=" + yMax.ToString
            text(7) = "zmin=" + zMin.ToString + ", zmax=" + zMax.ToString + ", X=" + FocusPoint.Z.ToString

            text(4) = "FPS=" + _fps.ToString + ", Triangle Count=" + NumFacets.ToString + ", Dir=" + CurDir.ToString + ", Scale=" + ScaleValue.ToString
            For i As Int32 = 0 To 7
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
                    Dim bolRunOnce As Boolean = True
                    Do While screenshot.IsContentLost Or bolRunOnce
                        GraphicsDevice.Clear(Microsoft.Xna.Framework.Color.CornflowerBlue)
                        For Each pass As EffectPass In BasicEffect.CurrentTechnique.Passes
                            pass.Apply()
                            GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, vertices, 0, NumFacets, VertexPositionColorNormal.VertexDeclaration)
                        Next
                        bolRunOnce = False
                    Loop
                    GraphicsDevice.SetRenderTarget(Nothing) ' finished with render target
                    Dim b As System.Drawing.Bitmap
                    Dim fs As New MemoryStream
                    ' save render target to stream
                    screenshot.SaveAsPng(fs, width, height)
                    ' read image from stream
                    b = New Bitmap(fs)
                    fs.Close()
                    ' Make Background Transparent
                    b.MakeTransparent(System.Drawing.Color.CornflowerBlue)
                    ' make filename
                    Dim s As String = Path.GetDirectoryName(SavePath) + "\" + Path.GetFileNameWithoutExtension(SavePath) + "."
                    If bolRotateToggle Then
                        s += "Top"
                    Else
                        s += CurDir.ToString
                    End If
                    s += ".png"
                    ' save it!
                    b.Save(s, System.Drawing.Imaging.ImageFormat.Png)

                    ' launch it in system viewer
                    'Process.Start(s)
                    If bolRotateToggle Then
                        CurDir = eDir.North
                        RotateY = Matrix.Identity
                        bolRotateToggle = False
                        OutputGenerated(0) = True
                        ScaleValue = ScaleValue * 0.7F
                        Scales = New Vector3(ScaleValue, ScaleValue, ScaleValue)
                    Else
                        OutputGenerated(CurDir + 1) = True
                        CurDir = CType(CurDir + 1, eDir)
                        If CurDir = eDir.Top Then CurDir = eDir.North
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
                    bolAlreadyRun = False
                End If
                GetScreen = False
                Spacedelay = True
                'worldMatrix = ObjectCenter * Matrix.CreateScale(Scales) * Matrix.CreateRotationX(MathHelper.ToRadians(90.0F)) * RotateY * RotateTop
            End If
            If verticesloaded = False Then GetScreen = False
            MyBase.Draw(gameTime)
        End Sub



        <STAThreadAttribute> _
        Sub SetColor()
            Dim c As New ColorDialog
            Dim dlgres As DialogResult
            c.Color = Drawing.Color.FromArgb(ObjectColor.A, ObjectColor.R, ObjectColor.G, ObjectColor.B)
            dlgres = c.ShowDialog()
            If dlgres = DialogResult.OK Then
                ObjectColor = Microsoft.Xna.Framework.Color.FromNonPremultiplied(c.Color.R, c.Color.G, c.Color.B, c.Color.A)
                BasicEffect.EmissiveColor = New Vector3(CSng(c.Color.R / 255), CSng(c.Color.G / 255), CSng(c.Color.B / 255))
                dlgres = DialogResult.Cancel
            End If
            c = Nothing
            colorthreadrunning = False
        End Sub

        ''' <summary>
        ''' Displays an open file dialog and then loads the chosen STL object.
        ''' </summary>
        ''' <remarks></remarks>
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
                                vertices(i * 3) = New VertexPositionColorNormal(New Vector3(.x, .y, .z), ObjectColor, vn)
                            End With
                        End With
                        With .Facets(i)
                            With .normal
                                vn = New Vector3(.x, .y, .z)
                            End With
                            With .v2
                                vertices(i * 3 + 1) = New VertexPositionColorNormal(New Vector3(.x, .y, .z), ObjectColor, vn)
                            End With
                        End With
                        With .Facets(i)
                            With .normal
                                vn = New Vector3(.x, .y, .z)
                            End With
                            With .v3
                                vertices(i * 3 + 2) = New VertexPositionColorNormal(New Vector3(.x, .y, .z), ObjectColor, vn)
                            End With
                        End With
                    Next
                    With .STLHeader
                        ObjectCenter = Matrix.CreateTranslation(-.XCenter, -.YCenter, .ZCenter)
                        xMin = .xmin
                        xMax = .xmax
                        yMin = .ymin
                        yMax = .ymax
                        zMin = .zmin
                        zMax = .zmax
                    End With
                End With
                verticesloaded = True
                For i As Int32 = 0 To 4
                    OutputGenerated(i) = False
                Next
                bolRotateToggle = True
                CurDir = eDir.North
                RotateY = Matrix.Identity
                FocusPoint.Z = 0
                FocusPoint.X = 0
                CameraOffset.X = 1000
                CameraOffset.Z = 1000
            End If
            loadthreadrunning = False
        End Sub

        Private xMin As Single, xMax As Single
        Private yMin As Single, yMax As Single
        Private zMin As Single, zMax As Single



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