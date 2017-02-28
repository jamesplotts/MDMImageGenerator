' Copyright 2017 by James Plotts.
' Licensed under Gnu GPL 3.0.

Imports System.Threading
Imports System


Namespace OpenForge.Development
    ''' <summary>
    ''' The main class.
    ''' </summary>
    Public Module Program

        ''' <summary>
        ''' The main entry point for the application.
        ''' </summary>
        <STAThread> _
        Public Sub Main()

#If Windows OrElse LINUX Then
            Using game = New MDMImageGenerator()
                game.Run()
            End Using
#End If
        End Sub
    End Module
End Namespace