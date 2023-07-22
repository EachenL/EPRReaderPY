using System.Collections;
using EasyPathology.Definitions.DataTypes;
using OpenCvSharp;

namespace EasyPathology.Inference.Utils;

/// <summary>
/// 用于将一个图像切分成小patch
/// </summary>
public class ImagePatcher : IEnumerable<Mat>
{
    /// <summary>
    /// 要切分成多大的小Patch
    /// </summary>
    public Size2I PatchSize { get; }

    /// <summary>
    /// 每切分一次，在X和Y上移动多少
    /// </summary>
    public Point2I Step { get; }

    /// <summary>
    /// 水平方向能切分多少
    /// </summary>
    public int HorizontalCount { get; }

    /// <summary>
    /// 竖直方向能切分多少
    /// </summary>
    public int VerticalCount { get; }

    private readonly Mat input;
    private readonly int padRight, padBottom;

    public ImagePatcher(Mat input, Size2I patchSize, Point2I step)
    {
        this.input = input;
        PatchSize = patchSize;
        Step = step;

        if (input.Width <= patchSize.Width)
        {
            padRight = patchSize.Width - input.Width;
            HorizontalCount = 1;
        }
        else
        {
            padRight = step.Y - (input.Width - patchSize.Width) % step.X;
            HorizontalCount = (input.Width + padRight - patchSize.Width) / step.X + 1;
        }

        if (input.Height <= patchSize.Height)
        {
            padBottom = patchSize.Height - input.Height;
            VerticalCount = 1;
        }
        else
        {
            padBottom = step.Y - (input.Height - patchSize.Height) % step.Y;
            VerticalCount = (input.Height + padBottom - patchSize.Height) / step.Y + 1;
        }
    }

    public IEnumerator<Mat> GetEnumerator()
    {
        for (var y = 0; y < VerticalCount; y++)
        {
            for (var x = 0; x < HorizontalCount; x++)
            {
                var mat = new Mat(PatchSize.Height, PatchSize.Width, input.Type(), new Scalar());
				var rowStart = y * Step.Y;
                var colStart = x * Step.X;
                var srcHeight = y == VerticalCount - 1 ? PatchSize.Height - padBottom : PatchSize.Height;
                var srcWidth = x == HorizontalCount - 1 ? PatchSize.Width - padRight : PatchSize.Width;
                input.SubMat(rowStart, rowStart + srcHeight, colStart, colStart + srcWidth)
	                .CopyTo(mat.SubMat(0, srcHeight, 0, srcWidth));
                yield return mat;
            }
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}