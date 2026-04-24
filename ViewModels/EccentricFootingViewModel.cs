using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace AddinVeMong.ViewModels
{
    public class EccentricFootingViewModel : INotifyPropertyChanged
    {
        private ExternalCommandData _commandData;

        // Kích thước đáy (Base)
        private double _baseLength = 2000;
        private double _baseWidth = 2000;

        // Kích thước đỉnh/cổ móng (Top)
        private double _topLength = 500;
        private double _topWidth = 500;

        // Độ lệch tâm (Eccentricity)
        private double _eccentricX = 400;
        private double _eccentricY = 400;

        // Chiều cao
        private double _hBase = 300;
        private double _hTaper = 500;

        public double BaseLength { get => _baseLength; set { _baseLength = value; OnPropertyChanged(); } }
        public double BaseWidth { get => _baseWidth; set { _baseWidth = value; OnPropertyChanged(); } }
        public double TopLength { get => _topLength; set { _topLength = value; OnPropertyChanged(); } }
        public double TopWidth { get => _topWidth; set { _topWidth = value; OnPropertyChanged(); } }
        public double EccentricX { get => _eccentricX; set { _eccentricX = value; OnPropertyChanged(); } }
        public double EccentricY { get => _eccentricY; set { _eccentricY = value; OnPropertyChanged(); } }
        public double HBase { get => _hBase; set { _hBase = value; OnPropertyChanged(); } }
        public double HTaper { get => _hTaper; set { _hTaper = value; OnPropertyChanged(); } }

        public ICommand DrawCommand { get; }
        public Action? CloseAction { get; set; }

        public EccentricFootingViewModel(ExternalCommandData commandData)
        {
            _commandData = commandData;
            DrawCommand = new RelayCommand(ExecuteDraw);
        }

        private void ExecuteDraw(object? obj)
        {
            CloseAction?.Invoke();

            UIDocument uidoc = _commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            try
            {
                View view = doc.ActiveView;
                if (!(view is ViewPlan plan))
                {
                    TaskDialog.Show("Lỗi", "Phải chạy trong mặt bằng (Plan View)");
                    return;
                }

                Level level = plan.GenLevel;
                double z = level.Elevation;

                XYZ p = uidoc.Selection.PickPoint("Chọn điểm đặt móng (tâm đáy)");

                using (Transaction t = new Transaction(doc, "Vẽ móng vát lệch tâm"))
                {
                    t.Start();
                    CreateEccentricFooting(doc, p, z);
                    t.Commit();
                }

                TaskDialog.Show("Thành công", "Đã vẽ móng vát lệch tâm!");
            }
            catch (Exception ex)
            {
                if (ex is Autodesk.Revit.Exceptions.OperationCanceledException) return;
                TaskDialog.Show("Lỗi", ex.Message);
            }
        }

        private void CreateEccentricFooting(Document doc, XYZ center, double zElevation)
        {
            // Chuyển đổi mm -> feet
            double bL = BaseLength / 304.8;
            double bW = BaseWidth / 304.8;
            double tL = TopLength / 304.8;
            double tW = TopWidth / 304.8;
            double eX = EccentricX / 304.8;
            double eY = EccentricY / 304.8;
            double hB = HBase / 304.8;
            double hT = HTaper / 304.8;

            double z0 = zElevation;
            double z1 = z0 + hB;
            double z2 = z1 + hT;

            // 1. Phần đế (Extrusion)
            CurveLoop baseLoop = CreateRectLoop(center.X, center.Y, bL, bW, z0);
            Solid baseSolid = GeometryCreationUtilities.CreateExtrusionGeometry(
                new List<CurveLoop> { baseLoop }, XYZ.BasisZ, hB);

            // 2. Phần vát (Loft)
            CurveLoop taperBottomLoop = CreateRectLoop(center.X, center.Y, bL, bW, z1);
            // Tâm đỉnh dịch chuyển theo độ lệch tâm
            CurveLoop taperTopLoop = CreateRectLoop(center.X + eX, center.Y + eY, tL, tW, z2);

            Solid taperSolid = GeometryCreationUtilities.CreateLoftGeometry(
                new List<CurveLoop> { taperBottomLoop, taperTopLoop }, 
                new SolidOptions(ElementId.InvalidElementId, ElementId.InvalidElementId));

            // Hiển thị trong Revit
            List<GeometryObject> shapes = new List<GeometryObject> { baseSolid, taperSolid };
            DirectShape ds = DirectShape.CreateElement(doc, new ElementId(BuiltInCategory.OST_StructuralFoundation));
            ds.SetShape(shapes);
        }

        private CurveLoop CreateRectLoop(double x, double y, double L, double W, double Z)
        {
            double hL = L / 2;
            double hW = W / 2;
            XYZ p1 = new XYZ(x - hL, y - hW, Z);
            XYZ p2 = new XYZ(x + hL, y - hW, Z);
            XYZ p3 = new XYZ(x + hL, y + hW, Z);
            XYZ p4 = new XYZ(x - hL, y + hW, Z);

            CurveLoop loop = new CurveLoop();
            loop.Append(Line.CreateBound(p1, p2));
            loop.Append(Line.CreateBound(p2, p3));
            loop.Append(Line.CreateBound(p3, p4));
            loop.Append(Line.CreateBound(p4, p1));
            return loop;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
