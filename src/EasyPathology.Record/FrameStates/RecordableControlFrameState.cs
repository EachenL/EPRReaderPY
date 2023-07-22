using EasyPathology.Definitions.DataTypes;
using EasyPathology.Record.Interfaces;
using System.Text.Json;
using EasyPathology.Definitions.Interfaces;

namespace EasyPathology.Record.FrameStates;

/// <summary>
/// 这个类记录RecordableControl的各项信息，比如窗口位置，是否可视
/// </summary>
public abstract class RecordableControlFrameState : IFrameState {
	public virtual uint GetLength(int version) {
		var size = IsVisible ? (sizeof(bool) + 2U * sizeof(int)) : sizeof(bool);
		if (IsVisible) {
			if (version >= 24) {
				size += sizeof(byte);
			}
		}

		return size;
	}

	public abstract FrameStateType Type { get; }

	public bool IsVisible { get; set; }
	public Point2I Location { get; set; }  // 存储时都存储成int，省空间
	public byte ZIndex { get; set; }

	public virtual void Read(BinaryReader br, int version) {
		IsVisible = br.ReadBoolean();
		if (IsVisible) {
			Location = new Point2I(br.ReadInt32(), br.ReadInt32());
			if (version >= 24) {
				ZIndex = br.ReadByte();
			}
		}
	}

	public virtual void Write(BinaryWriter bw, int version) {
		bw.Write(IsVisible);
		if (IsVisible) {
			bw.Write(Location.X);
			bw.Write(Location.Y);
			if (version >= 24) {
				bw.Write(ZIndex);
			}
		}
	}

	public virtual bool Equals(IFrameState? other) => (other is RecordableControlFrameState rws) && (IsVisible == rws.IsVisible) && (Location == rws.Location);

	public override string ToString() => JsonSerializer.Serialize(this, new JsonSerializerOptions {WriteIndented = false});
}