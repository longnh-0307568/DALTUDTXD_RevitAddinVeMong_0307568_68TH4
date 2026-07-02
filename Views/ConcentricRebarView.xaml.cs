using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using AddinVeMong.ViewModels;

namespace AddinVeMong.Views
{
    public partial class ConcentricRebarView : Window
    {
        public class Footing
        {
            public Point Origin { get; set; }
            public double B { get; set; } // Chiều dài móng
            public double H { get; set; } // Chiều cao móng
            public double Bc { get; set; } // Chiều rộng cột
            public double Hc { get; set; } // Chiều cao cột
            public double Abv { get; set; } // Chiều dày lớp bảo vệ
            public int Diameter { get; set; } // Đường kính thép
            public int LongRebarSpacing { get; set; } // Khoảng cách thép dài
            public int LongRebarQuantity { get; set; } // Số lượng thép dài
            public int ShortRebarSpacing { get; set; } // Khoảng cách thép dài
            public int ShortRebarQuantity { get; set; } // Số lượng thép ngắn
            public double ColumnRebarH { get; set; } // Chiều cao thép cột
            public double StirrupSpacing { get; set; } // Khoảng cách thép đai
            public double StirrupQuantity { get; set; } // Số lượng thép đai
        }

        private ConcentricRebarViewModel _viewModel;

        public ConcentricRebarView(ConcentricRebarViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            this.DataContext = viewModel;

            this.Loaded += ConcentricRebarView_Loaded;
        }

        private void ConcentricRebarView_Loaded(object sender, RoutedEventArgs e)
        {
            if (_viewModel?.RebarData != null)
            {
                _viewModel.RebarData.PropertyChanged += RebarData_PropertyChanged;
                RenderPreview();
            }
        }

        private void RebarData_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // Chỉ lắng nghe sự thay đổi của Số lượng thép ngắn (ShortQuantity) và đai cột
            if (e.PropertyName == "ShortQuantity" || e.PropertyName == "StirrupSpacing")
            {
                this.Dispatcher.Invoke(() => { RenderPreview(); });
            }
        }

