using System.Text;

namespace EasyPathology.Record.Interfaces;

/// <summary>
/// 目前使用一个byte，如果以后真的超过256种，可以拓展，即如果Type是0xFF，那就再往后读一字节
/// </summary>
public enum AdditionalInfoType : byte
{
    Unknown,
    Threshold,
    Roi,
    RoiList,

    SlideRulerTool,
    SlideComments,

    DiagnosisClassifyTree,
    DoctorDetailInfo,

	SlideMarkPenTool,

	SegmentedRoi,
	SegmentedRoiList,

	Next = 0xFF,
}

/// <summary>
/// <para>	文件尾部的附加信息								</para>
/// <para>	附加信息有不同的类型，但每段附加信息都是如下的构造	</para>
/// <para>	+--------+------+------+					</para>
/// <para>	| Length | Type | Data |					</para>
/// <para>	+--------+------+------+					</para>
/// <para>	|  uint  | byte |      |					</para>
/// <para>	+--------+------+------+					</para>
/// <para>	如果有动态数据长度，则需要动态获取				</para>
/// <para>	string按照UTF8编码							</para>
/// </summary>
public interface IAdditionalInfo : IBinaryReadWriteWithHeader {
	AdditionalInfoType Type { get; }
}

public static class AdditionalInfoUtils {
	public static void WriteChars(this BinaryWriter bw, string str) {
		bw.Write(Encoding.ASCII.GetBytes(str));
	}

	public static AdditionalInfoType ReadAdditionInfoType(this BinaryReader br) {
		try {
			return (AdditionalInfoType)br.ReadByte();
		} catch (InvalidCastException) {
			return AdditionalInfoType.Unknown;
		}
	}

	public static void CheckType(BinaryReader br, AdditionalInfoType type) {
		var readType = br.ReadAdditionInfoType();
		if (type != readType) {
			throw new AdditionalInfoTypeMismatchException(type, readType);
		}
	}

	public static void WriteAdditionalInfoHeader(this IAdditionalInfo additionalInfo, BinaryWriter bw, int version) {
#if DEBUG
		System.Diagnostics.Debug.Assert(additionalInfo != null);
#endif
		bw.Write((byte)additionalInfo.Type);
		bw.Write(additionalInfo.GetLength(version));
	}
}

