using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Media.Media3D;
using System.Windows.Threading;
using HelixToolkit.Wpf;
using Newtonsoft.Json;

namespace RealTimeJsonGraph
{
    public partial class MainWindow : Window
    {
        private DispatcherTimer timer;
        private Point3DCollection pointsCollection;
        private List<DataPoint> dataPoints;
        private int currentIndex;
        private FileSystemWatcher fileWatcher;

        public MainWindow()
        {
            InitializeComponent();

            // ポイントの初期化
            pointsCollection = new Point3DCollection();

            // データポイントの初期化
            dataPoints = new List<DataPoint>();
            currentIndex = 0;

            // タイマー設定（0.2秒ごとに更新）
            /*
            timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(0)
            };
            timer.Tick += UpdateGraph;
            timer.Start();
            */

            // JSONファイルの変更を監視
            SetupFileWatcher();

            // JSONデータを読み込み
            LoadData();

            // デバッグ用
            if (PointsVisual == null)
            {
                Console.WriteLine("PointsVisual is null");
            }
            else
            {
                Console.WriteLine("PointsVisual is set");
            }

            // 軸を追加
            AddAxes();
        }

        private void SetupFileWatcher()
        {
            string directoryPath = "C:\\Users\\super\\デスクトップ\\FINDi\\01_phase\\RealTimeJsonGraph";
            string fileName = "data.json";

            fileWatcher = new FileSystemWatcher(directoryPath, fileName)
            {
                NotifyFilter = NotifyFilters.LastWrite
            };

            fileWatcher.Changed += OnFileChanged;
            fileWatcher.EnableRaisingEvents = true;
        }

        private DateTime lastReadTime = DateTime.MinValue;
        private readonly TimeSpan debounceTime = TimeSpan.FromMilliseconds(500);

        private void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            // ファイルが変更されたときにデータを再読み込み
            var currentTime = DateTime.Now;
            if (currentTime - lastReadTime < debounceTime)
            {
                return;
            }
            lastReadTime = currentTime;

            Dispatcher.Invoke(() =>
            {
                LoadData();
                UpdateGraph(null, null); // グラフを更新
            });
        }

        private void LoadData()
        {
            string filePath = "C:\\Users\\super\\デスクトップ\\FINDi\\01_phase\\RealTimeJsonGraph\\data.json"; // JSONファイルのパス
            if (!File.Exists(filePath)) return;

            try
            {
                var jsonData = File.ReadAllText(filePath);
                var newDataPoints = JsonConvert.DeserializeObject<List<DataPoint>>(jsonData);

                if (newDataPoints != null)
                {
                    // 新しいデータポイントのみを追加
                    dataPoints = newDataPoints;
                    Console.WriteLine($"データポイント数: {dataPoints.Count}"); // デバッグ用
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"エラー: {ex.Message}");
            }
        }

        private void UpdateGraph(object sender, EventArgs e)
        {
            Console.WriteLine("UpdateGraph called"); // デバッグ用

            if (dataPoints == null || pointsCollection == null) return;

            int chunkSize = 1000; // 一度に処理するデータポイントの数
            int endIndex = Math.Min(currentIndex + chunkSize, dataPoints.Count);

            for (int i = currentIndex; i < endIndex; i++)
            {
                var dataPoint = dataPoints[i];
                pointsCollection.Add(new Point3D(dataPoint.GetX(), dataPoint.GetY(), dataPoint.GetZ()));
            }

            // グラフ更新
            PointsVisual.Points = pointsCollection;

            currentIndex = endIndex; // インデックスを更新

            // まだ処理するデータが残っている場合は次のチャンクを処理
            if (currentIndex < dataPoints.Count)
            {
                Dispatcher.InvokeAsync(() => UpdateGraph(null, null), DispatcherPriority.Background);
            }
        }

        public class DataPoint
        {
            [JsonProperty("x")]
            public string X { get; set; }

            [JsonProperty("y")]
            public string Y { get; set; }

            [JsonProperty("z")]
            public string Z { get; set; }

            public double GetX() => double.Parse(X);
            public double GetY() => double.Parse(Y);
            public double GetZ() => double.Parse(Z);
        }

        private void AddAxes()
        {
            // X軸
            var xAxis = new ArrowVisual3D
            {
                Point1 = new Point3D(0, 0, 0),
                Point2 = new Point3D(10, 0, 0),
                Diameter = 0.1,
                Fill = System.Windows.Media.Brushes.Red,
                Material = Materials.Red
            };
            HelixView.Children.Add(xAxis);

            var xLabel = new TextVisual3D
            {
                Position = new Point3D(11, 0, 0),
                Text = "X (10m)",
                Height = 1,
                Foreground = System.Windows.Media.Brushes.Red
            };
            HelixView.Children.Add(xLabel);

            // Y軸
            var yAxis = new ArrowVisual3D
            {
                Point1 = new Point3D(0, 0, 0),
                Point2 = new Point3D(0, 10, 0),
                Diameter = 0.1,
                Fill = System.Windows.Media.Brushes.Green,
                Material = Materials.Green
            };
            HelixView.Children.Add(yAxis);

            var yLabel = new TextVisual3D
            {
                Position = new Point3D(0, 11, 0),
                Text = "Y (10m)",
                Height = 1,
                Foreground = System.Windows.Media.Brushes.Green
            };
            HelixView.Children.Add(yLabel);

            // Z軸
            var zAxis = new ArrowVisual3D
            {
                Point1 = new Point3D(0, 0, 0),
                Point2 = new Point3D(0, 0, 10),
                Diameter = 0.1,
                Fill = System.Windows.Media.Brushes.Blue,
                Material = Materials.Blue
            };
            HelixView.Children.Add(zAxis);

            var zLabel = new TextVisual3D
            {
                Position = new Point3D(0, 0, 11),
                Text = "Z (10m)",
                Height = 1,
                Foreground = System.Windows.Media.Brushes.Blue
            };
            HelixView.Children.Add(zLabel);
        }

    }
}