        private void RenderPreview()
        {
            if (_viewModel?.RebarData == null) return;

            // Tìm Canvas và các Path trong giao diện
            var canvasPreview = FindVisualChildByName<Canvas>(this, "canvas1");
            var pathProfile = FindVisualChildByName<System.Windows.Shapes.Path>(this, "PathProfile");
            var pathLongRebar = FindVisualChildByName<System.Windows.Shapes.Path>(this, "PathLongRebar");
            var pathShortRebar = FindVisualChildByName<System.Windows.Shapes.Path>(this, "PathShortRebar");
            var pathColumnRebar = FindVisualChildByName<System.Windows.Shapes.Path>(this, "PathColumnRebar");
            var pathStirrup = FindVisualChildByName<System.Windows.Shapes.Path>(this, "PathStirrup");

            if (pathProfile == null || pathShortRebar == null || pathLongRebar == null || pathColumnRebar == null || pathStirrup == null) return;

            Footing Mong1 = new Footing();
            Mong1.B = 400; // Chiều rộng móng
            Mong1.H = 140; // Chiều cao móng
            Mong1.Bc = 120; // chiều rộng cột
            Mong1.Hc = 200; // chiều cao cột
            Mong1.Abv = 20; // chiều dày lớp bảo vệ
            Mong1.Diameter = 10;
            Mong1.ShortRebarSpacing = 20;
            Mong1.ShortRebarQuantity = _viewModel.RebarData.ShortQuantity;
            Mong1.StirrupSpacing = 50;
            Mong1.LongRebarQuantity = 5;
            Mong1.LongRebarSpacing = 20;
            Mong1.ColumnRebarH = Mong1.H + Mong1.Hc + 30; // chiều cao thép chờ, vượt khỏi cột một đoạn 30
            Mong1.StirrupQuantity = 5; // số lượng thép đai

            var canvas1 = FindVisualChildByName<Canvas>(this, "canvas1");
            Mong1.Origin = new Point(canvas1.Width / 2, canvas1.Height / 1.3);


            #region

            // Vẽ móng

            // Bước 1: Tạo PathGeometry 
            PathGeometry Geo_profile = new PathGeometry();

            // Tạo PathFigure 
            PathFigure Figure_profile = new PathFigure { IsClosed = true };

            // Khai báo điểm đầu tiên: Point 1 (Góc trên - bên trái của đế móng)
            Figure_profile.StartPoint = new Point(Mong1.Origin.X - Mong1.B / 2, Mong1.Origin.Y - Mong1.H / 2);
            
            // Tạo đoạn thẳng chứa các điểm tiếp theo (PolyLineSegment)
            PolyLineSegment Segment_profile = new PolyLineSegment();

            // Điểm 2 Góc dưới - bên trái của đế móng
            Segment_profile.Points.Add(new Point(Mong1.Origin.X - Mong1.B / 2, Mong1.Origin.Y + Mong1.H / 2));
            // Điểm 3: Góc dưới - bên phải của đế móng
            Segment_profile.Points.Add(new Point(Mong1.Origin.X + Mong1.B / 2, Mong1.Origin.Y + Mong1.H / 2));
            // Điểm 4: Góc trên - bên phải của đế móng
            Segment_profile.Points.Add(new Point(Mong1.Origin.X + Mong1.B / 2, Mong1.Origin.Y - Mong1.H / 2));
            // Điểm 5: Điểm giao bên phải giữa cột và móng
            Segment_profile.Points.Add(new Point(Mong1.Origin.X + Mong1.Bc / 2, Mong1.Origin.Y - Mong1.H / 2));
            // Điểm 6: Đỉnh phía trên - bên phải của cột
            Segment_profile.Points.Add(new Point(Mong1.Origin.X + Mong1.Bc / 2, Mong1.Origin.Y - Mong1.H / 2 - Mong1.Hc));
            // Điểm 7: Đỉnh phía trên - bên trái của cột
            Segment_profile.Points.Add(new Point(Mong1.Origin.X - Mong1.Bc / 2, Mong1.Origin.Y - Mong1.H / 2 - Mong1.Hc));
            // Điểm 8: Điểm giao bên trái giữa cột và móng
            Segment_profile.Points.Add(new Point(Mong1.Origin.X - Mong1.Bc / 2, Mong1.Origin.Y - Mong1.H / 2));
            
            // Gán các phân đoạn vào Figure
            Figure_profile.Segments.Add(Segment_profile);
            // Thêm Figure vào hình học
            Geo_profile.Figures.Add(Figure_profile);
            pathProfile.Data = Geo_profile;

            // Vẽ thép cạnh dài (khối đường thẳng)
            double longRebarY = (Mong1.Origin.Y + Mong1.H / 2) - Mong1.Abv;

            // Điểm bắt đầu (bên trái) lùi vào một khoảng Abv so với mép móng trái
            double longRebarStartX = (Mong1.Origin.X - Mong1.B / 2) + Mong1.Abv;
            // Điểm kết thúc (bên phải) lùi vào một khoảng Abv so với mép móng phải
            double longRebarEndX = (Mong1.Origin.X + Mong1.B / 2) - Mong1.Abv;

            LineGeometry longRebarGeo = new LineGeometry(
                new Point(longRebarStartX, longRebarY),
                new Point(longRebarEndX, longRebarY)
            );
            // Gán vào đối tượng Path hiển thị thép dài trên XAML
            pathLongRebar.Data = longRebarGeo;

            // Vẽ thép cạnh ngắn
            GeometryGroup shortRebarGroup = new GeometryGroup();
            // Bán kính hình tròn thép cạnh ngắn
            double radius = (double)Mong1.Diameter / 2;
            // Tọa độ Y tâm của thép ngắn nằm sát phía trên trục của thép dài
            double shortRebarY = longRebarY - radius;
            // Bước dịch giữa các tâm hình tròn = Khoảng cách thông thủy + Đường kính thép
            double step = Mong1.ShortRebarSpacing + Mong1.Diameter;
            // Xác định tọa độ X của điểm chính giữa móng (Trục đối xứng)
            double midX = Mong1.Origin.X;

            // Vòng lặp vẽ số lượng thép cạnh ngắn đối xứng từ tâm móng (B) ra hai bên
            for (int i = 0; i < Mong1.ShortRebarQuantity; i++)
            {
                double currentX = midX + (i - (Mong1.ShortRebarQuantity - 1) / 2.0) * step;

                EllipseGeometry circleRebar = new EllipseGeometry();
                circleRebar.Center = new Point(currentX, shortRebarY);
                circleRebar.RadiusX = radius;
                circleRebar.RadiusY = radius;

                shortRebarGroup.Children.Add(circleRebar);
            }
            // Gán dữ liệu hình học vào thuộc tính Data của PathShortRebar
            pathShortRebar.Data = shortRebarGroup;

            // Thép chờ cổ cột
            PathGeometry colRebarGeo = new PathGeometry();
            double offsetFromCol = 10; // Khoảng cách từ mép cột vào tim thanh thép
            // Tọa độ Y vị trí đáy của thép chờ cột (ngay sát phía trên đỉnh hình tròn thép ngắn)
            double colRebarBotY = shortRebarY - radius;
            // Tọa độ Y vị trí đỉnh của thép chờ dựa trên biến chiều cao ColumnRebarH
            double colRebarTopY = colRebarBotY - Mong1.ColumnRebarH;
            // Chiều dài đoạn bẻ cong sang hai bên
            double hookLength = 50;

            // Thanh trái
            PathFigure leftBarFigure = new PathFigure { IsClosed = false }; // không bao đóng vì là đường thẳng
            // Tọa độ X thân thanh bên trái
            double leftBarX = (Mong1.Origin.X - Mong1.Bc / 2) + offsetFromCol;
            // Điểm bắt đầu vẽ từ chỗ bẻ sang trái
            leftBarFigure.StartPoint = new Point(leftBarX - hookLength, colRebarBotY);

            PolyLineSegment leftBarSegment = new PolyLineSegment();
            // Gập vào góc vuông của chân thép
            leftBarSegment.Points.Add(new Point(leftBarX, colRebarBotY));
            // Chạy thẳng lên trên đỉnh cột một đoạn 20 (Y)
            leftBarSegment.Points.Add(new Point(leftBarX, colRebarTopY + 20));
            leftBarFigure.Segments.Add(leftBarSegment);
            colRebarGeo.Figures.Add(leftBarFigure);

            // Thanh phải
            PathFigure rightBarFigure = new PathFigure { IsClosed = false };
            double rightBarX = (Mong1.Origin.X + Mong1.Bc / 2) - offsetFromCol;
            rightBarFigure.StartPoint = new Point(rightBarX + hookLength, colRebarBotY);

            PolyLineSegment rightBarSegment = new PolyLineSegment();
            rightBarSegment.Points.Add(new Point(rightBarX, colRebarBotY));
            rightBarSegment.Points.Add(new Point(rightBarX, colRebarTopY - 50));
            rightBarFigure.Segments.Add(rightBarSegment);
            colRebarGeo.Figures.Add(rightBarFigure);

            pathColumnRebar.Data = colRebarGeo;

            // Thép đai
            GeometryGroup stirrupGroup = new GeometryGroup();
            for (int i = 0; i < Mong1.StirrupQuantity; i++)
            {
                // Thanh đầu tiên nằm cách chân thép cột một khoảng bằng StirrupSpacing
                double currentStirrupY = colRebarBotY - (i + 1) * Mong1.StirrupSpacing;
                LineGeometry stirrupLine = new LineGeometry(
                    new Point(leftBarX, currentStirrupY),
                    new Point(rightBarX, currentStirrupY)
                );
                stirrupGroup.Children.Add(stirrupLine);
            }
            // Gán dữ liệu vào đối tượng Path hiển thị thép đai 
            pathStirrup.Data = stirrupGroup;

            #endregion
        }

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
