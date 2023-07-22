namespace EasyPathology.Inference.Utils; 

/// <summary>
/// 将imagePatch重新拼接成大图像
/// </summary>
public class ImageUnpatcher {
	/// <summary>
	/// 小Patch的大小
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

	private readonly Mat output, weight;
	private readonly int padRight, padBottom;
	private int x, y;

	public ImageUnpatcher(Mat output, Size2I patchSize, Point2I step) {
		this.output = output;
		weight = new Mat(output.Rows, output.Cols, output.Type());

		PatchSize = patchSize;
		Step = step;

		if (output.Width <= patchSize.Width) {
			padRight = patchSize.Width - output.Width;
			HorizontalCount = 1;
		} else {
			padRight = step.Y - (output.Width - patchSize.Width) % step.X;
			HorizontalCount = (output.Width + padRight - patchSize.Width) / step.X + 1;
		}

		if (output.Height <= patchSize.Height) {
			padBottom = patchSize.Height - output.Height;
			VerticalCount = 1;
		} else {
			padBottom = step.Y - (output.Height - patchSize.Height) % step.Y;
			VerticalCount = (output.Height + padBottom - patchSize.Height) / step.Y + 1;
		}
	}

	public void Add(Mat patch) {
		if (x == -1 || y == -1) {
			throw new IndexOutOfRangeException();
		}

		var rowStart = y * Step.Y;
		var colStart = x * Step.X;
		var dstHeight = y == VerticalCount - 1 ? PatchSize.Height - padBottom : PatchSize.Height;
		var dstWidth = x == HorizontalCount - 1 ? PatchSize.Width - padRight : PatchSize.Width;

		using (var dstMat = output.SubMat(rowStart, rowStart + dstHeight, colStart, colStart + dstWidth)) {
			(dstMat + patch.SubMat(0, dstHeight, 0, dstWidth)).ToMat().CopyTo(dstMat);
		}

		// Cv2.ImShow("Preview", output);
		// Cv2.WaitKey(1);

		using (var dstWeight = weight.SubMat(rowStart, rowStart + dstHeight, colStart, colStart + dstWidth)) {
			(dstWeight + 1f).ToMat().CopyTo(dstWeight);
		}

		if (x == HorizontalCount - 1) {
			if (y == VerticalCount - 1) {
				(output / weight).ToMat().CopyTo(output);
				x = y = -1;
			}

			x = 0;
			y++;
		} else {
			x++;
		}
	}
}