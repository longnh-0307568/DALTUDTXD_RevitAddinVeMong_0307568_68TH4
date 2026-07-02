using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using AddinVeMong.ViewModels;

namespace AddinVeMong.Views
{
    public partial class EccentricRebarView : Window
    {
        // Khai báo cấu trúc lưu trữ thông số hình học giả lập để phục vụ vẽ đồ họa preview
        public class Footing
        {
            public Point Origin { get; set; }
            public double B { get; set; }
            public double H { get; set; }
            public double Bc { get; set; }
            public double Hc { get; set; }
            public double Abv { get; set; }
            public int Diameter { get; set; }
            public int LongRebarSpacing { get; set; }
            public int LongRebarQuantity { get; set; }
            public int ShortRebarSpacing { get; set; }
            public int ShortRebarQuantity { get; set; }
            public double ColumnRebarH { get; set; }
            public double StirrupSpacing { get; set; }
            public double StirrupQuantity { get; set; }
        }

        private EccentricRebarViewModel _viewModel;

        // Cập nhật hàm khởi tạo nhận tham số ViewModel giống hệt bên file đồng tâm
        public EccentricRebarView(EccentricRebarViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            this.DataContext = viewModel;

            // Đăng ký sự kiện Loaded để tính toán vẽ khi giao diện sẵn sàng
            this.Loaded += EccentricRebarView_Loaded;
        }

        private void EccentricRebarView_Loaded(object sender, RoutedEventArgs e)
        {
            if (_viewModel?.RebarData != null)
            {
                // Lắng nghe sự thay đổi của người dùng trên giao diện để cập nhật hình vẽ
                _viewModel.RebarData.PropertyChanged += RebarData_PropertyChanged;
                RenderPreview();
            }
        }

        private void RebarData_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // Chỉ kích hoạt vẽ lại khi số lượng thép ngắn (ShortQuantity) hoặc khoảng cách đai (StirrupSpacing) thay đổi
            if (e.PropertyName == "ShortQuantity" || e.PropertyName == "StirrupSpacing")
            {
                this.Dispatcher.Invoke(() => { RenderPreview(); });
            }
        }

