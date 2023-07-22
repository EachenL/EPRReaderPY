using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using EasyPathology.Core.Collections;
using EasyPathology.Core.Extensions;
using EasyPathology.Definitions.Interfaces;
using EasyPathology.Record.Interfaces;
using static OpenCvSharp.ML.DTrees;

namespace EasyPathology.Record.AdditionalInfos;

[ObservableObject]
public sealed partial class ClassifyTreeNode {
	[JsonIgnore]
	public ClassifyTreeNode? Parent { get; set; }

	[ObservableProperty] private string text;

	public int Level { get; }

	/// <summary>
	/// 唯一的ID，在Excel中Load的时候分配
	/// </summary>
	public uint Id { get; }

	[JsonIgnore]
	public bool IsChecked {
		get => isChecked;
		set {
			if (isChecked == value || isCascading) {
				return;
			}

			isChecked = value;
			isCascading = true;
			if (value) {  // 如果子项选择，那么上溯所有父亲也要被选择
				var parent = Parent;
				while (parent != null) {
					parent.IsChecked = true;
					parent = parent.Parent;
				}
			} else {  // 如果取消选择，所有子节点都要取消
				foreach (var child in Children) {
					child.IsChecked = false;
				}
			}
			isCascading = false;

			OnPropertyChanged();
		}
	}

	private bool isChecked;
	/// <summary>
	/// 是否在级联处理选中事件
	/// </summary>
	private bool isCascading;

	[JsonIgnore]
	public bool IsExpanded {
		get => isExpanded;
		set {
			if (Children.Count == 0) {  // 如果没有子项，就进行选择
				IsChecked = value;
			}

			SetProperty(ref isExpanded, value);  // 也得设置IsExpanded，不然不同步的话可能不会触发更改
		}
	}

	private bool isExpanded;

	/// <summary>
	/// 必选？如果没有选中就视为无效选择
	/// </summary>
	public bool IsRequired {
		get => propertyFlags.HasFlagUnsafe(PropertyFlags.Required);
		set => SetPropertyFlag(PropertyFlags.Required, value);
	}

	/// <summary>
	/// 当前项是否可选
	/// </summary>
	public bool IsCheckable {
		get => !propertyFlags.HasFlagUnsafe(PropertyFlags.NotCheckable);
		set => SetPropertyFlag(PropertyFlags.NotCheckable, !value);
	}

	[Flags]
	private enum PropertyFlags {
		/// <summary>
		/// 是否必选，不代表必须要选择此项，因为选择它的子项时它也会被选择
		/// </summary>
		Required = 1 << 0,
		/// <summary>
		/// 不可选，往往表示这只是一个分类名，只能选择它的子项，显示时会表示成Disabled
		/// </summary>
		NotCheckable = 1 << 1
	}

	private PropertyFlags propertyFlags;

	[JsonIgnore]
	internal int Properties {
		get => (int)propertyFlags;
		set => propertyFlags = (PropertyFlags)value;
	}

	private void SetPropertyFlag(PropertyFlags flag, bool value) {
		if (value) {
			propertyFlags |= flag;
		} else {
			propertyFlags &= ~flag;
		}
	}

	public ConcurrentObservableCollection<ClassifyTreeNode> Children { get; } = new();

	public ClassifyTreeNode(uint id, string text, int level, ClassifyTreeNode? parent = null) {
		Id = id;
		this.text = text;
		Level = level;
		Parent = parent;
	}

	/// <summary>
	/// 递归并对所有的Children执行
	/// </summary>
	/// <param name="action"></param>
	public void RecursionChildren(Action<ClassifyTreeNode> action) {
		action.Invoke(this);
		foreach (var node in Children) {
			node.RecursionChildren(action);
		}
	}

	public override string ToString() => Text;

	/// <summary>
	/// 获取包含路径的详细文本，形如 xx -> xx -> xx
	/// </summary>
	[JsonIgnore]
	public string DetailText {
		get {
			if (detailText == null) {
				if (Parent == null) {
					detailText = Text;
				}
				var stack = new Stack<string>();
				var node = this;
				while (node != null) {
					stack.Push(node.Text);
					node = node.Parent;
				}

				var sb = new StringBuilder();
				while (stack.Count > 1) {
					sb.Append(stack.Pop()).Append(" -> ");
				}

				sb.Append(stack.Pop());
				detailText = sb.ToString();
			}

			return detailText;
		}
	}

	private string? detailText;

	/// <summary>
	/// 只会Clone固有的属性，是否选中、是否展开等不会被复制
	/// </summary>
	/// <returns></returns>
	public ClassifyTreeNode Clone() {
		var clone = new ClassifyTreeNode(Id, Text, Level) {
			IsCheckable = IsCheckable
		};
		foreach (var cloneChild in Children.Select(static child => child.Clone())) {
			cloneChild.Parent = clone;
			clone.Children.Add(cloneChild);
		}
		return clone;
	}
}

/// <summary>
/// 用于处理分类树形列表的类，可以读取一个excel表
/// </summary>
///	存储结构如下
///	先是一个uint记录一共有多少个node
///	之后写出每个node的<see cref="ClassifyTreeNode.Id"/>, <see cref="ClassifyTreeNode.Level"/>, <see cref="ClassifyTreeNode.Text"/>，<see cref="ClassifyTreeNode.Properties"/>和parent ID
/// 存储时确保parent ID已经在前面读取了
public sealed class ClassifyTree : IAdditionalInfo {
	public static ClassifyTree Default { get; } = new();

