using EasyPathology.Record.AdditionalInfos;
using EasyPathology.Record.Interfaces;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;

namespace EasyPathology.Record.Services;

/// <summary>
/// 从Excel中加载ClassifyTree，以前ep中的默认实现
/// </summary>
public class ExcelClassifyTreeService : IClassifyTreeService {
	public ClassifyTree Load(Stream stream, ClassifyTreeLoadParameter parameter) {
		var wk = new XSSFWorkbook(stream);
		var sheet = wk.GetSheetAt(0);

		var tree = new ClassifyTree();
		var root = new ClassifyTreeNode(0, "root", -1);
		var id = 1U;
		LoadChildren(tree, root, ref id, sheet, 0, 0, sheet.LastRowNum, parameter);
		foreach (var child in root.Children) {
			child.Parent = null;  // 清除掉root
			tree.RootItems.Add(child);
		}

		return tree;
	}

	/// <summary>
	/// <see cref="Load"/>的辅助方法，用于递归读取
	/// </summary>
	private static void LoadChildren(ClassifyTree tree, ClassifyTreeNode parent, ref uint id, ISheet sheet, int row, int column, int maxRow, ClassifyTreeLoadParameter parameter) {
		while (row < maxRow) {
			var (cell, height) = GetCellAndHeight(sheet, row, column);
			if (cell == null || height == 0) {
				return;
			}
			var name = cell.ToString()?.Replace("->", "→");
			var node = new ClassifyTreeNode(id++, name ?? Core.Strings.Resources.ClassifyTree_Empty, column) {
				Parent = parent
			};
			ReadCellProperty(cell, node);
			parent.Children.Add(node);
			tree.AddNode(node);
			if (height > 0) {
				LoadChildren(tree, node, ref id, sheet, row, column + 1, row + height, parameter);
			}
			row += height;
		}
	}

	/// <summary>
	/// <see cref="LoadChildren"/>的辅助方法，用于获取合并单元格的高度
	/// </summary>
	private static (ICell?, int) GetCellAndHeight(in ISheet sheet, int row, in int column) {
		var tempRow = sheet.GetRow(row);
		if (tempRow == null) {
			return (null, 0);
		}
		var cell = tempRow.GetCell(column);
		if (cell == null) {
			return (null, 0);
		}
		if (!cell.IsMergedCell) {
			return (cell, 1);
		}
		var height = 1;
		ICell tempCell;
		while ((tempRow = sheet.GetRow(row + height)) != null && ((tempCell = tempRow.GetCell(column)) == null || tempCell.CellType == CellType.Blank)) {
			height++;
		}
		return (cell, height);
	}

	public void Save(ClassifyTree classifyTree, Stream stream) {
		if (classifyTree.RootItems.Count == 0) {
			return;
		}
		var workbook = new XSSFWorkbook();
		var styleDict = new Dictionary<int, XSSFCellStyle>();
		var sheet = workbook.CreateSheet("main");
		var root = new ClassifyTreeNode(0, "root", -1);
		foreach (var treeItem in classifyTree.RootItems) {
			root.Children.Add(treeItem);
		}
		SaveNode(root, workbook, sheet, 0, 0, styleDict);
		workbook.Write(stream);
	}

	/// <summary>
	/// <see cref="Save"/>的辅助方法，用于递归存储
	/// </summary>
	private static int SaveNode(in ClassifyTreeNode node, in XSSFWorkbook wb, in ISheet sheet, int row, in int column, in Dictionary<int, XSSFCellStyle> styleDict) {
		foreach (var child in node.Children) {
			if (child.Children.Count > 0) {
				var endRow = SaveNode(child, wb, sheet, row, column + 1, styleDict);
				var iRow = sheet.GetRow(row) ?? sheet.CreateRow(row);
				var cell = iRow.CreateCell(column);
				cell.SetCellValue(child.Text);
				SetCellStyle(cell, child, wb, styleDict);
				if (endRow > row) {
					sheet.AddMergedRegion(new CellRangeAddress(row, endRow, column, column));
				}
				row = endRow + 1;
			} else {
				var iRow = sheet.GetRow(row) ?? sheet.CreateRow(row);
				var cell = iRow.CreateCell(column);
				cell.SetCellValue(child.Text);
				SetCellStyle(cell, child, wb, styleDict);
				row++;
			}
		}
		sheet.AutoSizeColumn(column);
		return row - 1;
	}

	private static void SetCellStyle(in ICell cell, in ClassifyTreeNode node, in XSSFWorkbook wb, in Dictionary<int, XSSFCellStyle> styleDict) {
		var color = node.Properties;
		if (!styleDict.TryGetValue(color, out var style)) {
			style = (XSSFCellStyle)wb.CreateCellStyle();
			style.SetFillBackgroundColor(new XSSFColor(BitConverter.GetBytes(color)));
			style.Alignment = HorizontalAlignment.Center;
			style.VerticalAlignment = VerticalAlignment.Center;
		}
		cell.CellStyle = style;
	}

	private static void ReadCellProperty(ICell cell, ClassifyTreeNode node) {
		if (cell.CellStyle.FillBackgroundColorColor is XSSFColor { RGB.Length: 3 } cellColor) {
			var color = new byte[] { 0, cellColor.RGB[0], cellColor.RGB[1], cellColor.RGB[2] };
			node.Properties = BitConverter.ToInt32(color);
		} else {
			node.Properties = 0;
		}
	}
}