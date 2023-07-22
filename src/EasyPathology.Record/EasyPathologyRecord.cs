using System.IO.MemoryMappedFiles;
using System.Runtime.CompilerServices;
using EasyPathology.Definitions.DataTypes;
using EasyPathology.Definitions.Models;
using EasyPathology.Record.AdditionalInfos;
using EasyPathology.Record.FrameStates;
using EasyPathology.Record.Interfaces;

namespace EasyPathology.Record;

public class InvalidEprFileException : Exception {
	public string Segment { get; }

	public InvalidEprFileException(string segment) {
		Segment = segment;
	}

	public InvalidEprFileException(string segment, Exception inner) : base("InvalidEPRFileException", inner) {
		Segment = segment;
	}

	public override string Message => $"无效EPR文件：{Segment}";
}

/// <summary>
/// EPR文件
/// </summary>
[Serializable]
public class EasyPathologyRecord {
	/// <summary>
	/// 当前最新版本号
	/// </summary>
	public const int CurrentVersion = 25;

	/// <summary>
	/// 文件创建时间
	/// </summary>
	public DateTime CreateDateTime { get; set; }

	/// <summary>
	/// 文件版本号
	/// </summary>
	public required int Version { get; set; }

	/// <summary>
	/// 每秒帧率
	/// </summary>
	public required int FramesPerSecond { get; set; }
	
	/// <summary>
	/// 文件标题
	/// </summary>
	public string? Title { get; set; }

	/// <summary>
	/// EPR文件的总帧数
	/// </summary>
	public int FrameCount => RawDataFrames.Length;

	/// <summary>
	/// EPR文件中记录的屏幕横向分辨率
	/// </summary>
	public int ScreenPixelWidth { get; set; }

	/// <summary>
	/// EPR文件中记录的屏幕纵向分辨率
	/// </summary>
	public int ScreenPixelHeight { get; set; }

	/// <summary>
	/// 刚进入阅片时的宽度
	/// </summary>
	public long InitialWidth { get; set; }

	/// <summary>
	/// 刚进入阅片时的高度
	/// </summary>
	public long InitialHeight { get; set; }

	/// <summary>
	/// 阅片时，最大倍率的宽度
	/// </summary>
	public long MaxLevelWidth { get; set; }

	/// <summary>
	/// 阅片时，最大倍率的高度
	/// </summary>
	public long MaxLevelHeight { get; set; }

	/// <summary>
	/// 切片文件的最大宽度
	/// </summary>
	public long MaxSlideWidth { get; set; }

	/// <summary>
	/// 切片文件的最大高度
	/// </summary>
	public long MaxSlideHeight { get; set; }

	/// <summary>
	/// 最大level，也就是刚进入阅片的level
	/// </summary>
	public int MaxLevel { get; private set; }

	/// <summary>
	/// 最小level
	/// </summary>
	public int MinLevel { get; set; }

	/// <summary>
	/// 医生总共看了多少个level
	/// </summary>
	public int TotalLevelCount => MaxLevel - MinLevel + 1;

	/// <summary>
	/// EPR文件中记录的屏幕物理宽度，单位是cm
	/// </summary>
	public float ActualScreenWidthCentimeter { get; set; }

	/// <summary>
	/// EPR文件中记录的屏幕物理高度，单位是cm
	/// </summary>
	public float ActualScreenHeightCentimeter { get; set; }

	/// <summary>
	/// EPR文件中记录的切片文件名，不含路径，带拓展名
	/// </summary>
	public required string SlideName { get; init; }

	/// <summary>
	/// EPR文件中记录的切片的快速哈希值
	/// </summary>
	public required string QuickHash { get; init; }

	/// <summary>
	/// EPR文件中记录的医生信息
	/// </summary>
	public required DoctorDetail DoctorDetail { get; init; }

	/// <summary>
	/// EPR文件中记录的医生注释
	/// </summary>
	public string? AdditionalComment { get; set; }

	/// <summary>
	/// EPR文件中记录的医生判断类型字符串
	/// </summary>
	public string? SelectedType { get; set; }

	public RawDataFrame[] RawDataFrames {
		get => rawDataFrames ?? DataFrames.Cast<RawDataFrame>().ToArray();
		set => rawDataFrames = value;
	}

