using System.Runtime.InteropServices;
using EasyPathology.Inference.Models;

namespace EasyPathology.Inference.Utils;

public class ProcessPts
{
    // 说明文档有待拟写，不过有oneNote版本，可先参考tobiiProLab上指标含义和官网官方解释
    // https://www.tobiipro.com/learn-and-support/learn/steps-in-an-eye-tracking-study/data/understanding-Pro-Labs-eye-tracking-metrics/
    // https://www.tobiipro.cn/learn-and-support/learn/steps-in-an-eye-tracking-study/data/understanding-pro-labs-eye-tracking-metrics/

    ///////////////////////////////DOCTOR METRICS//////////////////////////////////
    /// <summary>
    /// 医生水平
    /// </summary>
    public ushort DoctorLevel { get; }
    /// <summary>
    /// 医生工作年龄
    /// </summary>
    public ushort DoctorWorkYear { get; }
    /// <summary>
    /// 医生阅片数，0阅片一万张以下，1阅片1w-5w，2 5w-10w，3 10w+
    /// </summary>
    public ushort DoctorReadNum { get; }

    //////////////////////////////AOI GENERATION METRICS///////////////////////////
    /// <summary>
    /// 切片名称
    /// </summary>
    public string WsiName { get; }
    /// <summary>
    /// 病变类型
    /// </summary>
    public string LesionType { get; }
    /// <summary>
    /// 眼动仪有效数据比例
    /// </summary>
    public float ValidDataRatio => TotalVGaze.Count / (float)epr.FrameCount;

    /// <summary>
    /// epr.MinLevel
    /// </summary>
    public int MinLevel { get; }
    public long SlideWidth { get; }
    public long SlideHeight { get; }
    /// <summary>
    /// 组织占比（背景）
    /// </summary>
    public double TissueRatio { get; }

    /// <summary>
    /// 有效组织面积
    /// </summary>
    public double VArea { get; }
    /// <summary>
    /// 单位组织面积有效时间比值
    /// </summary>
    public double VAreaTimeRatio
    {
        get => VArea / TotalFixationDbsTime;
    }

    /// <summary>
    /// 总凝视时间（未筛选）
    /// </summary>
    public double TotalGazeTime { get; }
    /// <summary>
    /// 不同level下注视时间（未筛选）
    /// </summary>
    public List<double> DifLevelGazeTime { get; } = new();
    /// <summary>
    /// 注视点数量（未筛选）
    /// </summary>
    public int GazeCount { get; }
    /// <summary>
    /// 有效注视点
    /// </summary>
    public int GazeVCount { get; }


    ////////////////////////////////AOI FIXATION&&DBS METRICS/////////////////////
    /// <summary>
    /// 所有注视区域时间，总注视时间（已筛选角速度和聚类）
    /// </summary>
    public double TotalFixationDbsTime { get; }
    /// <summary>
    /// 平均注视区域时间（已筛选角速度和聚类）
    /// </summary>
    public double AverageFixationDbsTime { get; }
    /// <summary>
    /// 该WSI注视区域最短的时间（已筛选角速度和聚类）
    /// </summary>
    public double GenMinFixationDbsTime { get; }
    /// <summary>
    /// 该WSI注视区域最长的时间（已筛选角速度和聚类）
    /// </summary>
    public double GenMaxFixationDbsTime { get; }
    /// <summary>
    /// 不同level上注视区域最短时间（已筛选角速度和聚类）
    /// </summary>
    public List<double> DifLevelMinFixationDbsTime { get; } = new();
    /// <summary>
    /// 不同level上注视区域最长时间（已筛选角速度和聚类）
    /// </summary>
    public List<double> DifLevelMaxFixationDbsTime { get; } = new();
    /// <summary>
    /// 注视区域数量,每个区域数量必大于等于8（已筛选角速度和聚类）
    /// </summary>
    public int FixationDbsCount { get; }
    /// <summary>
    /// 到第一次有效凝视的时间（已筛选角速度和聚类）
    /// </summary>
    public double ToFirstFixationDbsTime { get; }
    /// <summary>
    /// 首次注视时间（已筛选角速度和聚类）
    /// </summary>
    public double FirstFixationDbsTime { get; }


