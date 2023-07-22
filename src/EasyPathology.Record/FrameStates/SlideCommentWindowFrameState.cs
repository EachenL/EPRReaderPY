using EasyPathology.Definitions.Interfaces;
using EasyPathology.Record.Interfaces;
using System.Text.Json;

namespace EasyPathology.Record.FrameStates;

public class SlideCommentWindowFrameState : RecordableControlFrameState {
	public override FrameStateType Type => FrameStateType.SlideCommentWindow;

	public override uint GetLength(int version) {
		return (IsVisible ? 6U * sizeof(uint) + 4U * sizeof(double) + sizeof(int) : 0U) + base.GetLength(version);
	}

	public uint SelectedListId { get; set; }
	public uint SelectedListLength { get; set; }
	public uint ExpandedListId { get; set; }
	public uint ExpandedListLength { get; set; }
	public uint CommentListId { get; set; }
	public uint CommentListLength { get; set; }
	public int SelectedItemId { get; set; }
	public double TreeViewScrollViewerVOffset { get; set; }
	public double ListViewScrollViewerVOffset { get; set; }
	public double TreeViewScrollViewerHOffset { get; set; }
	public double ListViewScrollViewerHOffset { get; set; }

	public override void Read(BinaryReader br, int version) {
		this.BeginRead(br);
		base.Read(br, version);
		if (IsVisible) {
			SelectedListId = br.ReadUInt32();
			SelectedListLength = br.ReadUInt32();
			ExpandedListId = br.ReadUInt32();
			ExpandedListLength = br.ReadUInt32();
			TreeViewScrollViewerVOffset = br.ReadDouble();
			TreeViewScrollViewerHOffset = br.ReadDouble();
			ListViewScrollViewerVOffset = br.ReadDouble();
			ListViewScrollViewerHOffset = br.ReadDouble();
			CommentListId = br.ReadUInt32();
			CommentListLength = br.ReadUInt32();
			SelectedItemId = br.ReadInt32();
		}
		this.EndRead(br);
	}

	public override void Write(BinaryWriter bw, int version) {
		this.WriteFrameStateHeader(bw, version);
		base.Write(bw, version);
		if (IsVisible) {
			bw.Write(SelectedListId);
			bw.Write(SelectedListLength);
			bw.Write(ExpandedListId);
			bw.Write(ExpandedListLength);
			bw.Write(TreeViewScrollViewerVOffset);
			bw.Write(TreeViewScrollViewerHOffset);
			bw.Write(ListViewScrollViewerVOffset);
			bw.Write(ListViewScrollViewerHOffset);
			bw.Write(CommentListId);
			bw.Write(CommentListLength);
			bw.Write(SelectedItemId);
		}
	}

	public override bool Equals(IFrameState? other) =>
		base.Equals(other) && (other is SlideCommentWindowFrameState scs) &&
		(SelectedListId == scs.SelectedListId) && (SelectedListLength == scs.SelectedListLength) &&
		(ExpandedListId == scs.ExpandedListId) && (ExpandedListLength == scs.ExpandedListLength) &&
		TreeViewScrollViewerVOffset.Equals(scs.TreeViewScrollViewerVOffset) && TreeViewScrollViewerHOffset.Equals(scs.TreeViewScrollViewerHOffset) &&
		ListViewScrollViewerVOffset.Equals(scs.ListViewScrollViewerVOffset) && ListViewScrollViewerHOffset.Equals(scs.ListViewScrollViewerHOffset) &&
		(CommentListId == scs.CommentListId) && (CommentListLength == scs.CommentListLength) &&
		(SelectedItemId == scs.SelectedItemId);

	public override string ToString() => JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = false });
}