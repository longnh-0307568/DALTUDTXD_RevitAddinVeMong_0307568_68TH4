using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using AddinVeMong.ViewModels;

namespace AddinVeMong.ViewModels
{
    public class FootingViewModel : INotifyPropertyChanged
    {
        private ExternalCommandData _commandData;

        // Kích thước (mm)
        private double _baseSize = 4000;
        private double _topSize = 1200;

        private double _hBase = 300;
        private double _hStraight = 300;
        private double _hChamfer = 500;

        public double BaseSize { get => _baseSize; set { _baseSize = value; OnPropertyChanged(); } }
        public double TopSize { get => _topSize; set { _topSize = value; OnPropertyChanged(); } }

        public double HBase { get => _hBase; set { _hBase = value; OnPropertyChanged(); } }
        public double HStraight { get => _hStraight; set { _hStraight = value; OnPropertyChanged(); } }
        public double HChamfer { get => _hChamfer; set { _hChamfer = value; OnPropertyChanged(); } }

        public ICommand DrawCommand { get; }

        public FootingViewModel(ExternalCommandData commandData)
        {
            _commandData = commandData;
            DrawCommand = new RelayCommand(ExecuteDraw);
        }

        private void ExecuteDraw(object? obj)
        {
            Window? window = obj as Window;
            if (window != null)
            {
                window.Hide(); // Ẩn cửa sổ để chọn điểm trong Revit
            }

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

                List<XYZ> points = new List<XYZ>();

                // Chọn điểm
                while (true)
                {
                    try
                    {
                        XYZ p = uidoc.Selection.PickPoint("Chọn điểm (ESC để kết thúc)");
                        points.Add(p);
                    }
                    catch
                    {
                        break; // ESC
                    }
                }

                if (points.Count > 0)
                {
                    using (Transaction t = new Transaction(doc, "Create Chamfer Footing"))
                    {
                        t.Start();
                        foreach (XYZ p in points)
                        {
                            XYZ center = new XYZ(p.X, p.Y, z);
                            CreateChamferFooting(doc, center);
                        }
                        t.Commit();
                    }
                    TaskDialog.Show("OK", $"Đã vẽ {points.Count} móng vát!");
                }
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Lỗi", ex.Message);
            }
            finally
            {
                if (window != null)
                {
                    window.Show(); // Hiện lại cửa sổ sau khi vẽ xong
                }
            }
        }

        private void CreateChamferFooting(Document doc, XYZ center)
        {
            // Chuyển đổi sang feet
            double bSize = BaseSize / 304.8;
            double tSize = TopSize / 304.8;
            double hB = HBase / 304.8;
            double hS = HStraight / 304.8;
            double hC = HChamfer / 304.8;

            double z0 = center.Z;
            double z1 = z0 + hB;
            double z2 = z1 + hS;
            double z3 = z2 + hC;

            // Khối 1: Đế
            CurveLoop baseLoop = CreateSquare(center, bSize, z0);
            Solid solid1 = GeometryCreationUtilities.CreateExtrusionGeometry(
                new List<CurveLoop> { baseLoop },
                XYZ.BasisZ,
                hB + hS
            );

            // Khối 2: Phần vát
            CurveLoop loopBottomChamfer = CreateSquare(center, bSize, z2);
            CurveLoop loopTopChamfer = CreateSquare(center, tSize, z3);

            SolidOptions opt = new SolidOptions(ElementId.InvalidElementId, ElementId.InvalidElementId);
            Solid solid2 = GeometryCreationUtilities.CreateLoftGeometry(
                new List<CurveLoop> { loopBottomChamfer, loopTopChamfer },
                opt
            );

            List<GeometryObject> solids = new List<GeometryObject>() { solid1, solid2 };
            DirectShape ds = DirectShape.CreateElement(doc, new ElementId(BuiltInCategory.OST_StructuralFoundation));
            ds.SetShape(solids);
        }

        private CurveLoop CreateSquare(XYZ center, double size, double z)
        {
            double half = size / 2;
            XYZ p1 = new XYZ(center.X - half, center.Y - half, z);
            XYZ p2 = new XYZ(center.X + half, center.Y - half, z);
            XYZ p3 = new XYZ(center.X + half, center.Y + half, z);
            XYZ p4 = new XYZ(center.X - half, center.Y + half, z);

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