    /////////////////Maybe useful/////////////////////
    /// <summary>
    /// 到第一次有效注视的帧数（已筛选角速度和聚类）
    /// </summary>
    public int ToFirstFixationDbsFrame { get; }
    /// <summary>
    /// 首次注视帧数（已筛选角速度和聚类）
    /// </summary>
    public int FirstFixationDbsFrame { get; }
    /// <summary>
    /// 注视点数量（已筛选角速度和聚类）
    /// </summary>
    public int TotalVGazeDbsCount;
    /// <summary>
    /// 总注视帧数（已筛选角速度和聚类）
    /// </summary>
    public float TotalFixationDbsFrame { get; }



    //////////////////////////////////AOI VISIT METRICs///////////////////////////
    /*
	 * Visits are defined as the time between the start of the first fixation on the AOI 
	 * until the end of the last fixation on the AOI. The entry and exit saccades are not included.
	 * 这里由于只有一个AOI即一个切片
	*/
    /// <summary>
    /// visits的时间
    /// </summary>
    public double TotalDurationVisits { get; }
    //////////////////////////////////AOI GLANCE METRICs//////////////////////////
    /*Glances (ISO 15007-1) are defined as the time between the start of the saccade 
	 *leading into the AOI until the end of the last fixation on the AOI. The exit saccade is not included.
	 *PeakVelocity角速度
	 */
    public double TotalDurationGlances { get; }
    public double MinPeakVelocitySaccades { get; }
    public double MaxPeakVelocitySaccades { get; }
    /// <summary>
    /// 扫视时角速度方差
    /// </summary>
    public double SDPeakVelocitySaccades { get; }
    public double TimeToFirstSaccade { get; }
    public double PeakVelocityFirstSaccade { get; }
    /////////////////////////////////AOI SACCADE METRICS//////////////////////////
    /// <summary>
    /// 总扫视时间(总数-只经角速度筛选点数)
    /// </summary>
    public double TotalSaccadeTime { get; }
    /// <summary>
    /// 总扫视点数量
    /// </summary>
    public double TotalSaccadeCount { get; }
    public double TimeToEntrySaccade { get; }
    public double TimeToExitSaccade { get; }
    public List<double> PeakVelocityEntrySaccade { get; } = new();


    ////////////////////////////////AOI FIXATION METRICS/////////////////////
    /*Fixations can be defined as the periods of time where the eyes are relatively still, 
	 * holding the central foveal vision in place so that the visual system can take in detailed 
	 * information about what is being looked at. In Tobii Pro Lab, fixation is a sequence of raw gaze points,
	 * where the estimated velocity is below the velocity threshold set in the I-VT gaze Grouper.
	*/
    /// <summary>
    /// 所有注视区域时间，总注视时间（已筛选角速度）
    /// </summary>
    public double TotalFixationTime { get; }
    /// <summary>
    /// 平均注视区域时间（已筛选角速度）
    /// </summary>
    public double AverageFixationTime { get; }
    /// <summary>
    /// 该WSI注视区域最短的时间（已筛选角速度）
    /// </summary>
    public double GenMinFixationTime { get; }
    /// <summary>
    /// 该WSI注视区域最长的时间（已筛选角速度）
    /// </summary>
    public double GenMaxFixationTime { get; }
    /// <summary>
    /// 不同level上注视区域最短时间（已筛选角速度）
    /// </summary>
    public List<double> DifLevelMinFixationTime { get; } = new();
    /// <summary>
    /// 不同level上注视区域最长时间（已筛选角速度）
    /// </summary>
    public List<double> DifLevelMaxFixationTime { get; } = new();
    /// <summary>
    /// 注视区域数量（已筛选角速度）
    /// </summary>
    public int TotalFixationCount { get; }
    /// <summary>
    /// 到第一次有效凝视的时间（已筛选角速度）
    /// </summary>
    public double ToFirstFixationTime { get; }
    /// <summary>
    /// 首次注视时间（已筛选角速度）
    /// </summary>
    public double FirstFixationTime { get; }


