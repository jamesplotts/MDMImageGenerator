Imports System
Imports System.IO


'Stereolithography File Formats

'Binary Format (.stl)

'Binary (.stl) files are organized as an 84 byte header followed by = 50-byte records each of which describes one triangle facet:=20.

'# of | bytes | description
'-------------------------------------------------------------------
'80 | Any text such as the creator's name
'4 | int equal to the number of facets in file
'----here is where the facets start (triangle 1)-----------------
'4 | float normal x
'4 | float normal y
'4 | float normal z
'4 | float vertex1 x
'4 | float vertex1 y
'4 | float vertex1 z
'4 | float vertex2 x
'4 | float vertex2 y
'4 | float vertex2 z
'4 | float vertex3 x
'4 | float vertex3 y
'4 | float vertex3 z
'2 | unused (padding to make 50-bytes)
'--------------------------------facet 2-------------------------
'4 | float normal x
'4 | float normal y
'4 | float normal z
'4 | float vertex1 x
'4 | float vertex1 y
'4 | float vertex1 z
'4 | float vertex2 x
'4 | float vertex2 y
'4 | float vertex2 z
'4 | float vertex3 x
'4 | float vertex3 y
'4 | float vertex3 z
'2 | unused (padding to make 50-bytes)
'--------------------------------facet 3-------------------------
'etc. ...

'A facet entry begins with the x,y,z components of the triangle's face normal vector. The normal vector points in a direction away from the surface and it should be
'normalized to unit length. The x,y,z = coordinates of the triangle's three vertices come next. They are stored in CCW = order when viewing the facet from outside the
'surface. The direction of the normal vector follows the "right-hand-rule" when traversing the triangle vertices from 1 to 3, i.e., with the fingers of your right hand curled
'in the direction of vertex 1 to 2 to 3, your thumb points in the = direction of the surface normal.

'Notice that each facet entry is 50 bytes. So adding the 84 bytes in the header space, a binary file should have a size in bytes =3D 84 + (number of facets) * 50. 

'Notice the 2 extra bytes thrown in at the end of each entry to make it a nice even 50. 50 is a nice number for people, but not for most 32-bit computers because they
'store values on 4-byte boundaries. Therefore, when writing programs to read and write .stl files the programmer has to take care to design data structures that
'accomodate this problem. 

'The Velocity2 software writes a binary .stl file having the general organization described above. The first 84 bytes of the file form the = header, a C-language
'structure having the definition: 

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


