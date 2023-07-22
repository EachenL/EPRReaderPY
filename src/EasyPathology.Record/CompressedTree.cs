using EasyPathology.Record.Interfaces;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace EasyPathology.Record;

internal class CompressTreeNode<TData> where TData : IBinaryReadWriteWithHeader, new() {
	public uint id;
	public readonly TData? value;
	public readonly CompressTreeNode<TData>? parent;
	public readonly Dictionary<TData, CompressTreeNode<TData>> children = new();

	public CompressTreeNode() { }

	public CompressTreeNode(CompressTreeNode<TData> parent, TData value) {
		this.parent = parent;
		this.value = value;
	}

	public override string? ToString() => value?.ToString();
}

/// <summary>
/// 压缩的树形结构，本质上类似字典树
/// 例如我们有如下的几种列表
/// 1		1,1
/// 1,2		1,2
/// 1,2,3	1,3
/// 1,3		2,2
/// 3		3,1
/// 那么存储时，数据部分为
/// 123133
/// 如果要找1,2列表，只需知道起始和结束即可，这里为1,2。同理3列表为3,1
/// </summary>
public class CompressedTree<TData> where TData : IBinaryReadWriteWithHeader, new() {
	private readonly CompressTreeNode<TData> root = new();
	private uint nowId;

	public uint Length => length + sizeof(uint) * (1U + nowId);
	private uint length;

	/// <summary>
	/// 读取时数据存放在这里
	/// </summary>
	private TData[][]? data;

	/// <summary>
	/// 将一个序列存入，返回该序列的编号
	/// <para>如果是读取进来的，那不能直接调用该方法，因为为了加速解析，此时的数据处于<see cref="data"/>中</para>
	/// <para>如果要进行编辑，要调用<see cref="EnableEdit"/>将Data数据重新解析成树形结构</para>
	/// </summary>
	/// <param name="list"></param>
	/// <param name="version"></param>
	private uint AddList(IReadOnlyCollection<TData> list, int version) {
		Debug.Assert(data == null);  // 如果读取了数据，就不支持修改（目前还没做）
		if (list.Count == 0) {
			return 0;
		}
		var node = root;
		uint oldLengthTemp = 0U, oldLength = 0U, newLength = 0U, newId = 0U;
		foreach (var item in list) {
			if (node.id != 0) {
				newId = node.id;
			}
			var parent = node;
			if (node.children.Count > 0) {
				if (node.children.TryGetValue(item, out node)) {
					oldLengthTemp += item.GetLength(version);
					continue;
				}
				newId = 0;
				oldLength = oldLengthTemp;
			}
			node = new CompressTreeNode<TData>(parent, item);
			parent.children.Add(item, node);
			newLength += item.GetLength(version);
		}
		length += oldLength + newLength;
		Debug.Assert(node != root);
		if (node.id == 0) {
			if (newId == 0) {
				node.id = ++nowId;
			} else {
				node.id = newId;
			}
		}
		return node.id;
	}

	public void Write(BinaryWriter bw, int version) {
		if (data != null) {
			WriteData(bw, version);
			return;
		}

		bw.Write(nowId);
		if (nowId == 0) {
			return;
		}

		// 先写入到MemoryStream
		var listLengths = new uint[nowId];
		var memStreams = new MemoryStream[nowId];
		for (var i = 0; i < nowId; i++) {
			memStreams[i] = new MemoryStream();
		}
		var memWriters = new BinaryWriter[nowId];
		for (var i = 0; i < nowId; i++) {
			memWriters[i] = new BinaryWriter(memStreams[i]);
		}
		WriteInternal(listLengths, memWriters, root, version);

		// 再写入bw
		foreach (var listLength in listLengths) {
			bw.Write(listLength);
		}
		foreach (var memStream in memStreams) {
			memStream.Seek(0, SeekOrigin.Begin);
			memStream.CopyTo(bw.BaseStream);
			memStream.Dispose();
		}
	}

	private void WriteInternal(IList<uint> listLengths, IReadOnlyList<BinaryWriter> memWriters, CompressTreeNode<TData> node, int version) {
		if (node.children.Count == 0) {  // 到末尾了，开始回溯
			var index = (int)(node.id - 1);
			var bw = memWriters[index];
			var stack = new Stack<TData>();
			while (node != root) {
				Debug.Assert(node.parent != null && node.value != null);
				stack.Push(node.value);
				node = node.parent;
			}
			listLengths[index] = (uint)stack.Count;
			while (stack.Count > 0) {
				stack.Pop().Write(bw, version);
			}
			return;
		}
		foreach (var child in node.children.Values) {
			WriteInternal(listLengths, memWriters, child, version);
		}
	}

	private void WriteData(BinaryWriter bw, int version) {
		if (data == null) {
			throw new NullReferenceException("Data Cannot be null");
		}
		var count = (uint)data.Length;
		bw.Write(count);
		for (var i = 0; i < count; i++) {
			bw.Write((uint)data[i].Length);
		}
		for (var i = 0; i < count; i++) {
			for (var j = 0; j < data[i].Length; j++) {
				data[i][j].Write(bw, version);
			}
		}
	}