	private RawDataFrame[]? rawDataFrames;

	/// <summary>
	/// 数据
	/// </summary>
	public DataFrame[] DataFrames { get; private set; } = Array.Empty<DataFrame>();

	/// <summary>
	/// 记录当前EPR文件的路径
	/// </summary>
	public string? EprFilePath { get; set; }

	/// <summary>
	/// 对应了每个VirtualLevel的缩放
	/// </summary>
	public int[] LevelScales {
		get {
			if (zoom == null) {
				zoom = new int[MaxLevel + 1];
				for (var i = 0; i < zoom.Length; i++) {
					zoom[i] = (int)Math.Pow(2, zoom.Length - i - 1);
				}
			}
			return zoom;
		}
	}

	private int[]? zoom;

	public AdditionalInfoSet AdditionalInfoSet { get; init; } = new();

	public TimeSpan TotalTime { get; set; }

	/// <summary>
	/// 读取EPR
	/// </summary>
	/// <param name="eprFilePath">要处理的epr文件路径</param>
	/// <param name="readHeaderOnly">只读取文件头，不读数据</param>
	public static EasyPathologyRecord LoadFromFile(string eprFilePath, bool readHeaderOnly = false) {
		// 使用内存映射加速读取
		using var fs = File.OpenRead(eprFilePath);
		using var mmf = MemoryMappedFile.CreateFromFile(fs, null, 0, MemoryMappedFileAccess.Read, HandleInheritability.None, false);
		using var stream = mmf.CreateViewStream(0, 0, MemoryMappedFileAccess.Read);
		using var br = new BinaryReader(stream);

		void CheckSegment(string segment) {
			if (segment.Any(c => c != (char)br.ReadByte())) {
				throw new InvalidEprFileException(segment);
			}
		}

		CheckSegment("EPRFILE");  // 检查文件头

		var version = br.ReadInt32();
		var fps = br.ReadInt32();
		var frameCount = br.ReadInt32();
		if (frameCount < 1) {
			throw new InvalidEprFileException("录像没有有效帧。");
		}
		
		string? title = null;
		if (version >= 25) {
			title = br.ReadString();
		}

		CheckSegment("DOCTOR");
		var name = br.ReadString();
		var gender = (Gender)br.ReadByte();
		var age = br.ReadUInt16();
		var year = br.ReadUInt16();
		var doctorLevel = (DoctorLevel)br.ReadInt16();
		var slideReadCount = (SlideReadCount)br.ReadInt16();
		var doctorGuid = version >= 24 ? new Guid(br.ReadBytes(16)) : Guid.Empty;

		CheckSegment("SLIDE");
		string slideName, quickHash;
		try {
			slideName = br.ReadString();
			quickHash = br.ReadString();
		} catch (Exception e) {
			throw new InvalidEprFileException("SLIDE", e);
		}

		var epr = new EasyPathologyRecord {
			Version = version,
			FramesPerSecond = fps,
			Title = title,
			DoctorDetail = new DoctorDetail {
				Name = name,
				Gender = gender,
				Age = age,
				WorkYear = year,
				Level = doctorLevel,
				SlideReadCount = slideReadCount,
				Guid = doctorGuid
			},
			SlideName = slideName,
			QuickHash = quickHash,
			CreateDateTime = new FileInfo(eprFilePath).CreationTimeUtc,
			EprFilePath = eprFilePath
		};

		try {
			if (version >= 21) {
				epr.MaxSlideWidth = br.ReadInt64();
				epr.MaxSlideHeight = br.ReadInt64();
			}
			epr.MaxLevelWidth = br.ReadInt64();
			epr.MaxLevelHeight = br.ReadInt64();
			epr.MinLevel = br.ReadInt32();
		} catch (Exception e) {
			throw new InvalidEprFileException("SLIDE", e);
		}

		int CheckScreenI(int i) {
			if (i <= 0) {
				throw new InvalidEprFileException("SCREEN");
			}

			return i;
		}

		float CheckScreenF(float i) {
			if (i <= 0 || float.IsSubnormal(i)) {
				throw new InvalidEprFileException("SCREEN");
			}

			return i;
		}

		CheckSegment("SCREEN");
		epr.ScreenPixelWidth = CheckScreenI(br.ReadInt32());
		epr.ScreenPixelHeight = CheckScreenI(br.ReadInt32());
		epr.ActualScreenWidthCentimeter = CheckScreenF(br.ReadSingle());
		epr.ActualScreenHeightCentimeter = CheckScreenF(br.ReadSingle());

		if (readHeaderOnly) {
			// 估算总时间
			epr.TotalTime = TimeSpan.FromSeconds((double)frameCount / fps);
		}

		// magic number
		br.BaseStream.Seek("114514 1919810e reserve ".Length, SeekOrigin.Current);

		CheckSegment("DATA");
		if (version >= 24) {
			var dataLength = br.ReadInt64();
			if (readHeaderOnly) {
				br.BaseStream.Seek(dataLength, SeekOrigin.Current);  // 跳过DATA区块
			}
		} else {
			if (readHeaderOnly) {
				return epr;
			}
		}

		RawDataFrame[]? rawDataFrames = null;
		if (!readHeaderOnly) {
			rawDataFrames = new RawDataFrame[frameCount];
			var dataFrameErrorCount = 0;
			var isFrameStatesError = false;
			for (var i = 0; i < frameCount; i++) {
				try {
					rawDataFrames[i] = ReadRawDataFrame(br, epr.Version, ref isFrameStatesError);
				} catch (Exception e) {
					throw new InvalidEprFileException("DATA", e);
				}

				if (isFrameStatesError && ++dataFrameErrorCount > 10) {
					throw new Exception("FrameStates错误计数大于10，停止读取。");
				}

				isFrameStatesError = false;
			}
		}

		CheckSegment("TAIL");
		try {
			epr.AdditionalComment = br.ReadString();
			if (epr.Version < 22) {
				br.BaseStream.Seek(2, SeekOrigin.Current); // 跳过type
			}
			epr.SelectedType = br.ReadString();
		} catch (Exception e) {
			throw new InvalidEprFileException("TAIL", e);
		}

		if (epr.Version > 22) {
			CheckSegment("ADDITION");
			epr.AdditionalInfoSet.Read(br, version);

			var doctorDetail = epr.AdditionalInfoSet.DoctorDetailInfo;
			epr.DoctorDetail.AvatarData = doctorDetail.AvatarData;
			epr.DoctorDetail.Introduction = doctorDetail.Introduction;
			epr.DoctorDetail.Unit = doctorDetail.Unit;
		} else {
			br.BaseStream.Seek(-3, SeekOrigin.End);
		}

		CheckSegment("END");

		if (readHeaderOnly) {
			return epr;
		}
		if (rawDataFrames == null) {
			return epr;
		}

		// 数据后处理

		// 先进行滤波
		//var kf = new KalmanFilter2D {
		//	X = epr.DataFrames[0].EyeX,
		//	Y = epr.DataFrames[0].EyeY
		//};
		//for (var i = 1; i < epr.FrameCount; i++) {
		//	if (epr.DataFrames[i - 1].Level != epr.DataFrames[i].Level) {
		//		kf.Clear();
		//		kf.X = epr.DataFrames[i].EyeX;
		//		kf.Y = epr.DataFrames[i].EyeY;
		//	} else {
		//		kf.Push(epr.DataFrames[i].EyeX, epr.DataFrames[i].EyeY);
		//		epr.DataFrames[i].EyeX = (int)kf.X;
		//		epr.DataFrames[i].EyeY = (int)kf.Y;
		//	}
		//}

		double mpp = (epr.ActualScreenWidthCentimeter / epr.ScreenPixelWidth + epr.ActualScreenHeightCentimeter / epr.ScreenPixelHeight) / 2;
		var firstDataTimestamp = rawDataFrames[0].TimeStamp;
		var dataFrames = new DataFrame[frameCount];
		RawDataFrame? prevRawDataFrame = null;

		for (var index = 0; index < frameCount; index++) {
			var currentRawDataFrame = rawDataFrames[index];

			epr.MaxLevel = Math.Max(epr.MaxLevel, currentRawDataFrame.VirtualLevel);
			
			#region BeginTime和Duration

			var beginTime = TimeSpan.FromSeconds(currentRawDataFrame.TimeStamp - firstDataTimestamp);
			TimeSpan duration;
			if (index == frameCount - 1) {
				duration = TimeSpan.FromSeconds(1d / fps);
			} else {
				duration = TimeSpan.FromSeconds(rawDataFrames[index + 1].TimeStamp - currentRawDataFrame.TimeStamp);
			}

			#endregion

			#region FrameStates和IsFrameStateEqualsToPrevFrame

			IFrameState[] frameStates;
			var isFrameStateEqualsToPrevFrame = new bool[currentRawDataFrame.FrameStates.Length];
			if (prevRawDataFrame is { FrameStates.Length: > 0 }) {
				for (var j = 0; j < currentRawDataFrame.FrameStates.Length; j++) {
					if (prevRawDataFrame.FrameStates.Any(frameState => frameState.Equals(currentRawDataFrame.FrameStates[j]))) {
						isFrameStateEqualsToPrevFrame[j] = true;
					}
				}
			}
			if (isFrameStateEqualsToPrevFrame.All(static b => b) && prevRawDataFrame != null) {
				frameStates = prevRawDataFrame.FrameStates;  // 如果完全一样，就指向同一个对象
			} else {
				frameStates = currentRawDataFrame.FrameStates;
			}

			#endregion

			#region EyeData和Radius，需要插值

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			double CalculateRadius(RawDataFrame rawDataFrame) {
				return rawDataFrame.EyeDistanceToScreen * Math.Tan(3d / 180d) / mpp;
			}

			Point2D gazePosition;
			Point3F leftEyePosition, rightEyePosition;
			double radius;

			if (currentRawDataFrame.IsEyeTrackerDataValid) {
				gazePosition = new Point2D(currentRawDataFrame.GazeX, currentRawDataFrame.GazeY);
				leftEyePosition = currentRawDataFrame.LeftEyePosition;
				rightEyePosition = currentRawDataFrame.RightEyePosition;
				radius = CalculateRadius(currentRawDataFrame);
			} else {
				gazePosition = new Point2D();
				leftEyePosition = new Point3F();
				rightEyePosition = new Point3F();
				radius = 0d;
			}

			#endregion

			#region Velocity

			double velocity;
			if (index == frameCount - 1 || currentRawDataFrame.VirtualLevel != rawDataFrames[index + 1].VirtualLevel) {
				// Level不同或者到了最后一帧，无法计算速度，继承之前的速度
				velocity = index == 0 ? 0d : dataFrames[index - 1].Velocity;
			} else {
				var nextDataFrame = rawDataFrames[index + 1];
				if (!currentRawDataFrame.IsEyeTrackerDataValid && !nextDataFrame.IsEyeTrackerDataValid) {
					// 没有数据，那就记为0
					velocity = 0d;
				} else {
					var l = epr.LevelScales[currentRawDataFrame.VirtualLevel];
					double h;
					if (!currentRawDataFrame.IsEyeTrackerDataValid) {
						h = nextDataFrame.EyeDistanceToScreen;
					} else {
						if (!nextDataFrame.IsEyeTrackerDataValid) {
							h = currentRawDataFrame.EyeDistanceToScreen;
						} else {
							h = (currentRawDataFrame.EyeDistanceToScreen + nextDataFrame.EyeDistanceToScreen) / 2;
						}
					}

					[MethodImpl(MethodImplOptions.AggressiveInlining)]
					static double Distance(double x1, double y1, double x2, double y2) => SqrtRoot(x1 - x2, y1 - y2);

					[MethodImpl(MethodImplOptions.AggressiveInlining)]
					static double SqrtRoot(double a, double b) => Math.Sqrt(a * a + b * b);

					var sx = epr.ScreenPixelWidth / 2 - (currentRawDataFrame.ScreenX + nextDataFrame.ScreenX) / 2; // 屏幕中心坐标
					var sy = epr.ScreenPixelHeight / 2 - (currentRawDataFrame.ScreenY + nextDataFrame.ScreenY) / 2;
					var ac = Distance(currentRawDataFrame.GazeX, currentRawDataFrame.GazeY, sx, sy) * mpp;
					var ad = SqrtRoot(ac, h) / l;
					var bc = Distance(nextDataFrame.GazeX, nextDataFrame.GazeY, sx, sy) * mpp;
					var bd = SqrtRoot(bc, h) / l;
					var ab = Distance(currentRawDataFrame.GazeX, currentRawDataFrame.GazeY, nextDataFrame.GazeX, nextDataFrame.GazeY) * mpp;
					var r = (ad * ad + bd * bd - ab * ab) / (2 * ad * bd);
					r = r switch {
						> 1 => 1,
						< 0 => 0,
						_ => r
					};
					var angle = Math.Acos(r);
					var rad = angle / (nextDataFrame.TimeStamp - currentRawDataFrame.TimeStamp);
					velocity = 180 * rad / Math.PI;
				}
			}

			#endregion

			dataFrames[index] = new DataFrame(
				currentRawDataFrame,
				beginTime,
				duration,
				gazePosition,
				leftEyePosition,
				rightEyePosition,
				radius,
				currentRawDataFrame.IsEyeTrackerDataValid ? 0 : 1,
				velocity,
				index,
				frameStates,
				isFrameStateEqualsToPrevFrame);

			prevRawDataFrame = currentRawDataFrame;
		}

		epr.DataFrames = dataFrames;

		var minLevelZoom = epr.LevelScales[epr.MinLevel];
		epr.InitialWidth = epr.MaxLevelWidth / minLevelZoom;
		epr.InitialHeight = epr.MaxLevelHeight / minLevelZoom;
		if (epr.Version >= 23) {
			// 这里有坑！录像时有可能丢帧了！导致epr.FrameCount / epr.Fps并不一定是真的时长，版本23之后的Timestamp是真实的时间戳，应该通过这个来取得时长
			epr.TotalTime = epr.DataFrames[^1].BeginTime.Add(TimeSpan.FromSeconds(1d / epr.FramesPerSecond));
		} else {
			epr.TotalTime = TimeSpan.FromSeconds((double)epr.FrameCount / epr.FramesPerSecond);
		}

		return epr;
	}