        private void RenderPreview()
        {
            if (_viewModel?.RebarData == null) return;

            // Truy tìm CPath nằm bên trong DataTemplate bằng tên x:Name
            var canvasPreview = FindVisualChildByName<Canvas>(this, "canvas1");
            var pathProfile = FindVisualChildByName<System.Windows.Shapes.Path>(this, "PathProfile");
            var pathLongRebar = FindVisualChildByName<System.Windows.Shapes.Path>(this, "PathLongRebar");
            var pathShortRebar = FindVisualChildByName<System.Windows.Shapes.Path>(this, "PathShortRebar");
            var pathColumnRebar = FindVisualChildByName<System.Windows.Shapes.Path>(this, "PathColumnRebar");
            var pathStirrup = FindVisualChildByName<System.Windows.Shapes.Path>(this, "PathStirrup");

            // Đảm bảo tìm thấy đầy đủ các control trước khi vẽ để tránh crash ứng dụng
            if (pathProfile == null || pathShortRebar == null || pathLongRebar == null || pathColumnRebar == null || pathStirrup == null || canvasPreview == null) return;

            Footing Mong1 = new Footing();
            Mong1.B = 400;
            Mong1.H = 140;
            Mong1.Bc = 120;
            Mong1.Hc = 200;
            Mong1.Abv = 20;
            Mong1.Diameter = 10;
            Mong1.ShortRebarSpacing = 20;
            Mong1.ShortRebarQuantity = _viewModel.RebarData.ShortQuantity;
            Mong1.StirrupSpacing = 50;
            Mong1.LongRebarQuantity = 5;
            Mong1.LongRebarSpacing = 20;
            Mong1.ColumnRebarH = Mong1.H + Mong1.Hc + 30; // Thép chờ vượt lên trên cổ cột một đoạn 30
            Mong1.StirrupQuantity = 5;

            // Xác định điểm gốc tọa độ để đặt khối móng nằm cân đối trên Canvas
            Mong1.Origin = new Point(canvasPreview.Width / 2, canvasPreview.Height / 1.3);

            #region Tiến hành vẽ đồ họa bằng WPF Geometry

            // 1. Vẽ đường viền hình dạng bê tông móng và cổ cột
            PathGeometry Geo_profile = new PathGeometry();
            PathFigure Figure_profile = new PathFigure { IsClosed = true };
            Figure_profile.StartPoint = new Point(Mong1.Origin.X - Mong1.B / 2, Mong1.Origin.Y - Mong1.H / 2);

            PolyLineSegment Segment_profile = new PolyLineSegment();
            Segment_profile.Points.Add(new Point(Mong1.Origin.X - Mong1.B / 2, Mong1.Origin.Y + Mong1.H / 2));
            Segment_profile.Points.Add(new Point(Mong1.Origin.X + Mong1.B / 2, Mong1.Origin.Y + Mong1.H / 2));
            Segment_profile.Points.Add(new Point(Mong1.Origin.X + Mong1.B / 2, Mong1.Origin.Y - Mong1.H / 2));
            Segment_profile.Points.Add(new Point(Mong1.Origin.X + Mong1.Bc / 2, Mong1.Origin.Y - Mong1.H / 2));
            Segment_profile.Points.Add(new Point(Mong1.Origin.X + Mong1.Bc / 2, Mong1.Origin.Y - Mong1.H / 2 - Mong1.Hc));
            Segment_profile.Points.Add(new Point(Mong1.Origin.X - Mong1.Bc / 2, Mong1.Origin.Y - Mong1.H / 2 - Mong1.Hc));
            Segment_profile.Points.Add(new Point(Mong1.Origin.X - Mong1.Bc / 2, Mong1.Origin.Y - Mong1.H / 2));

            Figure_profile.Segments.Add(Segment_profile);
            Geo_profile.Figures.Add(Figure_profile);
            pathProfile.Data = Geo_profile;

            // 2. Vẽ thanh thép dọc lớp dưới chạy ngang đáy (Thép phương dài)
            double longRebarY = (Mong1.Origin.Y + Mong1.H / 2) - Mong1.Abv;
            double longRebarStartX = (Mong1.Origin.X - Mong1.B / 2) + Mong1.Abv;
            double longRebarEndX = (Mong1.Origin.X + Mong1.B / 2) - Mong1.Abv;

            LineGeometry longRebarGeo = new LineGeometry(
                new Point(longRebarStartX, longRebarY),
                new Point(longRebarEndX, longRebarY)
            );
            pathLongRebar.Data = longRebarGeo;

            // 3. Vẽ các chấm tròn thể hiện mặt cắt các thanh thép phương ngắn
            GeometryGroup shortRebarGroup = new GeometryGroup();
            double radius = (double)Mong1.Diameter / 2;
            double shortRebarY = longRebarY - radius;
            double step = Mong1.ShortRebarSpacing + Mong1.Diameter;
            double midX = Mong1.Origin.X;

            for (int i = 0; i < Mong1.ShortRebarQuantity; i++)
            {
                double currentX = midX + (i - (Mong1.ShortRebarQuantity - 1) / 2.0) * step;

                EllipseGeometry circleRebar = new EllipseGeometry();
                circleRebar.Center = new Point(currentX, shortRebarY);
                circleRebar.RadiusX = radius;
                circleRebar.RadiusY = radius;

                shortRebarGroup.Children.Add(circleRebar);
            }
            pathShortRebar.Data = shortRebarGroup;

            // 4. Vẽ 2 thanh thép chờ dọc cổ cột
            PathGeometry colRebarGeo = new PathGeometry();
            double offsetFromCol = 10;
            double colRebarBotY = shortRebarY - radius;
            double colRebarTopY = colRebarBotY - Mong1.ColumnRebarH;
            double hookLength = 50;

            // Vẽ thanh bên trái
            PathFigure leftBarFigure = new PathFigure { IsClosed = false };
            double leftBarX = (Mong1.Origin.X - Mong1.Bc / 2) + offsetFromCol;
            leftBarFigure.StartPoint = new Point(leftBarX - hookLength, colRebarBotY);

            PolyLineSegment leftBarSegment = new PolyLineSegment();
            leftBarSegment.Points.Add(new Point(leftBarX, colRebarBotY));
            leftBarSegment.Points.Add(new Point(leftBarX, colRebarTopY + 20));
            leftBarFigure.Segments.Add(leftBarSegment);
            colRebarGeo.Figures.Add(leftBarFigure);

            // Vẽ thanh bên phải
            PathFigure rightBarFigure = new PathFigure { IsClosed = false };
            double rightBarX = (Mong1.Origin.X + Mong1.Bc / 2) - offsetFromCol;
            rightBarFigure.StartPoint = new Point(rightBarX + hookLength, colRebarBotY);

            PolyLineSegment rightBarSegment = new PolyLineSegment();
            rightBarSegment.Points.Add(new Point(rightBarX, colRebarBotY));
            rightBarSegment.Points.Add(new Point(rightBarX, colRebarTopY - 50));
            rightBarFigure.Segments.Add(rightBarSegment);
            colRebarGeo.Figures.Add(rightBarFigure);

            pathColumnRebar.Data = colRebarGeo;

            // 5. Vẽ hệ thống các thanh thép đai ngang trong cổ cột
            GeometryGroup stirrupGroup = new GeometryGroup();
            for (int i = 0; i < Mong1.StirrupQuantity; i++)
            {
                double currentStirrupY = colRebarBotY - (i + 1) * Mong1.StirrupSpacing;
                LineGeometry stirrupLine = new LineGeometry(
                    new Point(leftBarX, currentStirrupY),
                    new Point(rightBarX, currentStirrupY)
                );
                stirrupGroup.Children.Add(stirrupLine);
            }
            pathStirrup.Data = stirrupGroup;

            #endregion
        }

        // Hàm tìm kiếm chính xác Control đồ họa được ẩn trong DataTemplate bằng tên định danh
        private T FindVisualChildByName<T>(DependencyObject parent, string name) where T : DependencyObject
        {
            if (parent == null) return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                string controlName = child.GetValue(FrameworkElement.NameProperty) as string;

                if (controlName == name && child is T)
                {
                    return (T)child;
                }

                T childOfChild = FindVisualChildByName<T>(child, name);
                if (childOfChild != null) return childOfChild;
            }
            return null;
        }
    }
}