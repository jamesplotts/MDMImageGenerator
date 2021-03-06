﻿' Copyright 2017 by James Plotts.
' Licensed under Gnu GPL 3.0.



Imports System
Imports System.IO
Imports System.Drawing
Imports System.Threading
Imports System.Diagnostics
Imports System.Windows.Forms
Imports Microsoft.Xna.Framework
Imports Microsoft.Xna.Framework.Input
Imports Microsoft.Xna.Framework.Content
Imports Microsoft.Xna.Framework.Graphics


Namespace OpenForge.Development

    ''' <summary>
    ''' This is the main type for the program.
    ''' </summary>
    Public Class MDMImageGenerator
        Inherits Game
        Private graphicsgdm As GraphicsDeviceManager
        Private spriteBatch As SpriteBatch
        Private projectionMatrix As Matrix
        Private worldMatrix As Matrix
        Private triangleVertices() As VertexPositionColor
        Private BasicEffect As BasicEffect
        Private screenshot As RenderTarget2D
        Private spriteFont As SpriteFont
        ' size of the screenshot images
        Private width As Int32 = 300
        Private height As Int32 = 300
        Private ScreenHeight As Int32 = 500
        Private ScreenWidth As Int32 = 500
        Private spriteFontPosition() As Vector2 = {New Vector2(5, 5), New Vector2(5, 20), New Vector2(5, 35), New Vector2(5, 50), New Vector2(5, 65), New Vector2(5, 80), New Vector2(5, 95), New Vector2(5, 110), New Vector2(5, 125), New Vector2(5, 140)}
        Private spriteFontPosition2() As Vector2 = {New Vector2(5, ScreenHeight - 15 - 5), New Vector2(5, ScreenHeight - 15 - 20), New Vector2(5, ScreenHeight - 15 - 35), New Vector2(5, ScreenHeight - 15 - 50), New Vector2(5, ScreenHeight - 15 - 65)}
        Private text() As String = {"Press 'F' to load an *.STL object, 'C' to set color", _
                                    "Use arrow keys for NSEW views. 'K' to specify output folder.", _
                                    "WASD to center object, mousewheel to scale, 'T' toggles lighting.", _
                                    "'SpaceBar' saves screenshot & advances view.", _
                                    "", "", "", "", "", ""}
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
        Private UseSeparateOutputFolders As Boolean
        Private OutputFolders As String
        Private OldLoadThreadRunning As Boolean

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
        Private cfthreadrunning As Boolean = False
        Private SpaceDelay As Boolean = False
        Private xMin As Single, xMax As Single
        Private yMin As Single, yMax As Single
        Private zMin As Single, zMax As Single
        Private pvtDefaultLighting As Boolean = False
        Private tfpX As Single, tfpZ As Single
        Private coX As Single, coZ As Single
        Private tfpX2 As Single, tfpZ2 As Single
        Private tfpX3 As Single, tfpZ3 As Single
        Private tfpX4 As Single, tfpZ4 As Single
        Private tfpX5 As Single, tfpZ5 As Single
        Private coX2 As Single, coZ2 As Single
        Private coX3 As Single, coZ3 As Single
        Private coX4 As Single, coZ4 As Single
        Private coX5 As Single, coZ5 As Single




        Public Sub New()
            graphicsgdm = New GraphicsDeviceManager(Me)
            Content.RootDirectory = "Content"
            graphicsgdm.PreferredBackBufferWidth = 500
            graphicsgdm.PreferredBackBufferHeight = 500
        End Sub

        ''' <summary>
        ''' Allows the program to perform any initialization it needs to before starting to run.
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
            SetLighting()
            spriteBatch = New SpriteBatch(GraphicsDevice)
            spriteFont = Content.Load(Of SpriteFont)("SpriteFont1")

        End Sub

        Private Sub SetLighting()
            With BasicEffect
                If pvtDefaultLighting Then
                    .LightingEnabled = False '// turn off the lighting subsystem.
                    .DirectionalLight0.DiffuseColor = Nothing
                    .DirectionalLight0.Direction = Nothing
                    .DirectionalLight0.SpecularColor = Nothing
                    .AmbientLightColor = Nothing
                    .EmissiveColor = Nothing
                    .EnableDefaultLighting()
                Else
                    .LightingEnabled = True '// turn on the lighting subsystem.
                    .DirectionalLight0.DiffuseColor = New Vector3(0.5F, 0.5F, 0.5F) '// a gray light
                    .DirectionalLight0.Direction = New Vector3(0, -1, 0)
                    .DirectionalLight0.SpecularColor = New Vector3(1, 1, 1) '// with white highlights
                    .AmbientLightColor = New Vector3(0.2F, 0.2F, 0.2F)
                    .EmissiveColor = New Vector3(0.7F, 0.7F, 0.7F)
                End If
            End With
        End Sub

        Sub Setposition()
            Select Case CurDir
                Case eDir.North
                    tfpX4 = FocusPoint.X
                    coX4 = CameraOffset.X
                    tfpZ4 = FocusPoint.Z
                    coZ4 = CameraOffset.Z
                Case eDir.South
                    tfpX = FocusPoint.X
                    coX = CameraOffset.X
                    tfpZ = FocusPoint.Z
                    coZ = CameraOffset.Z
                Case eDir.West
                    tfpX2 = FocusPoint.X
                    coX2 = CameraOffset.X
                    tfpZ2 = FocusPoint.Z
                    coZ2 = CameraOffset.Z
                Case eDir.East
                    tfpX3 = FocusPoint.X
                    coX3 = CameraOffset.X
                    tfpZ3 = FocusPoint.Z
                    coZ3 = CameraOffset.Z
            End Select
        End Sub

        ''' <summary>
        ''' Allows the program to run logic such as updating the world,
        ''' checking for collisions, gathering input, and playing audio.
        ''' </summary>
        ''' <param name="gameTime">Provides a snapshot of timing values.</param>
        Protected Overrides Sub Update(gameTime As GameTime)
            ' FPS counter logic here
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
                If Me.IsActive Then
                    ScaleValue += 0.001F * (scrollticks - ScrollValue)
                    Scales = New Vector3(ScaleValue, ScaleValue, ScaleValue)
                    ScrollValue = scrollticks
                End If
            End If
            ' Keyboard Input Processing Here
            Dim state As KeyboardState = Keyboard.GetState()
            With state
                If .IsKeyDown(Input.Keys.Up) Then ' rotate object to the north direction
                    RotateY = Matrix.Identity
                    CurDir = eDir.North
                End If
                If .IsKeyDown(Input.Keys.Down) Then ' rotate object to the south direction
                    RotateY = Matrix.CreateRotationY(MathHelper.ToRadians(180.0F))
                    CurDir = eDir.South
                End If
                If .IsKeyDown(Input.Keys.Left) Then ' rotate object to the west direction
                    RotateY = Matrix.CreateRotationY(MathHelper.ToRadians(-90.0F))
                    CurDir = eDir.West
                End If
                If .IsKeyDown(Input.Keys.Right) Then ' rotate object to the east direction
                    RotateY = Matrix.CreateRotationY(MathHelper.ToRadians(90.0F))
                    CurDir = eDir.East
                End If
                If .IsKeyDown(Input.Keys.C) Then  ' Launch color dialog thread
                    If colorthreadrunning = False Then
                        colorthreadrunning = True
                        Dim thread As New Thread(AddressOf SetColor)
                        thread.SetApartmentState(ApartmentState.STA)
                        thread.Start()
                    End If
                End If

                If .IsKeyDown(Input.Keys.K) Then ' Configure output folders
                    If cfthreadrunning = False Then
                        cfthreadrunning = True
                        Dim thread As New Thread(AddressOf ConfigureFolders)
                        thread.SetApartmentState(ApartmentState.STA)
                        thread.Start()
                    End If
                End If

                If (.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Escape)) Then End ' the program

                If (.IsKeyDown(Input.Keys.R)) Then ' reset flags denoting which views were exported
                    For i As Int32 = 0 To 4
                        OutputGenerated(i) = False
                    Next
                    bolRotateToggle = True
                    FocusPoint.Z = 0
                    FocusPoint.X = 0
                    CameraOffset.X = 1000
                    CameraOffset.Z = 1000
                End If

                If (.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Space) And Not SpaceDelay) Then ' Either load a file or save a screenshot and advance the view
                    Dim tb As Boolean = True
                    For i As Int32 = 0 To 4
                        tb = tb And OutputGenerated(i)
                        
                        
                    Next
                    If tb = True OrElse verticesloaded = False Then
                        If loadthreadrunning = False Then
                            tfpX = FocusPoint.X
                            coX = CameraOffset.X
                            tfpZ = FocusPoint.Z
                            coZ = CameraOffset.Z
                            loadthreadrunning = True
                            Dim thread As New Thread(AddressOf BackgroundLoader)
                            thread.SetApartmentState(ApartmentState.STA)
                            thread.Start()
                            OldLoadThreadRunning = True
                        End If
                    Else
                        GetScreen = True
                    End If
                End If

                If (.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.A)) Then  ' shift the object to the left in the view
                    FocusPoint.X += MoveIncrement
                    CameraOffset.X += MoveIncrement
                    FocusPoint.Z -= MoveIncrement
                    CameraOffset.Z -= MoveIncrement
                    Setposition()
                End If
                If (.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.D)) Then  ' shift the object to the right in the view
                    FocusPoint.X -= MoveIncrement
                    CameraOffset.X -= MoveIncrement
                    FocusPoint.Z += MoveIncrement
                    CameraOffset.Z += MoveIncrement
                    Setposition()
                End If
                If (.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.W)) Then  ' shift the object up in the view
                    FocusPoint.X += MoveIncrement
                    CameraOffset.X += MoveIncrement
                    FocusPoint.Z += MoveIncrement
                    CameraOffset.Z += MoveIncrement
                    Setposition()
                End If
                If (.IsKeyDown(Input.Keys.O)) Then
                    CenterObject()
                    Setposition()
                End If
                If (.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.S)) Then  ' shift the object down in the view
                    FocusPoint.X -= MoveIncrement
                    CameraOffset.X -= MoveIncrement
                    FocusPoint.Z -= MoveIncrement
                    CameraOffset.Z -= MoveIncrement
                    Setposition()
                End If
                If (.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.F)) Then ' load a new object
                    If loadthreadrunning = False Then
                        loadthreadrunning = True
                        Dim thread As New Thread(AddressOf BackgroundLoader)
                        thread.SetApartmentState(ApartmentState.STA)
                        thread.Start()
                    End If
                End If
                If (.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.T)) AndAlso Not SpaceDelay Then   ' toggle default lighting mode
                    pvtDefaultLighting = Not pvtDefaultLighting
                    SetLighting()
                    SpaceDelay = True
                End If
            End With
            MyBase.Update(gameTime)
        End Sub

        ''' <summary>
        ''' This is called when the game should draw itself.
        ''' </summary>
        ''' <param name="gameTime">Provides a snapshot of timing values.</param>
        Protected Overrides Sub Draw(gameTime As GameTime)
            _total_frames += 1 ' FPS Counter

            If OldLoadThreadRunning = True And loadthreadrunning = False Then
                OldLoadThreadRunning = False
                CenterObject()

            End If
            If SizingUp Then
                ScaleValue += 0.1F
                If Not IsBorderClear() Then SizingUp = False
            End If
            If SizingDown Then
                ScaleValue -= 0.1F
                If IsBorderClear() Then SizingDown = False
            End If

            ' Set various matrices
            If bolRotateToggle Then
                RotateTop = Matrix.CreateRotationZ(MathHelper.ToRadians(55.0F)) * Matrix.CreateRotationY(MathHelper.ToRadians(135.0F))
            Else
                RotateTop = Matrix.Identity
            End If
            Scales = New Vector3(ScaleValue, ScaleValue, ScaleValue)
            worldMatrix = ObjectCenter * Matrix.CreateScale(Scales) * Matrix.CreateRotationX(MathHelper.ToRadians(90.0F)) * RotateY * RotateTop
            BasicEffect.Projection = projectionMatrix
            BasicEffect.View = ViewMatrix
            BasicEffect.World = worldMatrix

            ' Turn off culling so we see both sides of our rendered triangle
            Dim RasterizerState As New RasterizerState()
            RasterizerState.CullMode = CullMode.None

            ' Prepare GraphicsDevice
            GraphicsDevice.Clear(Microsoft.Xna.Framework.Color.CornflowerBlue)
            GraphicsDevice.RasterizerState = RasterizerState

            Try
                ' Draw all the triangles for the currently loaded object
                For Each pass As EffectPass In BasicEffect.CurrentTechnique.Passes
                    pass.Apply()
                    If verticesloaded = True Then
                        GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, vertices, 0, NumFacets, VertexPositionColorNormal.VertexDeclaration)
                    Else

                    End If

                Next
            Catch e As Exception
            End Try


            ' Prepare screen text display
            text(4) = "FPS=" + _fps.ToString + ", Triangle Count=" + NumFacets.ToString + ", Dir=" + CurDir.ToString + ", Scale=" + ScaleValue.ToString
            text(5) = "xmin=" + xMin.ToString + ", xmax=" + xMax.ToString ' object bounds
            text(6) = "ymin=" + yMin.ToString + ", ymax=" + yMax.ToString
            text(7) = "zmin=" + zMin.ToString + ", zmax=" + zMax.ToString
            text(8) = "XY: " + pvtUS1.ToString + ", " + pvtUS2.ToString
            'text(8) = "cam pos: " + CameraOffset.ToString
            ' Draw the text output to the screen
            spriteBatch.Begin(, , , DepthStencilState.Default)
            For i As Int32 = 0 To 8 ' upper left text array
                spriteBatch.DrawString(spriteFont, text(i), spriteFontPosition(i), Microsoft.Xna.Framework.Color.Black)
            Next
            For i As Int32 = 0 To 4 ' lower left text array
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
                If bolAlreadyRun = False Then ' rendering to a RenderTarget2D
                    bolAlreadyRun = True
                    Dim b As System.Drawing.Bitmap
                    b = GrabScreenshot()
                    If bolRotateToggle Then ' Crop image
                        If BmpHasNonTransparentArea(b) Then
                            b = CropBitmap(b, pvtCropRegion)
                        End If
                    End If
                    ' Make Background Transparent
                    b.MakeTransparent(System.Drawing.Color.CornflowerBlue)
                    ' assemble output filename
                    If OutputFolders Is Nothing OrElse OutputFolders = "" Then
                        OutputFolders = Path.GetDirectoryName(SavePath)
                        ' OutputFolders = SavePath '
                    End If

                    Dim s As String = OutputFolders + "\" + Path.GetFileNameWithoutExtension(SavePath) + "."
                    If bolRotateToggle Then
                        s += "Top"
                    Else
                        s += CurDir.ToString
                    End If
                    s += ".png"
                    ' save the PNG image!
                    b.Save(s, System.Drawing.Imaging.ImageFormat.Png)

                    ' launch it in system viewer
                    ' Process.Start(s)
                    If bolRotateToggle Then ' Advance image from Top-Down to North Isometric
                        CurDir = eDir.North
                        RotateY = Matrix.Identity
                        bolRotateToggle = False
                        OutputGenerated(0) = True
                        ScaleValue = ScaleValue * 0.7F
                        Scales = New Vector3(ScaleValue, ScaleValue, ScaleValue)
                        
                    Else ' Advance image to next isometric
                        OutputGenerated(CurDir + 1) = True
                        CurDir = CType(CurDir + 1, eDir)
                        If CurDir = eDir.Top Then CurDir = eDir.North
                        Select Case CurDir
                            Case eDir.North
                                RotateY = Matrix.Identity
                                tfpX4 = FocusPoint.X
                                coX4 = CameraOffset.X
                                tfpZ4 = FocusPoint.Z
                                coZ4 = CameraOffset.Z
                                FocusPoint.X = tfpX
                                CameraOffset.X = coX
                                FocusPoint.Z = tfpZ
                                CameraOffset.Z = coZ
                            Case eDir.South
                                RotateY = Matrix.CreateRotationY(MathHelper.ToRadians(180.0F))
                                tfpX = FocusPoint.X
                                coX = CameraOffset.X
                                tfpZ = FocusPoint.Z
                                coZ = CameraOffset.Z
                                FocusPoint.X = tfpX2
                                CameraOffset.X = coX2
                                FocusPoint.Z = tfpZ2
                                CameraOffset.Z = coZ2

                            Case eDir.West
                                RotateY = Matrix.CreateRotationY(MathHelper.ToRadians(-90.0F))
                                tfpX2 = FocusPoint.X
                                coX2 = CameraOffset.X
                                tfpZ2 = FocusPoint.Z
                                coZ2 = CameraOffset.Z
                                FocusPoint.X = tfpX3
                                CameraOffset.X = coX3
                                FocusPoint.Z = tfpZ3
                                CameraOffset.Z = coZ3

                            Case eDir.East
                                RotateY = Matrix.CreateRotationY(MathHelper.ToRadians(90.0F))
                                tfpX3 = FocusPoint.X
                                coX3 = CameraOffset.X
                                tfpZ3 = FocusPoint.Z
                                coZ3 = CameraOffset.Z
                                FocusPoint.X = tfpX4
                                CameraOffset.X = coX4
                                FocusPoint.Z = tfpZ4
                                CameraOffset.Z = coZ4

                        End Select
                    End If
                    bolAlreadyRun = False
                End If
                bolAlreadyRun = False
                GetScreen = False
                SpaceDelay = True
            End If
            If verticesloaded = False Then GetScreen = False
            MyBase.Draw(gameTime)
        End Sub


        Private SizingUp As Boolean = False
        Private SizingDown As Boolean = False

        Private Sub CenterObject()
            If IsBorderClear() Then
                SizingUp = True
            Else
                SizingDown = True
            End If
        End Sub

        Private Function IsBorderClear() As Boolean
            Dim topcleared As Boolean = True
            Dim bottomcleared As Boolean = True
            Dim leftcleared As Boolean = True
            Dim rightcleared As Boolean = True
            Dim b As Bitmap
            b = GrabScreenshot()
            If Not b Is Nothing Then
                With b
                    Dim x As Int32 = 0
                    Do While x < b.Width - 1 And topcleared And bottomcleared
                        topcleared = topcleared And (.GetPixel(x, 0).ToArgb = System.Drawing.Color.CornflowerBlue.ToArgb)
                        bottomcleared = bottomcleared And (.GetPixel(x, b.Height - 1).ToArgb = System.Drawing.Color.CornflowerBlue.ToArgb)
                        x += 1
                    Loop
                    If Not topcleared Or Not bottomcleared Then Return False
                    Dim y As Int32 = 0
                    Do While y < .Height - 1 And leftcleared And rightcleared
                        leftcleared = leftcleared And (.GetPixel(0, y).ToArgb = System.Drawing.Color.CornflowerBlue.ToArgb)
                        rightcleared = rightcleared And (.GetPixel(b.Width - 1, y).ToArgb = System.Drawing.Color.CornflowerBlue.ToArgb)
                        y += 1
                    Loop
                    If Not leftcleared Or Not rightcleared Then Return False
                End With
                b = Nothing
                Return True
            Else
                Return False
            End If
        End Function

        Private Function GrabScreenshot() As Bitmap
            Dim ss As RenderTarget2D
            Dim b As System.Drawing.Bitmap = Nothing
            Static bolGeneratingCurrently As Boolean
            If Not bolGeneratingCurrently Then
                bolGeneratingCurrently = True
                Scales = New Vector3(ScaleValue, ScaleValue, ScaleValue)
                worldMatrix = ObjectCenter * Matrix.CreateScale(Scales) * Matrix.CreateRotationX(MathHelper.ToRadians(90.0F)) * RotateY * RotateTop
                BasicEffect.Projection = projectionMatrix
                BasicEffect.View = ViewMatrix
                BasicEffect.World = worldMatrix

                ' Turn off culling so we see both sides of our rendered triangle
                Dim RasterizerState As New RasterizerState()
                RasterizerState.CullMode = CullMode.None

                ' Prepare GraphicsDevice
                'GraphicsDevice.Clear(Microsoft.Xna.Framework.Color.CornflowerBlue)
                GraphicsDevice.RasterizerState = RasterizerState

                ss = New RenderTarget2D(GraphicsDevice, width, height, False, SurfaceFormat.Color, DepthFormat.Depth24)
                GraphicsDevice.SetRenderTarget(ss)
                Dim bolRunOnce As Boolean = True
                Do While bolRunOnce
                    GraphicsDevice.Clear(Microsoft.Xna.Framework.Color.CornflowerBlue)
                    For Each pass As EffectPass In BasicEffect.CurrentTechnique.Passes
                        pass.Apply()
                        If verticesloaded = True Then
                            GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, vertices, 0, NumFacets, VertexPositionColorNormal.VertexDeclaration)
                        End If
                    Next
                    bolRunOnce = False
                Loop
                GraphicsDevice.SetRenderTarget(Nothing) ' finished with render target

                Dim fs As New MemoryStream
                Try
                    ' save intermediate PNG image to stream
                    ss.SaveAsPng(fs, width, height)
                    ' read image from stream to a bitmap object
                    b = New Bitmap(fs)
                Catch
                End Try
                fs.Close()
                ss = Nothing
                fs = Nothing
                bolGeneratingCurrently = False
            End If
            Return b
        End Function

        <STAThreadAttribute> _
        Sub ConfigureFolders()
            cfthreadrunning = True
            Dim f As New FolderBrowserDialog
            If SavePath Is Nothing OrElse SavePath.Length = 0 Then
                SavePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            End If
            f.SelectedPath = SavePath.Substring(0, SavePath.Length - Path.GetFileName(SavePath).Length)
            f.Description = "Select Output Folder for images:"
            Dim dlgres As DialogResult
            dlgres = f.ShowDialog()
            If dlgres = DialogResult.OK Then OutputFolders = f.SelectedPath
            cfthreadrunning = False
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


        Private pvtUS1 As Single, pvtUS2 As Single
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

            fd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            dlgres = fd.ShowDialog()
            If dlgres = DialogResult.OK Then
                Dim stl As STLDefinition.STLObject
                SavePath = fd.FileName
                stl = STLDefinition.LoadSTL(fd.OpenFile())
                NumFacets = CInt(stl.STLHeader.nfacets)
                pvtUS1 = stl.STLHeader.xpixelsize
                pvtUS2 = stl.STLHeader.ypixelsize
                Dim vn As Vector3
                Dim lVertices(NumFacets * 3) As VertexPositionColorNormal
                With stl
                    For i As Int32 = 0 To NumFacets - 1
                        With .Facets(i)
                            With .normal
                                vn = New Vector3(.x, .y, .z)
                            End With
                            With .v1
                                lVertices(i * 3) = New VertexPositionColorNormal(New Vector3(.x, .y, .z), ObjectColor, vn)
                            End With
                        End With
                        With .Facets(i)
                            With .normal
                                vn = New Vector3(.x, .y, .z)
                            End With
                            With .v2
                                lVertices(i * 3 + 1) = New VertexPositionColorNormal(New Vector3(.x, .y, .z), ObjectColor, vn)
                            End With
                        End With
                        With .Facets(i)
                            With .normal
                                vn = New Vector3(.x, .y, .z)
                            End With
                            With .v3
                                lVertices(i * 3 + 2) = New VertexPositionColorNormal(New Vector3(.x, .y, .z), ObjectColor, vn)
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
                verticesloaded = False
                'ReDim vertices(NumFacets * 3)
                vertices = lVertices
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

        Private pvtCropRegion As System.Drawing.Rectangle
        Private Function BmpHasNonTransparentArea(ByVal bmp As Bitmap) As Boolean
            Dim x As Int32 = GetLeftMostSolidPix(bmp)
            If x > -1 Then
                Dim x2 As Int32 = GetRightMostSolidPix(bmp)
                If x2 > -1 Then
                    Dim y As Int32 = GetTopMostSolidPix(bmp)
                    If y > -1 Then
                        Dim y2 As Int32 = GetBottomMostSolidPix(bmp)
                        If y2 > -1 Then
                            pvtCropRegion = New Drawing.Rectangle
                            With pvtCropRegion
                                .X = x
                                .Y = y
                                .Width = x2 - x
                                .Height = y2 - y
                            End With
                            Return True
                        End If
                    End If
                End If
            End If
            Return False
        End Function

        Private Function GetLeftMostSolidPix(ByVal bmp As Bitmap) As Int32
            Dim c As Drawing.Color = Drawing.Color.FromArgb(255, 100, 149, 237)
            For i As Int32 = 0 To bmp.Size.Width - 1
                For j As Int32 = 0 To bmp.Size.Height - 1
                    If Not bmp.GetPixel(i, j) = c Then Return i
                Next
            Next
            Return -1
        End Function

        Private Function GetRightMostSolidPix(ByVal bmp As Bitmap) As Int32
            Dim c As Drawing.Color = Drawing.Color.FromArgb(255, 100, 149, 237)
            For i As Int32 = bmp.Size.Width - 1 To 0 Step -1
                For j As Int32 = 0 To bmp.Size.Height - 1
                    If Not bmp.GetPixel(i, j) = c Then Return i
                Next
            Next
            Return -1
        End Function

        Private Function GetTopMostSolidPix(ByVal bmp As Bitmap) As Int32
            Dim c As Drawing.Color = Drawing.Color.FromArgb(255, 100, 149, 237)
            For j As Int32 = 0 To bmp.Size.Height - 1
                For i As Int32 = 0 To bmp.Size.Width - 1
                    If Not bmp.GetPixel(i, j) = c Then Return j
                Next
            Next
            Return -1
        End Function

        Private Function GetBottomMostSolidPix(ByVal bmp As Bitmap) As Int32
            Dim c As Drawing.Color = Drawing.Color.FromArgb(255, 100, 149, 237)
            For j As Int32 = bmp.Size.Height - 1 To 0 Step -1
                For i As Int32 = 0 To bmp.Size.Width - 1
                    If Not bmp.GetPixel(i, j) = c Then Return j
                Next
            Next
            Return -1
        End Function

        Private Function CropBitmap(ByRef bmp As Bitmap, ByVal cropX As Integer, ByVal cropY As Integer, ByVal cropWidth As Integer, ByVal cropHeight As Integer) As Bitmap
            Return CropBitmap(bmp, New System.Drawing.Rectangle(cropX, cropY, cropWidth, cropHeight))
        End Function

        Private Function CropBitmap(ByRef bmp As Bitmap, ByVal rect As Drawing.Rectangle) As Bitmap
            Dim cropped As Bitmap = bmp.Clone(rect, bmp.PixelFormat)
            Return cropped
        End Function

    End Class




End Namespace