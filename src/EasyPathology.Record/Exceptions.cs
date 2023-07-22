using EasyPathology.Record.Interfaces;

namespace EasyPathology.Record;

public class BinaryReadLengthMismatchException : Exception {
	public readonly uint ExpectLength;
	public readonly uint ReadLength;

	public BinaryReadLengthMismatchException(uint expectLength, uint readLength) {
		ExpectLength = expectLength;
		ReadLength = readLength;
	}

	public override string Message => $"读取数据时发生错误：长度不正确: {ReadLength}，应为{ExpectLength}";
}

public class AdditionalInfoTypeUnknownException : Exception {
	private readonly AdditionalInfoType additionalInfoType;

	public AdditionalInfoTypeUnknownException(AdditionalInfoType additionalInfoType) {
		this.additionalInfoType = additionalInfoType;
	}

	public override string Message => "未知的AdditionalInfoType：" + additionalInfoType;
}

public class AdditionalInfoLengthMismatchException : Exception {
	public readonly uint ExpectLength;
	public readonly uint ReadLength;

	public AdditionalInfoLengthMismatchException(uint expectLength, uint readLength) {
		ExpectLength = expectLength;
		ReadLength = readLength;
	}

	public override string Message => $"读取AdditionalInfo时发生错误：Length长度不符，读取长度为{ReadLength}，应该为{ExpectLength}";
}

public class AdditionalInfoTypeMismatchException : Exception {
	public readonly AdditionalInfoType ExpectType;
	public readonly AdditionalInfoType ReadType;

	public AdditionalInfoTypeMismatchException(AdditionalInfoType expectType, AdditionalInfoType readType) {
		this.ExpectType = expectType;
		this.ReadType = readType;
	}

	public override string Message => $"读取AdditionalInfo时发生错误：意料之外的Type: {ReadType}，应为{ExpectType}";
}

public class FrameStateTypeUnknownException : Exception {
	public override string Message => "未知的AdditionalInfoType";
}