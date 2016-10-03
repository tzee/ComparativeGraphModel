Imports Emgu.CV.Features2D

Public Class NEW_TEST

    Dim MAXIMUM_DISTANCE_THRESHOLD = 3.05 'Z
    Dim MINIMUM_AMOUNT_TO_MATCH = 5 'Allowable distance threshold

    Private Sub NEW_TEST_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        Dim imageList As List(Of Bitmap) = loadImages()
        Dim nodes As New List(Of ImageFeature())
        For Each image In imageList
            nodes.Add(GetNodeMap(image))
        Next

        Dim list = GetReducedNodes(nodes)

        For i As Integer = 0 To list.Count() - 1
            Console.WriteLine("Point " + (i + 1).ToString() + "   X: " + list(i).X.ToString() + " Y: " + list(i).Y.ToString())
        Next
    End Sub
    'Loads sample images into a list of bitmaps
    Private Function loadImages() As List(Of Bitmap)
        Dim x As New List(Of Bitmap)
        x.Add(New Bitmap("C:\Users\Tim Zee\Dropbox\Tim Paper\SampleImages\Sample1.png"))
        x.Add(New Bitmap("C:\Users\Tim Zee\Dropbox\Tim Paper\SampleImages\Sample2.png"))
        x.Add(New Bitmap("C:\Users\Tim Zee\Dropbox\Tim Paper\SampleImages\Sample3.png"))
        x.Add(New Bitmap("C:\Users\Tim Zee\Dropbox\Tim Paper\SampleImages\Sample4.png"))
        x.Add(New Bitmap("C:\Users\Tim Zee\Dropbox\Tim Paper\SampleImages\Sample5.png"))
        x.Add(New Bitmap("C:\Users\Tim Zee\Dropbox\Tim Paper\SampleImages\Sample6.png"))
        x.Add(New Bitmap("C:\Users\Tim Zee\Dropbox\Tim Paper\SampleImages\Sample7.png"))
        x.Add(New Bitmap("C:\Users\Tim Zee\Dropbox\Tim Paper\SampleImages\Sample8.png"))
        x.Add(New Bitmap("C:\Users\Tim Zee\Dropbox\Tim Paper\SampleImages\Sample9.png"))
        x.Add(New Bitmap("C:\Users\Tim Zee\Dropbox\Tim Paper\SampleImages\Sample10.png"))
        Return x
    End Function

    'Takes a bitmap and returns the node map for it 
    Private Function GetNodeMap(x As Bitmap) As ImageFeature()
        x = _edgeDetector.getGrayScale(x)
        x = _edgeDetector.getEdges(x)
        Dim nodes As ImageFeature() = _faceComparer.GetNodeMap(New profile() With {.face = x})
        Return nodes
    End Function

    Private Class Node
        Public originalPoint As Point
        Public testPoint As Point
        Public amount As Integer
    End Class

    'Run's node map comparison and returns quantitaive simialirty via a percentage
    Private Function RunANN(masterList As List(Of Point), testList As List(Of Point)) As Double
        Dim result As Double

        Return result
    End Function

    'Returns a list of points after the algorithm is applied
    Private Function GetReducedNodes(list As List(Of ImageFeature())) As List(Of Point)
        Dim returnList As New List(Of ImageFeature())
        Dim testList As New List(Of List(Of Node))
        Dim MasterList As New List(Of Node)
        Dim FinalList As New List(Of Point)

        'Gets a list of points that are within range from every image to every other image
        For i As Integer = 0 To list.Count() - 1
            testList.Add(New List(Of Node))
            For j As Integer = 0 To list.Count() - 1
                If i <> j Then
                    For p1 As Integer = 0 To list(i).Count() - 1
                        For p2 As Integer = 0 To list(j).Count() - 1
                            Dim point1 As Point = newPoint(list(i)(p1))
                            Dim point2 As Point = newPoint(list(j)(p2))

                            If withinRange(point1, point2) Then
                                testList(i).Add(New Node() With {.originalPoint = point1, .testPoint = point2})
                            End If
                        Next
                    Next
                End If
            Next
        Next

        'Takes a list of node point instances and condenceses to a list of node points and counts the
        'amount of instances and holds only one of the node positions
        For i As Integer = 0 To list.Count() - 1
            For j As Integer = 0 To testList(i).Count() - 1
                If i <> j Then
                    Dim originalPoint As Point? = testList(i)(j).originalPoint
                    Dim testPoint As Point? = testList(i)(j).testPoint

                    If originalPoint.HasValue And testPoint.HasValue Then
                        If Not MasterList.Any(Function(x As Node) withinRange(x.originalPoint, originalPoint)) Then
                            MasterList.Add(New Node() With {.originalPoint = originalPoint, .testPoint = testPoint, .amount = 1})
                        Else
                            Dim addToPoint = MasterList.FirstOrDefault(Function(x As Node) x.originalPoint = originalPoint)
                            If addToPoint IsNot Nothing Then
                                MasterList.Remove(MasterList.FirstOrDefault(Function(x As Node) x.originalPoint = addToPoint.originalPoint))
                                addToPoint.amount += 1
                                MasterList.Add(addToPoint)
                            End If
                        End If
                    End If
                End If
            Next
        Next

        'List returns only those nodes that appear in enough images from the list to be considered 
        'Common data rather than foreign data

        'TODO This should not just add the first occurence but take the average points of all of the 
        'occurences to create a balanced list for the final list
        For i As Integer = 0 To MasterList.Count() - 1
            If MasterList(i).amount >= MINIMUM_AMOUNT_TO_MATCH Then
                FinalList.Add(MasterList(i).originalPoint)
            End If
        Next

        Return FinalList
    End Function

    'Tests to see if two points are within the MAXIMUM_DISTANCE_THRESHOLD
    Private Function withinRange(point1 As Point, point2 As Point) As Boolean
        Return If(Distance(point1, point2) <= MAXIMUM_DISTANCE_THRESHOLD, True, False)
    End Function

    'Takes an ImageFeature and returns the same information represented as a point
    Private Function newPoint(x As ImageFeature)
        Return New Point(x.KeyPoint.Point.X, x.KeyPoint.Point.Y)
    End Function

    'Returns Euclidean distance between two points
    Private Function Distance(x As Point, y As Point) As Double
        Return Math.Sqrt(Math.Pow(y.X - x.X, 2) + Math.Pow(y.Y - x.Y, 2))
    End Function

    Dim _dataAccess = New FusiformDataAccess()
    Dim _edgeDetector = New EdgeDetector()
    Dim _faceComparer = New FaceComparer()
End Class