    /// <summary>
    /// 总非扫视点数量(即角速度小于threshold)（不等于 TotalVGazeDbsCount，后者还会经过聚类筛除noise）
    /// </summary>
    public double TotalVGazeCount { get; }
    /// <summary>
    /// 不同level下注视点数量，经过角速度，没有Dbs
    /// </summary>
    public List<double> DifLevelVGazeCount { get; } = new();


    /// <summary>
    /// 到第一次有效注视的帧数（已筛选角速度）
    /// </summary>
    public int ToFirstFixationFrame { get; }





    ///////////////////////////////AOI RATION METRICS///////////////////////
    /// <summary>
    /// 首次凝视区域占组织比(去除背景)
    /// </summary>
    public double FirstVGazeRatioTissue { get; }
    /// <summary>
    /// 总凝视区域占组织比(去除背景)
    /// </summary>
    /// 
    public double TotalVGazeRatioTissue { get; }
    /// <summary>
    /// 不同level下注视区域组织占比
    /// </summary>
    public List<double> DifLevelVGazeRatioTissue { get; } = new();
    /// <summary>
    /// 稀疏比：注视点（已筛选角速度和聚类）/ 注视点（未筛选）
    /// </summary>
    public double SparsityRatio { get; }
    /// <summary>
    /// 注视率：注视点（已筛选角速度和聚类）/ 总阅片时间（未筛选）
    /// </summary>
    public double VGazeRatioTime { get; }
    /// <summary>
    /// 各倍率下的 注视占比（筛选后视点占该倍率视点）
    /// </summary>
    public List<double> DifLevelVGazeRatioGaze { get; } = new();


    /////////////////////////////////UTILS////////////////////////////////


    /// <summary>
    /// 有效数据（未经角速度，Dbs）
    /// </summary>
    private List<DataFrame> TotalVGaze { get; }
    /// <summary>
    /// 所有视点：只经过角速度筛选
    /// </summary>
    private List<DataFrame> TotalVAngGaze { get; }
    /// <summary>
    /// epr数据读入
    /// </summary>
    private EasyPathologyRecord epr { get; }

    /// <summary>
    /// 按level和时间顺序储存视点倍率下聚类结果
    /// [level1:
    ///     [time1:[kind1:[],kind2:[],...],
    ///     time2:[kind1:[],kind2:[],...],
    ///     ],
    ///  level2:[[[]]]
    ///  ]
    /// </summary>
    private List<List<List<ClusterPoint>>>[] DbsResult { get; }
    /// <summary>
    /// 按时间和level储存有效的注视点
    /// </summary>
    private List<List<ClusterPoint>>[] VResult { get; }
    private List<DataFrame> SaccadesResult = new();

    /// <summary>
    /// 按level和时间顺序储存视点倍率下聚类的level
    /// [level1:
    ///     [time1:[kind1:[],kind2:[],...],
    ///     time2:[kind1:[],kind2:[],...],
    ///     ],
    ///  level2:[[[]]]
    ///  ]
    /// </summary>
    private List<int> OrderDbsLevel = new();
    /// <summary>
    /// 按level和时间顺序储存视点倍率下聚类结果R
    /// [level1:
    ///     [time1:[kind1:[],kind2:[],...],
    ///     time2:[kind1:[],kind2:[],...],
    ///     ],
    ///  level2:[[[]]]
    ///  ]
    /// </summary>
    private List<int>[] OrderDbsR { get; }

    /// <summary>
    /// 返回只经过角速度筛选的点按时间顺序level改变的索引
    /// </summary>
    private List<int> ChangeIndex { get; }
    private int[] zoom { get; }


