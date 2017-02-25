Imports System.Threading
Imports System


Namespace safeprojectname
    ''' <summary>
    ''' The main class.
    ''' </summary>
    Public Module Program

        ''' <summary>
        ''' The main entry point for the application.
        ''' </summary>
        Public Sub Main()

#If Windows OrElse LINUX Then
            Using game = New Game1()
                game.Run()
            End Using
#End If
        End Sub
    End Module
End Namespace