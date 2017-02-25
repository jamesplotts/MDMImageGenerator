Imports System
Imports System.IO


' Thanks to thjerman for the Stereolithography File Formats post at http://forums.codeguru.com/showthread.php?148668-loading-a-stl-3d-model-file

Public Class STLDefinition

    Public Class STLHeader
        'id' is a null-terminated string of the form "filename.stl", where filename is the name of the converted ".bin" file.
        Public Id(22) As Char
        'date' is the date stamp in UNIX ctime() format.
        Public DateCreated(26) As Char
        'xmin' - 'zmax' are the geometric bounds on the data 
        Public xmin As Single
        Public xmax As Single
        Public ymin As Single
        Public ymax As Single
        Public zmin As Single
        Public zmax As Single
        Public xpixelsize As Single  ' Dimensions of grid for this model
        Public ypixelsize As Single  ' in user units.
        Public nfacets As UInt32
    End Class

    Public Class Vertex
        Public x As Single
        Public y As Single
        Public z As Single
    End Class

    Public Class Facet
        Public normal As New Vertex '  facet surface normal
        Public v1 As New Vertex     '  vertex 1
        Public v2 As New Vertex     '  vertex 2
        Public v3 As New Vertex     '  vertex 3
    End Class

    Public Class STLObject
        Public STLHeader As New STLHeader
        Public Facets() As Facet
    End Class

    ''' <summary>
    ''' LoadSTL reads a stream and converts the data to an STLObject.  
    ''' Stream must contain an STL formatted file.
    ''' </summary>
    ''' <param name="stream"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function LoadSTL(ByVal stream As Stream) As STLDefinition.STLObject
        Dim vStl As New STLDefinition.STLObject
        Try
            Dim index As Int32 = 0
            Dim tb(4) As Byte
            Dim tb2(30) As Byte
            Dim sr As Stream = stream
            stream.Read(tb2, 0, 22)
            stream.Read(tb2, 0, 26)
            With vStl
                With .STLHeader
                    .xmin = rc(sr)
                    .xmax = rc(sr)
                    .ymin = rc(sr)
                    .ymax = rc(sr)
                    .xmin = rc(sr)
                    .xmax = rc(sr)
                    .xpixelsize = rc(sr)
                    .ypixelsize = rc(sr)
                    sr.Read(tb, 0, 4)
                    .nfacets = BitConverter.ToUInt32(tb, 0)
                End With
                ReDim .Facets(CInt(.STLHeader.nfacets))
                For i As Int32 = 0 To CInt(.STLHeader.nfacets) - 1
                    .Facets(i) = LoadFacet(sr)
                Next
            End With
        Catch
            vStl = Nothing
        End Try
        Return vStl
    End Function

    Private Shared Function LoadFacet(ByVal sr As Stream) As Facet
        Dim retval As New Facet
        With retval
            With .normal
                .x = rc(sr)
                .y = rc(sr)
                .z = rc(sr)
            End With
            With .v1
                .x = rc(sr)
                .y = rc(sr)
                .z = rc(sr)
            End With
            With .v2
                .x = rc(sr)
                .y = rc(sr)
                .z = rc(sr)
            End With
            With .v3
                .x = rc(sr)
                .y = rc(sr)
                .z = rc(sr)
            End With
        End With
        Dim tb(2) As Byte
        sr.Read(tb, 0, 2) ' just padding bytes not used.
        Return retval
    End Function

    ''' <summary>
    ''' Reads 4 bytes from the supplied Stream,
    ''' converts them to a Single and returns the result.
    ''' </summary>
    ''' <param name="sr">A valid Stream object</param>
    ''' <returns>A Single value read from the stream.</returns>
    ''' <remarks></remarks>
    Private Shared Function rc(ByVal sr As Stream) As Single
        Dim tb(4) As Byte
        sr.Read(tb, 0, 4)
        Return BitConverter.ToSingle(tb, 0)
    End Function

End Class