    /// <summary>
    /// 视点筛选分析
    /// </summary>
    /// <param name="vThreshold">视点角速度阈值,推荐30</param>
    /// <param name="detailsLevel">精准程度，对应于切片level数，越小越精准越慢，从0开始 推荐：level 2</param>
    /// <param name="minPts">核心点形成的圆中含有的最少点数</param>
    public ProcessPts(EasyPathologyRecord epr, string slidePath, double vThreshold = 30, int detailsLevel = 2, int minPts = 10/*, string path_ndpi, List<DataRecTem> data, List<int[]> xy,
                                List<double> r_list, List<int> l_list,
                                List<double> angularv, List<double> z*/)
    {
        this.epr = epr;
        var data = epr.DataFrames;
        var doctor_info = epr.DoctorDetail;
        DoctorLevel = (ushort)doctor_info.Level;
        DoctorWorkYear = doctor_info.WorkYear;
        DoctorReadNum = (ushort)doctor_info.SlideReadCount;

        zoom = epr.LevelScales;
        SlideWidth = epr.MaxSlideWidth;
        SlideHeight = epr.MaxSlideHeight;
        WsiName = epr.SlideName;
        MinLevel = epr.MinLevel;
        // level从小到大（图片由大到小），储存每个level下聚类后的结果
        DbsResult = new List<List<List<ClusterPoint>>>[epr.MaxLevel - epr.MinLevel + 1];
        VResult = new List<List<ClusterPoint>>[epr.MaxLevel - epr.MinLevel + 1];
        for (var i = 0; i < DbsResult.Length; i++)
        {

            DbsResult[i] = new List<List<List<ClusterPoint>>>();
            VResult[i] = new List<List<ClusterPoint>>();
        }

        // 清理第一遍，获取有效数据(绝对坐标)
        TotalVGaze = new List<DataFrame>();
        // Data lastD = null;
        foreach (var d in data)
        {
            if (d.GazeX < 0 || d.GazeY < 0 || !d.IsEyeTrackerDataValid)
            {
                continue;
            }
            TotalVGaze.Add(d);
        }
        if (TotalVGaze.Count != 0)
        {
            // 不同level下数量和经角速度筛选后数量
            for (var level = epr.MinLevel; level <= epr.MaxLevel; level++)
            {
                var gazeNoDbsCount = 0;
                // 该level下所有注视点数(未筛选)
                var count = 0;
                for (int k = 0; k < TotalVGaze.Count; k++)
                {
                    if (TotalVGaze[k].VirtualLevel != level)
                    {
                        continue;
                    }
                    count++;
                    if (TotalVGaze[k].Velocity > vThreshold)
                    {
                        continue;
                    }
                    gazeNoDbsCount++;

                }
                // 不同倍率下注视点数量 经角速度
                DifLevelVGazeCount.Add(gazeNoDbsCount);

                // 不同倍率下注视时间未筛选
                DifLevelGazeTime.Add(count * 0.02);
            }

            // 储存视点level在TotalVAngGaze中改变的索引，ex：2，2，2，2，4，4，4（0，4）
            var lastLevel = -1;
            ChangeIndex = new List<int>();
            TotalVAngGaze = new List<DataFrame>();
            var test = new List<DataFrame>(); ;
            for (int k = 0; k < TotalVGaze.Count; k++)
            {
                if (TotalVGaze[k].Velocity > vThreshold)
                {
                    SaccadesResult.Add(TotalVGaze[k]);
                    continue;
                }
                TotalVAngGaze.Add(TotalVGaze[k]);
                if (TotalVGaze[k].VirtualLevel != lastLevel)
                {
                    ChangeIndex.Add(TotalVAngGaze.Count - 1);
                    lastLevel = TotalVGaze[k].VirtualLevel;
                }
            }


            OrderDbsR = new List<int>[epr.MaxLevel - epr.MinLevel + 1];
            for (var i = 0; i < OrderDbsR.Length; i++)
            {
                OrderDbsR[i] = new List<int>();
            }

            // 注视过程中level一直没有改变
            if (ChangeIndex.Count == 1)
            {
                var eps = TotalVGaze.Sum(static vGaze => vGaze.Radius) / TotalVGaze.Count;
                var level = TotalVGaze[0].VirtualLevel;

                var dBPoints = TotalVGaze.Select(static gaze => new ClusterPoint(gaze.GazeX - gaze.ScreenX, gaze.GazeY - gaze.ScreenY, gaze.FrameIndex, gaze.VirtualLevel, gaze.Radius)).ToList();

                var dbs = ClusterScan.GetClusters(dBPoints, eps, minPts);
                if (dbs == null)
                {

                }

                // 注视点数量（已筛选角速度和聚类）
                TotalVGazeDbsCount += dbs.Select(static kind => kind.Count).Sum();
                VResult[level - epr.MinLevel].Add(dBPoints);
                OrderDbsLevel.Add(level);
                OrderDbsR[level - epr.MinLevel].Add((int)eps);
                DbsResult[level - epr.MinLevel].Add(dbs);
            }


            // 按顺序level进行聚类            
            for (var i = 0; i < ChangeIndex.Count - 1; i++)
            {
                var dBPoints = new List<ClusterPoint>();
                var sumR = 0d;
                var level = -1;
                sumR = (from VGaze in TotalVAngGaze.Skip(ChangeIndex[i]).Take(ChangeIndex[i + 1] - ChangeIndex[i]) select VGaze.Radius).Sum();
                var eps = sumR / TotalVAngGaze.Skip(ChangeIndex[i]).Take(ChangeIndex[i + 1] - ChangeIndex[i]).Count();


                foreach (var gaze in TotalVAngGaze.Skip(ChangeIndex[i]).Take(ChangeIndex[i + 1] - ChangeIndex[i]))
                {

                    dBPoints.Add(new ClusterPoint(gaze.GazeX - gaze.ScreenX, gaze.GazeY - gaze.ScreenY, gaze.FrameIndex, gaze.VirtualLevel, gaze.Radius));
                    level = gaze.VirtualLevel;
                }
                var dbs = ClusterScan.GetClusters(dBPoints, eps, minPts);

                // 注视点数量（已筛选角速度和聚类）
                if (dbs == null)
                {
                    continue;
                }

                TotalVGazeDbsCount += (from kind in dbs select kind.Count).Sum();
                VResult[level - epr.MinLevel].Add(dBPoints);
                OrderDbsLevel.Add(level);
                OrderDbsR[level - epr.MinLevel].Add((int)eps);
                DbsResult[level - epr.MinLevel].Add(dbs.ToList());
            }

            // 病变类型
            LesionType = epr.SelectedType;
            // 总阅片时间（未筛选）
            TotalGazeTime = epr.FrameCount * 0.02;

            // 第一次和最后一次相关数据计算
            var minDbsTick = (from orderLevelPts in DbsResult from levelPts in orderLevelPts from kindPts in levelPts from pt in kindPts select pt.FrameIndex).Min();
            var firstDbsPt = (from orderLevelPts in DbsResult from levelPts in orderLevelPts from kindPts in levelPts from pt in kindPts select pt).Single(t => t.FrameIndex == minDbsTick);
            var minVTick = (from levelPt in VResult from pts in levelPt from pt in pts select pt.FrameIndex).Min();
            var maxVTick = (from levelPt in VResult from pts in levelPt from pt in pts select pt.FrameIndex).Max();
            var firstVPt = (from levelPt in VResult from pts in levelPt from pt in pts select pt).Single(t => t.FrameIndex == minVTick);

            // Glances metric相关计算
            // 去掉exit部分的saccade
            SaccadesResult = SaccadesResult.Take(SaccadesResult.IndexOf(SaccadesResult.Find(t => t.FrameIndex > maxVTick)) - 1).ToList();
            if (SaccadesResult.Count != 0)
            {
                TimeToFirstSaccade = SaccadesResult[0].TimeStamp;
                PeakVelocityFirstSaccade = SaccadesResult[0].Velocity;
                MinPeakVelocitySaccades = (from result in SaccadesResult select result.Velocity).Min();
                MaxPeakVelocitySaccades = (from result in SaccadesResult select result.Velocity).Max();
                var sumSaccadeVelocity = (from result in SaccadesResult select result.Velocity).Sum();
                SDPeakVelocitySaccades = Math.Sqrt((from result in SaccadesResult select Math.Pow(result.Velocity - sumSaccadeVelocity, 2)).Sum());
            }



            for (var i = 0; i < minVTick - 1; i++)
            {
                PeakVelocityEntrySaccade.Add(TotalVGaze[i].Velocity);
            }
            if (minVTick == 0)
            {
                TimeToEntrySaccade = 0;
            }
            else
            {
                TimeToEntrySaccade = (minVTick - 1) * 0.02;
            }
            TimeToExitSaccade = (epr.FrameCount - maxVTick) * 0.02;

            TotalDurationGlances = maxVTick * 0.02;
            TotalDurationVisits = (maxVTick - minVTick) * 0.02;



            // 到第一次有效注视的帧数
            ToFirstFixationDbsTime = minDbsTick * 0.02;
            ToFirstFixationDbsFrame = minDbsTick;
            ToFirstFixationTime = minVTick * 0.02;
            ToFirstFixationFrame = minVTick;
            // 注视点数量（未筛选）
            GazeCount = epr.FrameCount;
            GazeVCount = TotalVGaze.Count;
            // 注视时间（已筛选角速度和聚类）
            TotalFixationDbsTime = TotalVGazeDbsCount * 0.02;
            // 顺序注视区域总数(相邻连个缩放之间小于8忽略)
            FixationDbsCount = (from level in DbsResult from time in level select time.Count).Sum();
            AverageFixationDbsTime = TotalFixationDbsTime / FixationDbsCount;

            foreach (var levelPt in DbsResult)
            {
                if (levelPt.Count != 0)
                {
                    DifLevelMinFixationDbsTime.Add((from time in levelPt from kind in time select kind.Count).Min() * 0.02);
                    DifLevelMaxFixationDbsTime.Add((from time in levelPt from kind in time select kind.Count).Max() * 0.02);
                }
            }
            foreach (var levelPt in VResult)
            {
                if (levelPt.Count != 0)
                {
                    DifLevelMinFixationTime.Add((from vx in levelPt select vx.Count).Min() * 0.02);
                    DifLevelMaxFixationTime.Add((from vx in levelPt select vx.Count).Max() * 0.02);
                }
            }
            GenMaxFixationDbsTime = DifLevelMaxFixationDbsTime.Max();
            GenMinFixationDbsTime = DifLevelMinFixationDbsTime.Min();
            GenMinFixationTime = DifLevelMinFixationTime.Min();
            GenMaxFixationTime = DifLevelMaxFixationTime.Max();
            // 总扫视时间
            TotalSaccadeTime = TotalGazeTime - (from levelGazeCountNoDbs in DifLevelVGazeCount select levelGazeCountNoDbs).Sum() * 0.02;
            // 注视帧数（已筛选角速度和聚类）
            TotalFixationDbsFrame = TotalVGazeDbsCount;
            // 非扫视点数量
            TotalVGazeCount = (from levelGazeCountNoDbs in DifLevelVGazeCount select levelGazeCountNoDbs).Sum();
            TotalFixationTime = TotalVGazeCount * 0.02;
            TotalFixationCount = (from levelPt in VResult select levelPt.Count()).Sum();
            AverageFixationTime = TotalFixationTime / TotalFixationCount;

            // 扫视点数量
            TotalSaccadeCount = GazeCount - TotalVGazeCount;
            // 稀疏比：注视点（已筛选角速度和聚类）/ 注视点（未筛选）
            SparsityRatio = TotalVGazeDbsCount / GazeCount;
            // 注视率：注视点（已筛选角速度和聚类）/ 总阅片时间（未筛选）
            VGazeRatioTime = TotalVGazeDbsCount / TotalGazeTime;

            // 首次注视时间
            FirstFixationDbsTime = DbsResult[firstDbsPt.Level - epr.MinLevel][0][0].Count * 0.02;
            FirstFixationTime = VResult[firstDbsPt.Level - epr.MinLevel][0].Count * 0.02;
            // 首次注视帧数
            FirstFixationDbsFrame = (int)(FirstFixationDbsTime / 0.02);

            // 各倍率下注视占比
            for (var i = 0; i < epr.MaxLevel - epr.MinLevel + 1; i++)
            {
                DifLevelVGazeRatioGaze.Add((from levelResults in DbsResult[i]
                                            from oneResult in levelResults
                                            select oneResult.Count).Sum() / (DifLevelGazeTime[i] / 0.02));
            }

            var otsuTissue = new OtsuTissue(epr, detailsLevel, slidePath);
            VArea = otsuTissue.TissuePixels;
            // 组织占比(+背景)
            //TissueRatio = (double)otsuTissue.TissuePixels / otsuTissue.WsiPixels;
            // 首次注视区域占组织比
            var mask = new Mat(new Size(epr.MaxSlideWidth / zoom[zoom.Length - 1 - detailsLevel], epr.MaxSlideHeight / zoom[zoom.Length - 1 - detailsLevel]), MatType.CV_8UC1);
            for (var i = 0; i < FirstFixationDbsFrame; i++)
            {

                Cv2.Circle(mask, DbsResult[firstDbsPt.Level - epr.MinLevel][0][0][i].X * zoom[DbsResult[firstDbsPt.Level - epr.MinLevel][0][0][i].Level] / zoom[zoom.Length - 1 - detailsLevel],
                    DbsResult[firstDbsPt.Level - epr.MinLevel][0][0][i].Y * zoom[DbsResult[firstDbsPt.Level - epr.MinLevel][0][0][i].Level] / zoom[zoom.Length - 1 - detailsLevel],
                    OrderDbsR[firstDbsPt.Level - epr.MinLevel][0] * zoom[DbsResult[firstDbsPt.Level - epr.MinLevel][0][0][i].Level] / zoom[zoom.Length - 1 - detailsLevel],
                    Scalar.Gray, -1);
            }
            /*          Cv2.Resize(mask, mask, new OpenCvSharp.Size(500, 500));
						Cv2.ImShow("test",mask);
						Cv2.WaitKey();
						*/
            FirstVGazeRatioTissue = (double)Cv2.CountNonZero(mask) / otsuTissue.WsiPixels;

            // 总凝视区域占组织比
            mask = new Mat(new Size(epr.MaxSlideWidth / zoom[zoom.Length - 1 - detailsLevel], epr.MaxSlideHeight / zoom[zoom.Length - 1 - detailsLevel]), MatType.CV_8UC1);
            var difLevelMat = new Mat[epr.MaxLevel - epr.MinLevel + 1];
            for (var i = 0; i < difLevelMat.Length; i++)
            {
                difLevelMat[i] = new Mat(new Size(epr.MaxSlideWidth / zoom[zoom.Length - 1 - detailsLevel], epr.MaxSlideHeight / zoom[zoom.Length - 1 - detailsLevel]), MatType.CV_8UC1);
            }
            for (var i = 0; i < DbsResult.Length; i++)
            {
                for (var j = 0; j < DbsResult[i].Count(); j++)
                {
                    foreach (var pt in DbsResult[i][j].SelectMany(kind => kind))
                    {

                        Cv2.Circle(mask, pt.X * zoom[pt.Level] / zoom[zoom.Length - 1 - detailsLevel],
                            pt.Y * zoom[pt.Level] / zoom[zoom.Length - 1 - detailsLevel], OrderDbsR[pt.Level - epr.MinLevel][j] * zoom[pt.Level]
                                                                                          / zoom[zoom.Length - 1 - detailsLevel], Scalar.Gray, -1);

                        Cv2.Circle(difLevelMat[i], pt.X * zoom[pt.Level] / zoom[zoom.Length - 1 - detailsLevel],
                            pt.Y * zoom[pt.Level] / zoom[zoom.Length - 1 - detailsLevel], OrderDbsR[pt.Level - epr.MinLevel][j] * zoom[pt.Level]
                                                                                          / zoom[zoom.Length - 1 - detailsLevel], Scalar.Gray, -1);
                    }
                }
            }
            //Cv2.Resize(mask, mask, new OpenCvSharp.Size(500, 500));
            //Cv2.ImShow("test", mask);
            //Cv2.WaitKey();
            /*          var a = Cv2.CountNonZero(mask);
						var b = otsuTissue.WsiPixels * zoom[detailsLevel] / zoom[0];*/
            var a = (double)Cv2.CountNonZero(mask);
            TotalVGazeRatioTissue = a / otsuTissue.WsiPixels;

            // 不同level下注视区域组织占比
            foreach (var levelMat in difLevelMat)
            {
                //Cv2.ImShow("test",levelMat);
                //Cv2.WaitKey();
                DifLevelVGazeRatioTissue.Add((double)Cv2.CountNonZero(levelMat) / otsuTissue.WsiPixels);
            }
        }


    }
    public List<DataFrame> GetTotalVGaze()
    {
        return TotalVGaze;
    }
    public int[] GetZoom()
    {
        return zoom;
    }
    /// 按level和时间顺序储存视点倍率下聚类结果
    /// [level1:
    ///     [time1:[kind1:[],kind2:[],...],
    ///     time2:[kind1:[],kind2:[],...],
    ///     ],
    ///  level2:[[[]]]
    ///  ]
    public List<List<List<ClusterPoint>>>[] GetDbsResult()
    {
        return DbsResult;
    }

}


