using EasyPathology.Definitions.Interfaces;
using EasyPathology.Definitions.Models;
using EasyPathology.Record.Interfaces;

namespace EasyPathology.Record.AdditionalInfos;

public class DoctorDetailInfo : IAdditionalInfo {
	public AdditionalInfoType Type => AdditionalInfoType.DoctorDetailInfo;

	public uint GetLength(int version) => 
		Unit.GetStringSaveLength() + Introduction.GetStringSaveLength() + sizeof(int) + (uint)(AvatarData?.Length ?? 0);

	public string? Unit { get; set; }

	public string? Introduction { get; set; }

	/// <summary>
	/// 头像，jpg编码
	/// </summary>
	public byte[]? AvatarData { get; set; }

	public DoctorDetailInfo() { }

	public DoctorDetailInfo(DoctorDetail doctorDetail) {
		Unit = doctorDetail.Unit;
		Introduction = doctorDetail.Introduction;
		AvatarData = doctorDetail.AvatarData;
	}

	public void Read(BinaryReader br, int version) {
		this.BeginRead(br);
		Unit = br.ReadString();
		Introduction = br.ReadString();
		var length = br.ReadInt32();
		if (length > 0) {
			AvatarData = br.ReadBytes(length);
		}
		this.EndRead(br);
	}

	public void Write(BinaryWriter bw, int version) {
		this.WriteAdditionalInfoHeader(bw, version);
		bw.Write(Unit ?? string.Empty);
		bw.Write(Introduction ?? string.Empty);
		if (AvatarData is { Length: > 0 }) {
			bw.Write(AvatarData.Length);
			bw.Write(AvatarData);
		} else {
			bw.Write(0);
		}
	}
}