	public void Read(BinaryReader br, int version) {
		nowId = br.ReadUInt32();
		if (nowId == 0) {
			return;
		}

		data = new TData[nowId][];
		for (var i = 0; i < nowId; i++) {
			var listSize = br.ReadUInt32();
			data[i] = new TData[listSize];
		}
		for (var i = 0; i < nowId; i++) {
			for (var j = 0; j < data[i].Length; j++) {
				data[i][j] = new TData();
				data[i][j].Read(br, version);
				length += data[i][j].GetLength(version);
			}
		}
	}

	/// <summary>
	/// 尚未实现，以后有需要再弄
	/// </summary>
	/// <exception cref="NotImplementedException"></exception>
	public void EnableEdit() => throw new NotImplementedException();
		
	/// <summary>
	/// 可录制的List，内容会被优化保存到<see cref="CompressedTree{T}"/>
	/// </summary>
	public sealed class RecordableList : ObservableCollection<TData> {
		/// <summary>
		/// 集合的内容自上次调用<see cref="AddToCompressedTree"/>之后是否改变过
		/// </summary>
		public bool IsChanged { get; private set; }

		public uint ListId { get; private set; }

		public RecordableList() {
			CollectionChanged += (_, _) => IsChanged = true;
		}

		/// <summary>
		/// 将当前集合存入CompressedTree，只会在发生改变之后调用，不必担心性能损失
		/// </summary>
		/// <param name="compressedTree"></param>
		/// <param name="version"></param>
		/// <returns></returns>
		public uint AddToCompressedTree(CompressedTree<TData> compressedTree, int version) {
			if (!IsChanged) {
				return ListId;
			}
			ListId = compressedTree.AddList(this, version);
			IsChanged = false;
			return ListId;
		}

		/// <summary>
		/// 解析<see cref="CompressedTree{T}"/>，做过性能优化
		/// </summary>
		/// <param name="listId"></param>
		/// <param name="listLength"></param>
		/// <param name="compressedTree"></param>
		public void ParseFromCompressedTree(uint listId, uint listLength, CompressedTree<TData> compressedTree) {
			if (listId == 0 || listLength == 0) {  // list为0表示空表
				Clear();
			} else {
				var data = compressedTree.data ?? throw new NullReferenceException("Compressed Tree data is null");
				if (listId == ListId) { // 如果listId相同，那就保证了开头是一样的，现在可能是三种情况
					if (listLength == Count) { // 完全相同，无需更新，直接返回
						return;
					}

					if (listLength < Count) {
						for (var i = Count - 1; i >= listLength; i--) {
							RemoveAt(i);
						}
					} else {
						for (var i = Count; i < listLength; i++) {
							Add(data[listId - 1][i]);
						}
					}
				} else {
					Clear();
					for (var i = 0; i < listLength; i++) {
						Add(data[listId - 1][i]);
					}
				}
			}
			ListId = listId;
		}
	}

	/// <summary>
	/// 可录制的HashSet，适合存储不要求顺序的项目（如Id）内容会被优化保存到<see cref="CompressedTree{T}"/>
	/// </summary>
	public sealed class RecordableSet : ObservableHashSet<TData> {
		/// <summary>
		/// 集合的内容自上次调用<see cref="AddToCompressedTree"/>之后是否改变过
		/// </summary>
		public bool IsChanged { get; private set; }

		public uint ListId { get; private set; }

		public RecordableSet() {
			CollectionChanged += (_, _) => IsChanged = true;
		}

		/// <summary>
		/// 将当前集合存入CompressedTree，只会在发生改变之后调用，不必担心性能损失
		/// </summary>
		/// <param name="compressedTree"></param>
		/// <param name="version"></param>
		/// <returns></returns>
		public uint AddToCompressedTree(CompressedTree<TData> compressedTree, int version) {
			if (!IsChanged) {
				return ListId;
			}
			ListId = compressedTree.AddList(this, version);
			IsChanged = false;
			return ListId;
		}

		/// <summary>
		/// 解析<see cref="CompressedTree{T}"/>
		/// </summary>
		/// <param name="listId"></param>
		/// <param name="listLength"></param>
		/// <param name="compressedTree"></param>
		public void ParseFromCompressedTree(uint listId, uint listLength, CompressedTree<TData> compressedTree) {
			var count = Count;
			var data = compressedTree.data;

			if (data == null || listId == 0) {  // list为0表示空表
				Clear();
			} else if (listId == ListId) {  // 如果listId相同，那就保证了开头是一样的，现在可能是三种情况
				if (listLength == count) {  // 完全相同，无需更新，直接返回
					return;
				}
				if (listLength < count) {  // 例如此时为1,2,3 需要的是1,2
					for (var i = count - 1; i >= listLength; i--) {  // 那就需要倒序遍历，把多出来的删掉
						Remove(data[listId - 1][i]);
					}
				} else {  // 例如此时为1,2 需要的是1,2,3,4
					for (var i = count; i < listLength; i++) {  // 正序遍历，从3开始添加
						Add(data[listId - 1][i]);
					}
				}
			} else {  // 开头不一样
				Clear();
				for (var i = 0; i < listLength; i++) {
					Add(data[listId - 1][i]);
				}
			}
			ListId = listId;
		}
	}
}