/// <summary>
/// OTSU+.MorphologyEx除去WSI背景
/// </summary>
internal class OtsuTissue
{
    public long TissuePixels { get; }
    public int W { get; }
    public int H { get; }


    //List<OpenSlideImage.ImageDimensions> LevelD = new List<OpenSlideImage.ImageDimensions>();
    public OtsuTissue(EasyPathologyRecord epr, int detailsLevel, string wsiFilePath)
    {

        //var wsi = OpenSlideImage.Open(wsiFilePath);

        /*            for (int i = 0; i < Wsi.LevelCount ; i++)
					{

						LevelD.Add(Wsi.GetLevelDimensions(i));
					}*/
        //Console.WriteLine(a.RealHeight.ToString(), a.RealWidth.ToString());
        //var (w, h) = wsi.GetLevelDimensions(detailsLevel);
        var (w, h) = (epr.MaxLevelWidth, epr.MaxLevelHeight);
        W = (int)(w / detailsLevel + 1);
        H = (int)(h / detailsLevel + 1);
        var ptr = Marshal.AllocHGlobal(W * H * 4);
        //wsi.ReadRegion(detailsLevel, 0, 0, W, H, ptr);
        var src = new Mat(H, W, MatType.CV_8UC4, ptr);

        //Cv2.Resize(src, src, new OpenCvSharp.Size(500, 700));
        //Cv2.ImShow("test", src);
        //Cv2.WaitKey();

        Mat grayImage = new();
        Cv2.CvtColor(src, grayImage, ColorConversionCodes.BGR2GRAY);
        src.Dispose();
        //Cv2.Resize(gray_image, gray_image, new OpenCvSharp.Size(500, 700));
        //Cv2.ImShow("test", gray_image);
        //Cv2.WaitKey();
        Marshal.FreeHGlobal(ptr);
        Mat binary = new();
        Cv2.Threshold(grayImage, binary, 0, 255, ThresholdTypes.Otsu | ThresholdTypes.Binary);
        grayImage.Dispose();
        var kernel1 = Cv2.GetStructuringElement(MorphShapes.Rect, new Size(25, 25));
        var kernel2 = Cv2.GetStructuringElement(MorphShapes.Rect, new Size(10, 10));
        var kernel3 = Cv2.GetStructuringElement(MorphShapes.Rect, new Size(15, 15));
        Cv2.MorphologyEx(binary, binary, MorphTypes.Open, kernel1);
        Cv2.MorphologyEx(binary, binary, MorphTypes.Close, kernel2);
        Cv2.MorphologyEx(binary, binary, MorphTypes.Open, kernel3);
        //Cv2.Resize(binary, binary, new OpenCvSharp.Size(500, 700));
        //Cv2.ImShow("test", binary);
        //Cv2.WaitKey();
        //Mat dst = new Mat();
        //Cv2.AddWeighted(gray_image, 0.8, binary, 0.5, 0, dst);
        //Cv2.ImShow("test", dst);
        //Cv2.WaitKey();


        TissuePixels = H * W - Cv2.CountNonZero(binary);
        binary.Dispose();
    }
    /// <summary>
    /// 返回在该DetailLevel下的结果
    /// </summary>
    /// <returns></returns>
    public long WsiPixels => H * W;

    /*        /// <summary>
			/// 返回WsiDeepZoomGeneration
			/// </summary>
			/// <returns></returns>
			public List<OpenSlideImage.ImageDimensions> GetLevelD()
			{
				return LevelD;
			}*/


}