	/// <summary>
	/// 所有Node，下标为Id - 1
	/// </summary>
	[JsonIgnore]
	public IReadOnlyList<ClassifyTreeNode> Items => items;

	private readonly List<ClassifyTreeNode> items = new();

	/// <summary>
	/// Level为0的Nodes组成的列表，用于绑定到ItemsSource
	/// </summary>
	public ConcurrentObservableCollection<ClassifyTreeNode> RootItems { get; } = new();

	public delegate void IsCheckedChangedHandler(ClassifyTreeNode node);

	public event IsCheckedChangedHandler? IsCheckedChanged;

	public ClassifyTree() { }

	/// <summary>
	/// 通过旧的Tree克隆一个新的，深拷贝
	/// </summary>
	/// <param name="old"></param>
	public ClassifyTree(ClassifyTree old) {
		foreach (var node in old.RootItems) {
			var clone = node.Clone();
			RootItems.Add(clone);
			CloneChild(clone);
		}
	}

	public ClassifyTree Clone() {
		return new ClassifyTree(this);
	}

	private void CloneChild(ClassifyTreeNode clone) {
		items.Add(clone);
		clone.PropertyChanged += Node_OnPropertyChanged;
		foreach (var child in clone.Children) {
			CloneChild(child);
		}
	}

	/// <summary>
	/// 将所有Node设为没有选中和没有展开的状态
	/// </summary>
	public void Reset() {
		foreach (var node in Items) {
			node.IsChecked = false;
			node.IsExpanded = false;
		}
	}

	/// <summary>
	/// 清空所有已加载的Node
	/// </summary>
	private void Clear() {
		items.Clear();
		RootItems.Clear();
	}

	/// <summary>
	/// 解析
	/// </summary>
	/// <param name="text"></param>
	public void DeserializeDetailText(string text) {
		foreach (var line in text.Split('\n')) {
			IEnumerable<ClassifyTreeNode> nodes = RootItems;
			foreach (var word in line.Split("->").Select(static s => s.Trim())) {
				var node = nodes.FirstOrDefault(n => n.Text == word);
				if (node == null) {
					break;
				}

				node.IsChecked = node.IsExpanded = true;
				nodes = node.Children;
			}
		}
	}

	public uint GetLength(int version) {
		var textLength = 0U;
		foreach (var node in RootItems) {
			CalcNodeTextLength(ref textLength, node);
		}
		return sizeof(uint) + (uint)Items.Count * (2U * sizeof(uint) + 2U * sizeof(int)) + textLength;
	}

	private static void CalcNodeTextLength(ref uint length, ClassifyTreeNode node) {
		length += node.Text.GetStringSaveLength();
		foreach (var child in node.Children) {
			CalcNodeTextLength(ref length, child);
		}
	}

	public AdditionalInfoType Type => AdditionalInfoType.DiagnosisClassifyTree;

	/// <summary>
	/// 回放时调用
	/// </summary>
	/// <param name="br"></param>
	/// <param name="version"></param>
	public void Read(BinaryReader br, int version) {
		Clear();
		this.BeginRead(br);
		var nodeCount = br.ReadUInt32();
		var nodes = new ClassifyTreeNode[nodeCount];
		for (var i = 0; i < nodeCount; i++) {
			var node = new ClassifyTreeNode(br.ReadUInt32(), br.ReadString(), br.ReadInt32()) {
				Properties = br.ReadInt32()
			};
			var parentId = br.ReadUInt32();
			nodes[node.Id - 1] = node;
			if (node.Level == 0) {
				RootItems.Add(node);
			} else {
				var parent = nodes[(int)parentId - 1];
				Debug.Assert(parent != null);  // 肯定不是null，如果是null说明有问题
				node.Parent = parent;
				parent.Children.Add(node);  // 这个别忘了
			}
			node.PropertyChanged += Node_OnPropertyChanged;
		}
		items.AddRange(nodes);
		this.EndRead(br);
	}

	internal void AddNode(ClassifyTreeNode node) {
		node.PropertyChanged += Node_OnPropertyChanged;
		items.Add(node);
	}

	public void Write(BinaryWriter bw, int version) {
		this.WriteAdditionalInfoHeader(bw, version);
		bw.Write((uint)Items.Count);
		foreach (var node in RootItems) {
			WriteNode(bw, node);
		}
	}

	private static void WriteNode(BinaryWriter bw, ClassifyTreeNode node) {
		bw.Write(node.Id);
		bw.Write(node.Text);
		bw.Write(node.Level);
		bw.Write(node.Properties);
		bw.Write(node.Parent?.Id ?? 0U);
		foreach (var child in node.Children) {
			WriteNode(bw, child);
		}
	}

	private void Node_OnPropertyChanged(object? sender, PropertyChangedEventArgs e) {
		if (e.PropertyName == nameof(ClassifyTreeNode.IsChecked)) {
			IsCheckedChanged?.Invoke((ClassifyTreeNode)sender.NotNull());
		}
	}
}