	private static void WriteRawDataFrame(RawDataFrame rawDataFrame, BinaryWriter bw, int version) {
		bw.Write(rawDataFrame.ScreenX);
		bw.Write(rawDataFrame.ScreenY);
		bw.Write(rawDataFrame.VirtualLevel);
		bw.Write(rawDataFrame.GazeX);
		bw.Write(rawDataFrame.GazeY);

		if (version >= 24) {
			bw.Write(rawDataFrame.LeftEyePosition.X);
			bw.Write(rawDataFrame.LeftEyePosition.Y);
			bw.Write(rawDataFrame.LeftEyePosition.Z);
			bw.Write(rawDataFrame.RightEyePosition.X);
			bw.Write(rawDataFrame.RightEyePosition.Y);
			bw.Write(rawDataFrame.RightEyePosition.Z);
			bw.Write(rawDataFrame.IsEyeTrackerDataValid);
		} else {
			bw.Write(rawDataFrame.EyeDistanceToScreen);
		}

		bw.Write(rawDataFrame.TimeStamp);

		if (version >= 23) {
			bw.Write(rawDataFrame.CursorX);
			bw.Write(rawDataFrame.CursorY);
			var length = (uint)rawDataFrame.FrameStates.Sum(frameState => frameState.GetLength(version));
			bw.Write(length);
			if (length > 0) {
				bw.Write((uint)rawDataFrame.FrameStates.Length);
				foreach (var frameState in rawDataFrame.FrameStates) {
					frameState.Write(bw, version);
				}
			}
		}
	}

