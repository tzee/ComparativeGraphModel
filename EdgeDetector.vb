Imports System.Drawing.Imaging
Imports AForge.Imaging
Imports AForge.Imaging.Filters

Public Class EdgeDetector : Inherits BaseUsingCopyPartialFilter

    Public Function getGrayScale(input As Bitmap) As Bitmap
        Dim gsFilter As New Grayscale(0.2126, 0.07152, 0.1722)
        input = Image.Clone(input, PixelFormat.Format24bppRgb)
        input = gsFilter.Apply(input)
        Dim brightFilter As New BrightnessCorrection(15)
        input = brightFilter.Apply(input)
        Dim contrastFilter As New ContrastCorrection(25)
        input = contrastFilter.Apply(input)
        Return input
    End Function

    Public Function getHigherContrast(input As Bitmap, amount As Integer)
        Dim contrast As New ContrastCorrection(amount)
        input = Image.Clone(input, PixelFormat.Format24bppRgb)
        input = contrast.Apply(input)
        Return input
    End Function

    Public Function getEdges(input As Bitmap) As Bitmap
        Dim result As Bitmap = input
        Dim cannyEdgeDetTight As New CannyEdgeDetector(20, 50)

        If input IsNot Nothing Then
            input = getGrayScale(input)
            Dim input2 As Bitmap = input
            cannyEdgeDetTight.ApplyInPlace(input)
        End If
        Return input
    End Function

    Public Overrides ReadOnly Property FormatTranslations As Dictionary(Of PixelFormat, PixelFormat)
        Get
            Throw New NotImplementedException()
        End Get
    End Property

    Protected Overrides Sub ProcessFilter(sourceData As UnmanagedImage, destinationData As UnmanagedImage, rect As Rectangle)
        Throw New NotImplementedException()
    End Sub
End Class
