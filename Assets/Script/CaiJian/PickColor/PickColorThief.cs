using System;
using System.Collections.Generic;
// using System.Drawing;
using System.Linq;
using UnityEngine;
using ColorThief;

public class PickColorThief
{
    protected const int DefaultColorCount = 5;
    protected const int DefaultQuality = 10;
    protected const bool DefaultIgnoreWhite = true;

    /// <summary>
    ///     Use the median cut algorithm to cluster similar colors and return the base color from the largest cluster.
    /// </summary>
    /// <param name="pixels">The source image pixels.</param>
    /// <param name="quality">
    ///     0 is the highest quality settings. 10 is the default. There is
    ///     a trade-off between quality and speed. The bigger the number,
    ///     the faster a color will be returned but the greater the
    ///     likelihood that it will not be the visually most dominant color.
    /// </param>
    /// <param name="ignoreWhite">if set to <c>true</c> [ignore white].</param>
    /// <returns></returns>
    public QuantizedColor GetColor(int w, int h, Color32[] pixels, int quality = DefaultQuality, bool ignoreWhite = DefaultIgnoreWhite)
    {
        var palette = GetPalette(w, h, pixels, DefaultColorCount, quality, ignoreWhite);
        var dominantColor = palette.FirstOrDefault();
        return dominantColor;
    }

    /// <summary>
    ///     Use the median cut algorithm to cluster similar colors.
    /// </summary>
    /// <param name="pixels">The source image pixels.</param>
    /// <param name="colorCount">The color count.</param>
    /// <param name="quality">
    ///     0 is the highest quality settings. 10 is the default. There is
    ///     a trade-off between quality and speed. The bigger the number,
    ///     the faster a color will be returned but the greater the
    ///     likelihood that it will not be the visually most dominant color.
    /// </param>
    /// <param name="ignoreWhite">if set to <c>true</c> [ignore white].</param>
    /// <returns></returns>
    /// <code>true</code>
    public List<QuantizedColor> GetPalette(int w,int h,Color32[] pixels, int colorCount = DefaultColorCount, int quality = DefaultQuality, bool ignoreWhite = DefaultIgnoreWhite)
    {
        var cmap = GetColorMap(w,h, pixels, colorCount, quality, ignoreWhite);
        return cmap != null ? cmap.GeneratePalette() : new List<QuantizedColor>();
    }

    /// <summary>
    ///     Use the median cut algorithm to cluster similar colors.
    /// </summary>
    /// <param name="pixels">The source image pixels.</param>
    /// <param name="colorCount">The color count.</param>
    /// <param name="quality">
    ///     0 is the highest quality settings. 10 is the default. There is
    ///     a trade-off between quality and speed. The bigger the number,
    ///     the faster a color will be returned but the greater the
    ///     likelihood that it will not be the visually most dominant color.
    /// </param>
    /// <param name="ignoreWhite">if set to <c>true</c> [ignore white].</param>
    /// <returns></returns>
    private CMap GetColorMap(int w, int h, Color32[] pixels, int colorCount, int quality = DefaultQuality, bool ignoreWhite = DefaultIgnoreWhite)
    {
        var pixelArray = GetPixelsFast(w,h, pixels, quality, ignoreWhite);

        // Send array to quantize function which clusters values using median
        // cut algorithm
        var cmap = Mmcq.Quantize(pixelArray, colorCount);
        return cmap;
    }

    private IEnumerable<int> GetIntFromPixel(Color32[] pixels)
    {
        for(int i=0; i< pixels.Length; i++)
        {
            Color32 clr = pixels[i];
            yield return clr.b;
            yield return clr.g;
            yield return clr.r;
            yield return clr.a;
        }
    }

    private int[][] GetPixelsFast(int w, int h, Color32[] pixels, int quality, bool ignoreWhite)
    {
        //var imageData = GetIntFromPixel(pixels);
        //var pls = imageData.ToArray();
        
        int [] pls = new int[pixels.Length * 4];
        for (int i = 0; i < pixels.Length; i+=4)
        {
            pls[i] = pixels[i].b;
            pls[i+1] = pixels[i].g;
            pls[i+2] = pixels[i].r;
            pls[i+3] = pixels[i].a;
        }

    
        var pixelCount = w * h;

        const int colorDepth = 4;

        var expectedDataLength = pixelCount * colorDepth;
        if(expectedDataLength != pls.Length)
        {
            throw new ArgumentException("(expectedDataLength = "
                                        + expectedDataLength + ") != (pixels.length = "
                                        + pls.Length + ")");
        }

        // Store the RGB values in an array format suitable for quantize
        // function

        // numRegardedPixels must be rounded up to avoid an
        // ArrayIndexOutOfBoundsException if all pixels are good.

        var numRegardedPixels = (quality <= 0) ? 0 : (pixelCount + quality - 1) / quality;

        var numUsedPixels = 0;
        var pixelArray = new int[numRegardedPixels][];

        for(var i = 0; i < pixelCount; i += quality)
        {
            var offset = i * 4;
            var b = pls[offset];
            var g = pls[offset + 1];
            var r = pls[offset + 2];
            var a = pls[offset + 3];

            // If pixel is mostly opaque and not white
            if(a >= 125 && !(ignoreWhite && r > 250 && g > 250 && b > 250))
            {
                pixelArray[numUsedPixels] = new[] {r, g, b};
                numUsedPixels++;
            }
        }

        // Remove unused pixels from the array
        var copy = new int[numUsedPixels][];
        Array.Copy(pixelArray, copy, numUsedPixels);
        return copy;
    }

    public List<UnityEngine.Color> GetPaletteColorList(int w, int h, Color32[] pixels, int colorCount = DefaultColorCount, int quality = DefaultQuality, bool ignoreWhite = DefaultIgnoreWhite)
    {
        List<UnityEngine.Color> paletteColorList = new List<UnityEngine.Color>();

        List<QuantizedColor> colors = GetPalette(w,h, pixels, colorCount, quality, ignoreWhite);
        for (int i = 0; i < colors.Count; i++)
        {
            paletteColorList.Add(colors[i].UnityColor);
        }

        return paletteColorList;
    }
}