	private static RawDataFrame ReadRawDataFrame(BinaryReader br, int version, ref bool isFrameStatesError) {
		var screenX = br.ReadInt32();
		var screenY = br.ReadInt32();
		var virtualLevel = br.ReadInt32();
		var eyeX = br.ReadInt32();
		var eyeY = br.ReadInt32();
		Point3F leftEyePosition, rightEyePosition;
		bool isEyeTrackerDataValid;

		if (version >= 24) {
			leftEyePosition = new Point3F(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
			rightEyePosition = new Point3F(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
			isEyeTrackerDataValid = br.ReadBoolean();
		} else {
			var eyeDistanceToScreen = (float)br.ReadDouble();
			if (eyeDistanceToScreen > 0) {
				isEyeTrackerDataValid = true;
			} else {
				isEyeTrackerDataValid = false;
				eyeDistanceToScreen = -eyeDistanceToScreen;
			}

			leftEyePosition = rightEyePosition = new Point3F(0f, 0f, eyeDistanceToScreen * 10);
		}

		var timeStamp = br.ReadDouble();
		int cursorX, cursorY;
		var frameStates = Array.Empty<IFrameState>();

		if (version >= 23) {
			cursorX = br.ReadInt32();
			cursorY = br.ReadInt32();
			var length = br.ReadUInt32();
			if (length > 0) {
				var stateCount = br.ReadUInt32();
				if (stateCount > 0) {
					var pos = br.BaseStream.Position;
					try {
						frameStates = new IFrameState[(int)stateCount];
						for (var i = 0; i < stateCount; i++) {
							frameStates[i] = br.ReadFrameState(version);
						}
#if DEBUG
#pragma warning disable CS0168
					} catch (Exception e) {
#pragma warning restore CS0168
						System.Diagnostics.Debugger.Break();
#else
					} catch {
#endif
						// 需要跳过的长度
						// stateCount * IFrameState.HeaderLength：所有IFrameState的Header的长度
						// length: 所有IFrameState不含Header的长度
						var skipBytes = pos + stateCount * IFrameState.HeaderLength + length - br.BaseStream.Position;
						br.BaseStream.Seek(skipBytes, SeekOrigin.Current); // 读取错误，跳过

						isFrameStatesError = true;
					}
				}
			}
		} else {
			cursorX = 0;
			cursorY = 0;
		}

		return new RawDataFrame(
			eyeX,
			eyeY,
			virtualLevel,
			screenX,
			screenY,
			leftEyePosition,
			rightEyePosition,
			isEyeTrackerDataValid,
			timeStamp,
			cursorX,
			cursorY,
			frameStates);
	}

	public void Save(Stream stream, int version = CurrentVersion) {
		using var bw = new BinaryWriter(stream);

		bw.WriteChars("EPRFILE");
		bw.Write(version);
		bw.Write(FramesPerSecond);
		bw.Write(FrameCount);
		if (version >= 25) {
			bw.Write(Title ?? string.Empty);
		}

		bw.WriteChars("DOCTOR");
		bw.Write(DoctorDetail.Name ?? string.Empty);
		bw.Write((byte)DoctorDetail.Gender);
		bw.Write(DoctorDetail.Age);
		bw.Write(DoctorDetail.WorkYear);
		bw.Write((ushort)DoctorDetail.Level);
		bw.Write((ushort)DoctorDetail.SlideReadCount);
		if (version >= 24) {
			bw.Write(DoctorDetail.Guid.ToByteArray());
		}

		bw.WriteChars("SLIDE");
		bw.Write(SlideName);
		bw.Write(QuickHash);
		bw.Write(MaxSlideWidth);
		bw.Write(MaxSlideHeight);
		bw.Write(MaxLevelWidth);
		bw.Write(MaxLevelHeight);
		bw.Write(MinLevel);

		bw.WriteChars("SCREEN");
		bw.Write(ScreenPixelWidth);
		bw.Write(ScreenPixelHeight);
		bw.Write(ActualScreenWidthCentimeter);
		bw.Write(ActualScreenHeightCentimeter);
		bw.WriteChars("114514 1919810e reserve ");

		bw.WriteChars("DATA");
		var lengthPosition = bw.BaseStream.Position;
		if (version >= 24) {
			bw.BaseStream.Seek(sizeof(long), SeekOrigin.Current);
		}
		foreach (var rawDataFrame in RawDataFrames) {
			WriteRawDataFrame(rawDataFrame, bw, version);
		}
		if (version >= 24) {
			var dataEndPosition = bw.BaseStream.Position;
			bw.BaseStream.Seek(lengthPosition, SeekOrigin.Begin);
			bw.Write(dataEndPosition - lengthPosition - sizeof(long));
			bw.BaseStream.Seek(dataEndPosition, SeekOrigin.Begin);
		}

		bw.WriteChars("TAIL");
		bw.Write(AdditionalComment ?? string.Empty);
		bw.Write(SelectedType ?? string.Empty);

		bw.WriteChars("ADDITION");
		AdditionalInfoSet.DoctorDetailInfo = new DoctorDetailInfo(DoctorDetail);
		AdditionalInfoSet.Write(bw, version);

		bw.WriteChars("END");
	}
}