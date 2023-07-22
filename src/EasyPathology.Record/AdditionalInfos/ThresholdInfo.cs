using EasyPathology.Definitions.Interfaces;
using EasyPathology.Record.Interfaces;

namespace EasyPathology.Record.AdditionalInfos;

internal class ThresholdInfo : IAdditionalInfo {
	public uint GetLength(int version) => sizeof(double);

	public AdditionalInfoType Type => AdditionalInfoType.Threshold;

	public double Threshold { get; set; } = 30d;

	public void Read(BinaryReader br, int version) {
		this.BeginRead(br);
		Threshold = br.ReadDouble();
		this.EndRead(br);
	}

	public void Write(BinaryWriter bw, int version) {
		this.WriteAdditionalInfoHeader(bw, version);
		bw.Write(Threshold);
	}
}