Imports Emgu.CV.Features2D
Imports Emgu.CV.Structure

Public Class FeatureExtractor

    Public Function GetExtractedFeatures(image As Bitmap) As ImageFeature()
        Dim convertedImage As New Emgu.CV.Image(Of Gray, Byte)(image)
        Dim detector As New SURFDetector(400, False)
        Dim result As ImageFeature() = detector.DetectFeatures(convertedImage, Nothing)
        Return result
    End Function

    Public Function ExtractedBitmap(original As Bitmap, features As ImageFeature()) As Bitmap
        Dim result As New Bitmap(original)
        Dim graphics As Graphics = Graphics.FromImage(result)
        Dim pen As New Pen(Color.DarkCyan, 3)
        For Each feature In features
            graphics.DrawRectangle(pen, feature.KeyPoint.Point.X, feature.KeyPoint.Point.Y, 1, 1)
        Next
        Return result
    End Function
End Class
