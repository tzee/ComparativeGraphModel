Imports Emgu.CV.Features2D
Imports System.Math

Public Class FaceComparer

    Public Const COMPARISON_PERCENTAGE As Integer = 5

    Public Sub New()
        _edgeDetector = New EdgeDetector()
        _faceRecognizer = New FaceRecognizer()
        _featureExtractor = New FeatureExtractor()
        _lineCreator = New LineCreator()
    End Sub

    Public Function GetNodeMap(profile As profile) As ImageFeature()
        Dim grayScale As Bitmap = _faceRecognizer.FormatToGrayScale(profile.face)
        Dim edgesOnly As Bitmap = _faceRecognizer.FormatEdgeDection(grayScale)
        Dim faceEdgesOnly As Bitmap = _faceRecognizer.PullOutFace(profile.face, edgesOnly)
        Dim AttemptedFeatures As ImageFeature() = _featureExtractor.GetExtractedFeatures(faceEdgesOnly)
        Return AttemptedFeatures
    End Function

    Public Function Compare(profile As profile, face As Bitmap) As Double
        Dim grayScale As Bitmap = _faceRecognizer.FormatToGrayScale(face)
        Dim edgesOnly As Bitmap = _faceRecognizer.FormatEdgeDection(grayScale)
        Dim faceEdgesOnly As Bitmap = _faceRecognizer.PullOutFace(face, edgesOnly)
        Dim AttemptedFeatures As ImageFeature() = _featureExtractor.GetExtractedFeatures(faceEdgesOnly)

        Dim result As Double

        If profile.nodes IsNot Nothing Then
            Dim profileFeatures As ImageFeature() = profile.nodes.ToArray()

            CleanPoints(profileFeatures, AttemptedFeatures, COMPARISON_PERCENTAGE, face.Size)
            Dim NodeStayDeletedRatio As Double = profileFeatures.Count() / profile.nodes.Count()
            If NodeStayDeletedRatio > 1 Then
                NodeStayDeletedRatio = profile.nodes.Count() / profileFeatures.Count()
            End If
            result = ComputeANN(profileFeatures, AttemptedFeatures, NodeStayDeletedRatio)
        Else
            result = 0
        End If

        Return result
    End Function

    Private Function ComputeANN(originalPoints As ImageFeature(), comparitivePoints As ImageFeature(), nodeStayDelete As Double) As Double
        Dim result As Double = 0

        If originalPoints.Count() > 0 And comparitivePoints.Count() > 0 Then
            Dim originalLines As New List(Of Point())
            Dim modifiedLines As New List(Of Point())

            'Creating the original lines 
            For i As Integer = 0 To originalPoints.Count() - 1
                For j As Integer = 0 To comparitivePoints.Count() - 1
                    If i <> j Then
                        Dim tempPoint1 As New Point(originalPoints(i).KeyPoint.Point.X, originalPoints(i).KeyPoint.Point.Y)
                        Dim tempPoint2 As New Point(originalPoints(j).KeyPoint.Point.X, originalPoints(j).KeyPoint.Point.Y)
                        Dim tempList() As Point = {tempPoint1, tempPoint2}
                        originalLines.Add(tempList)
                    End If
                Next
            Next

            'Creating the modified lines
            For i As Integer = 0 To originalPoints.Count() - 1
                For j As Integer = 0 To comparitivePoints.Count() - 1
                    If i <> j Then
                        Dim tempPoint1 As New Point(originalPoints(i).KeyPoint.Point.X, originalPoints(i).KeyPoint.Point.Y)
                        Dim tempPoint2 As New Point(originalPoints(j).KeyPoint.Point.X, originalPoints(j).KeyPoint.Point.Y)
                        Dim tempList() As Point = {tempPoint1, tempPoint2}
                        modifiedLines.Add(tempList)
                    End If
                Next
            Next

            'Normalization of all points in originalPoints
            Dim totalX, totalY As Double
            Dim aveX, aveY As Double
            For Each line As Point() In originalLines
                totalX += line(0).X + line(1).X
                totalY += line(0).Y + line(1).Y
            Next

            aveX = totalX / (originalLines.Count() * 2)
            aveY = totalY / (originalLines.Count() * 2)

            For Each line As Point() In originalLines
                line(0).X -= aveX
                line(1).X -= aveX
                line(0).Y -= aveY
                line(1).Y -= aveY
            Next

            Dim hn1 As Double
            Dim wX1, wX2, wY1, wY2, bias As Double
            Dim learningConstant As Double = 0.01

            Dim rand As New Random
            wX1 = rand.Next(-10, 10)
            wX2 = rand.Next(-10, 10)
            wY1 = rand.Next(-10, 10)
            wY2 = rand.Next(-10, 10)
            bias = rand.Next(-5, 5)

            'Training the Neural Net
            For iterator As Integer = 0 To 50000
                hn1 = 0
                For i As Integer = 0 To originalLines.Count() - 1
                    hn1 += (originalLines(i)(0).X * wX1)
                    hn1 += (originalLines(i)(1).X * wX2)
                    hn1 += (originalLines(i)(0).Y * wY1)
                    hn1 += (originalLines(i)(1).Y * wY2)

                    hn1 += bias

                    Dim [error] As Double = Tanh(hn1) 'Hyperbolic Tangent

                    wX1 = wX1 + [error] * originalLines(i)(0).X * learningConstant
                    wX2 = wX2 + [error] * originalLines(i)(1).X * learningConstant
                    wY1 = wY1 + [error] * originalLines(i)(0).Y * learningConstant
                    wY2 = wY2 + [error] * originalLines(i)(1).Y * learningConstant
                Next
            Next

            'Normalization of all points in comparitivePoints
            Dim totalfX, totalfY As Double
            Dim avefX, avefY As Double
            For Each line As Point() In modifiedLines
                totalfX += line(0).X + line(1).X
                totalfY += line(0).Y + line(1).Y
            Next

            avefX = totalfX / (modifiedLines.Count() * 2)
            avefY = totalfY / (modifiedLines.Count() * 2)

            For Each line As Point() In modifiedLines
                line(0).X -= avefX
                line(1).X -= avefX
                line(0).Y -= avefY
                line(1).Y -= avefY
            Next

            'Run the comparitive points against the network to get a result as double
            Dim modified As Double

            For Each line As Point() In modifiedLines
                modified += line(0).X * wX1
                modified += line(0).Y * wY1
                modified += line(1).X * wX2
                modified += line(1).Y * wY2
            Next
            modified += bias

            result = (modified / hn1) * 100 * nodeStayDelete
        Else
            result = 0
        End If

        If result < 0 Or result > 100 Then
            result = 0
        End If

        Return result
    End Function

    Public Sub CleanPoints(ByRef storedData As ImageFeature(), ByRef attemptedData As ImageFeature(),
                            percentageAllowable As Double, imageSize As Size)
        Dim MAX_ALLOWABLE_DISTANCE As Double = (percentageAllowable / 100) * (Math.Sqrt(imageSize.Width * imageSize.Height))
        Dim resultStored As New List(Of ImageFeature)
        Dim resultAttempted As New List(Of ImageFeature)

        If storedData IsNot Nothing AndAlso storedData.Count <> 0 Then
            For i As Integer = 0 To storedData.Count() - 1
                For j As Integer = 0 To attemptedData.Count() - 1
                    If Distance(New Point(storedData(i).KeyPoint.Point.X, storedData(i).KeyPoint.Point.Y),
                                New Point(attemptedData(j).KeyPoint.Point.X, attemptedData(j).KeyPoint.Point.Y)) < MAX_ALLOWABLE_DISTANCE Then
                        resultStored.Add(storedData(i))
                        resultAttempted.Add(attemptedData(j))
                        Exit For
                    End If
                Next
            Next
        End If
        storedData = resultStored.ToArray()
        attemptedData = resultAttempted.ToArray()
    End Sub

    Private Function Distance(point1 As Point, point2 As Point) As Double
        Return Math.Sqrt(Math.Pow(point2.Y - point1.Y, 2) + Math.Pow(point2.X - point1.X, 2))
    End Function

    Public Function getComparisonPercentage() As Double
        Return COMPARISON_PERCENTAGE
    End Function

    Private Property _edgeDetector As EdgeDetector = Nothing
    Private Property _faceRecognizer As FaceRecognizer = Nothing
    Private Property _featureExtractor As FeatureExtractor = Nothing
    Private Property _lineCreator As LineCreator = Nothing
End Class
