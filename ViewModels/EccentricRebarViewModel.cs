using AddinVeMong.Commands;
using AddinVeMong.Models;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace AddinVeMong.ViewModels
{
    public class EccentricRebarViewModel : INotifyPropertyChanged
    {
        private readonly ExternalCommandData _commandData;
        private readonly Document _doc;
        private readonly UIDocument _uiDoc;
        private readonly List<Element> _selectedFootings;

        // Đối tượng dữ liệu nhận liên kết (Binding) trực tiếp từ EccentricRebarView
        private FootingRebarModel _rebarData = new FootingRebarModel();
        public FootingRebarModel RebarData
        {
            get => _rebarData;
            set { _rebarData = value; OnPropertyChanged(); }
        }

        public ICommand DrawRebarCommand { get; }

        // Khởi tạo/Constructor
        public EccentricRebarViewModel(ExternalCommandData commandData, List<Element> selectedFootings)
        {
            _commandData = commandData;
            _uiDoc = commandData.Application.ActiveUIDocument;
            _doc = _uiDoc.Document;
            _selectedFootings = selectedFootings;

            DrawRebarCommand = new RelayCommand(ExecuteDrawRebar);
        }

        // --- HÀM THỰC THI CHÍNH KHI NHẤN NÚT "CREATE" ---
        private void ExecuteDrawRebar(object parameter)
        {
            try
            {
                View3D activeView3D = _doc.ActiveView as View3D;

                var barTypes = new FilteredElementCollector(_doc)
                    .OfClass(typeof(RebarBarType))
                    .Cast<RebarBarType>()
                    .ToList();

                if (barTypes.Count == 0)
                {
                    TaskDialog.Show("Lỗi", "Dự án không có bất kỳ RebarBarType nào. Vui lòng load một Family thép (Rebar) vào dự án trước.");
                    return;
                }

                RebarBarType longBarType = barTypes.FirstOrDefault(t => t.Name.Contains(RebarData.LongDiameter.ToString()) || Math.Abs(UnitUtils.ConvertFromInternalUnits(t.get_Parameter(BuiltInParameter.REBAR_BAR_DIAMETER).AsDouble(), UnitTypeId.Millimeters) - RebarData.LongDiameter) < 0.1);
                RebarBarType shortBarType = barTypes.FirstOrDefault(t => t.Name.Contains(RebarData.ShortDiameter.ToString()) || Math.Abs(UnitUtils.ConvertFromInternalUnits(t.get_Parameter(BuiltInParameter.REBAR_BAR_DIAMETER).AsDouble(), UnitTypeId.Millimeters) - RebarData.ShortDiameter) < 0.1);
                RebarBarType starterBarType = barTypes.FirstOrDefault(t => t.Name.Contains(RebarData.StarterDiameter.ToString()) || Math.Abs(UnitUtils.ConvertFromInternalUnits(t.get_Parameter(BuiltInParameter.REBAR_BAR_DIAMETER).AsDouble(), UnitTypeId.Millimeters) - RebarData.StarterDiameter) < 0.1);
                RebarBarType stirrupBarType = barTypes.FirstOrDefault(t => t.Name.Contains(RebarData.StirrupDiameter.ToString()) || Math.Abs(UnitUtils.ConvertFromInternalUnits(t.get_Parameter(BuiltInParameter.REBAR_BAR_DIAMETER).AsDouble(), UnitTypeId.Millimeters) - RebarData.StirrupDiameter) < 0.1);

                if (longBarType == null) longBarType = barTypes.FirstOrDefault();
                if (shortBarType == null) shortBarType = barTypes.FirstOrDefault();
                if (starterBarType == null) starterBarType = barTypes.FirstOrDefault();
                if (stirrupBarType == null) stirrupBarType = barTypes.FirstOrDefault();

                RebarStyle styleStandard = RebarStyle.Standard;
                RebarStyle styleStirrup = RebarStyle.StirrupTie;

                using (Transaction trans = new Transaction(_doc, "Vẽ thép móng lệch tâm"))
                {
                    trans.Start();

                    foreach (Element footing in _selectedFootings)
                    {
                        if (!(footing is FamilyInstance footingInstance)) continue;

                        ElementType footingType = _doc.GetElement(footing.GetTypeId()) as ElementType;
                        if (footingType == null) continue;

                        Parameter pLength = footingType.LookupParameter("Length");
                        Parameter pWidth = footingType.LookupParameter("Width");
                        Parameter pHeight = footingType.LookupParameter("Foundation Thickness") ?? footingType.LookupParameter("Thickness");

                        if (pLength == null || pWidth == null || pHeight == null) continue;

                        double footingLengthMm = UnitUtils.ConvertFromInternalUnits(pLength.AsDouble(), UnitTypeId.Millimeters);
                        double footingWidthMm = UnitUtils.ConvertFromInternalUnits(pWidth.AsDouble(), UnitTypeId.Millimeters);
                        double footingHeightMm = UnitUtils.ConvertFromInternalUnits(pHeight.AsDouble(), UnitTypeId.Millimeters);

                        Transform tf = footingInstance.GetTransform();
                        XYZ uX = tf.BasisX;
                        XYZ uY = tf.BasisY;
                        XYZ uZ = tf.BasisZ;
                        XYZ footingCenter = tf.Origin;

                        BoundingBoxXYZ bbox = footing.get_BoundingBox(null);

                        double coverFoot = UnitUtils.ConvertToInternalUnits(RebarData.Cover, UnitTypeId.Millimeters);
                        double lengthFoot = UnitUtils.ConvertToInternalUnits(footingLengthMm, UnitTypeId.Millimeters);
                        double widthFoot = UnitUtils.ConvertToInternalUnits(footingWidthMm, UnitTypeId.Millimeters);
                        double heightFoot = UnitUtils.ConvertToInternalUnits(footingHeightMm, UnitTypeId.Millimeters);

                        double zBottom = -heightFoot + coverFoot;
                        XYZ baseCenter = footingCenter + uZ * zBottom;

                        List<Curve> curvesLong = new();
                        List<Curve> curvesShort = new();
                        XYZ normalLong, normalShort;

                        double hookLong = UnitUtils.ConvertToInternalUnits(RebarData.LongHookLength, UnitTypeId.Millimeters);
                        double hookShort = UnitUtils.ConvertToInternalUnits(RebarData.ShortHookLength, UnitTypeId.Millimeters);
                        double longDiaFoot = UnitUtils.ConvertToInternalUnits(RebarData.LongDiameter, UnitTypeId.Millimeters);

                        double longSpacingFoot = UnitUtils.ConvertToInternalUnits(RebarData.LongSpacing, UnitTypeId.Millimeters);
                        double shortSpacingFoot = UnitUtils.ConvertToInternalUnits(RebarData.ShortSpacing, UnitTypeId.Millimeters);

                        double totalDistLong = (RebarData.LongQuantity > 1) ? (RebarData.LongQuantity - 1) * longSpacingFoot : 0;
                        double totalDistShort = (RebarData.ShortQuantity > 1) ? (RebarData.ShortQuantity - 1) * shortSpacingFoot : 0;

                        if (lengthFoot >= widthFoot)
                        {
                            double startX_Long = -totalDistLong / 2;
                            XYZ p2 = baseCenter + uX * startX_Long + uY * (-lengthFoot / 2 + coverFoot);
                            XYZ p3 = baseCenter + uX * startX_Long + uY * (lengthFoot / 2 - coverFoot);
                            XYZ p1 = p2 + uZ * hookLong;
                            XYZ p4 = p3 + uZ * hookLong;
                            curvesLong.AddRange(new[] { Line.CreateBound(p1, p2), Line.CreateBound(p2, p3), Line.CreateBound(p3, p4) });

                            normalLong = uX;

                            double startY_Short = -totalDistShort / 2;
                            XYZ baseCenterShort = footingCenter + uZ * (zBottom + longDiaFoot);
                            XYZ q2 = baseCenterShort + uX * (-widthFoot / 2 + coverFoot) + uY * startY_Short;
                            XYZ q3 = baseCenterShort + uX * (widthFoot / 2 - coverFoot) + uY * startY_Short;
                            XYZ q1 = q2 + uZ * hookShort;
                            XYZ q4 = q3 + uZ * hookShort;
                            curvesShort.AddRange(new[] { Line.CreateBound(q1, q2), Line.CreateBound(q2, q3), Line.CreateBound(q3, q4) });

                            normalShort = uY;
                        }
                        else
                        {
                            double startY_Long = -totalDistLong / 2;
                            XYZ p2 = baseCenter + uX * (-widthFoot / 2 + coverFoot) + uY * startY_Long;
                            XYZ p3 = baseCenter + uX * (widthFoot / 2 - coverFoot) + uY * startY_Long;
                            XYZ p1 = p2 + uZ * hookLong;
                            XYZ p4 = p3 + uZ * hookLong;
                            curvesLong.AddRange(new[] { Line.CreateBound(p1, p2), Line.CreateBound(p2, p3), Line.CreateBound(p3, p4) });

                            normalLong = uY;

                            double startX_Short = -totalDistShort / 2;
                            XYZ baseCenterShort = footingCenter + uZ * (zBottom + longDiaFoot);
                            XYZ q2 = baseCenterShort + uX * startX_Short + uY * (-lengthFoot / 2 + coverFoot);
                            XYZ q3 = baseCenterShort + uX * startX_Short + uY * (lengthFoot / 2 - coverFoot);
                            XYZ q1 = q2 + uZ * hookShort;
                            XYZ q4 = q3 + uZ * hookShort;
                            curvesShort.AddRange(new[] { Line.CreateBound(q1, q2), Line.CreateBound(q2, q3), Line.CreateBound(q3, q4) });

                            normalShort = uX;
                        }

                        if (longBarType != null && curvesLong.Count > 0)
                        {
                            Rebar rebarLong = Rebar.CreateFromCurves(_doc, styleStandard, longBarType, null, null, footing, normalLong, curvesLong, RebarHookOrientation.Right, RebarHookOrientation.Right, true, true);
                            if (rebarLong != null)
                            {
                                rebarLong.GetShapeDrivenAccessor().SetLayoutAsFixedNumber(RebarData.LongQuantity, totalDistLong, true, true, true);
                                SetRebarSolid3D(rebarLong, activeView3D);
                            }
                        }

                        if (shortBarType != null && curvesShort.Count > 0)
                        {
                            Rebar rebarShort = Rebar.CreateFromCurves(_doc, styleStandard, shortBarType, null, null, footing, normalShort, curvesShort, RebarHookOrientation.Right, RebarHookOrientation.Right, true, true);
                            if (rebarShort != null)
                            {
                                rebarShort.GetShapeDrivenAccessor().SetLayoutAsFixedNumber(RebarData.ShortQuantity, totalDistShort, true, true, true);
                                SetRebarSolid3D(rebarShort, activeView3D);
                            }
                        }

                        double eccXFoot = UnitUtils.ConvertToInternalUnits(RebarData.EccentricityX, UnitTypeId.Millimeters);
                        double eccYFoot = UnitUtils.ConvertToInternalUnits(RebarData.EccentricityY, UnitTypeId.Millimeters);
                        XYZ eccentricCenter = footingCenter + uX * eccXFoot + uY * eccYFoot;

                        double colXFoot = UnitUtils.ConvertToInternalUnits(RebarData.ColumnWidthX, UnitTypeId.Millimeters);
                        double colYFoot = UnitUtils.ConvertToInternalUnits(RebarData.ColumnWidthY, UnitTypeId.Millimeters);
                        double starterHookFoot = UnitUtils.ConvertToInternalUnits(RebarData.StarterHookLength, UnitTypeId.Millimeters);

                        double zStarterBottom = zBottom + longDiaFoot + UnitUtils.ConvertToInternalUnits(RebarData.ShortDiameter, UnitTypeId.Millimeters);
                        double starterLenFoot = UnitUtils.ConvertToInternalUnits(RebarData.StarterLength, UnitTypeId.Millimeters) + Math.Abs(zStarterBottom);

                        XYZ corner1 = eccentricCenter + uX * (colXFoot / 2 - coverFoot) + uY * (colYFoot / 2 - coverFoot);
                        XYZ corner2 = eccentricCenter - uX * (colXFoot / 2 - coverFoot) + uY * (colYFoot / 2 - coverFoot);
                        XYZ corner3 = eccentricCenter - uX * (colXFoot / 2 - coverFoot) - uY * (colYFoot / 2 - coverFoot);
                        XYZ corner4 = eccentricCenter + uX * (colXFoot / 2 - coverFoot) - uY * (colYFoot / 2 - coverFoot);

                        List<XYZ> starterCorners = new List<XYZ> { corner1, corner2, corner3, corner4 };

                        double deltaShortLenFoot = UnitUtils.ConvertToInternalUnits(150, UnitTypeId.Millimeters); // Khoảng chênh lệch giữa thanh dài và ngắn (150mm)

                        for (int i = 0; i < starterCorners.Count; i++)
                        {
                            XYZ cornerPt = starterCorners[i];
                            XYZ basePt = new XYZ(cornerPt.X, cornerPt.Y, zStarterBottom);

                            // Phân loại chiều dài: i = 0, 2 (thanh dài) | i = 1, 3 (thanh ngắn)
                            double currentLen = starterLenFoot;
                            if (i == 1 || i == 3)
                            {
                                currentLen -= deltaShortLenFoot;
                            }

                            XYZ starterTopPoint = basePt + uZ * currentLen;

                            List<Curve> starterCurves = new List<Curve>();
                            XYZ hookDirection = (cornerPt - eccentricCenter).Normalize();
                            XYZ pHook = basePt + hookDirection * starterHookFoot; // Đổi hookStart -> pHook

                            starterCurves.Add(Line.CreateBound(pHook, basePt));
                            starterCurves.Add(Line.CreateBound(basePt, starterTopPoint));

                            XYZ sNormal = hookDirection.CrossProduct(uZ).Normalize();

                            if (starterBarType != null)
                            {
                                Rebar starterRebar = Rebar.CreateFromCurves(_doc, styleStandard, starterBarType, null, null, footing, sNormal, starterCurves, RebarHookOrientation.Right, RebarHookOrientation.Right, true, true);
                                if (starterRebar != null) SetRebarSolid3D(starterRebar, activeView3D);
                            }
                        }

                        CurveLoop stirrupLoop = new CurveLoop();
                        XYZ stirrupBaseCenter = eccentricCenter + uZ * zStarterBottom;

                        XYZ s1 = stirrupBaseCenter + uX * (colXFoot / 2) + uY * (colYFoot / 2);
                        XYZ s2 = stirrupBaseCenter + uX * (-colXFoot / 2) + uY * (colYFoot / 2);
                        XYZ s3 = stirrupBaseCenter + uX * (-colXFoot / 2) + uY * (-colYFoot / 2);
                        XYZ s4 = stirrupBaseCenter + uX * (colXFoot / 2) + uY * (-colYFoot / 2);

                        stirrupLoop.Append(Line.CreateBound(s1, s2));
                        stirrupLoop.Append(Line.CreateBound(s2, s3));
                        stirrupLoop.Append(Line.CreateBound(s3, s4));
                        stirrupLoop.Append(Line.CreateBound(s4, s1));

                        List<Curve> finalStirrupProfile = stirrupLoop.ToList();

                        if (stirrupBarType != null && finalStirrupProfile.Count > 0)
                        {
                            Rebar stirrupRebar = Rebar.CreateFromCurves(_doc, styleStirrup, stirrupBarType, null, null, footing, uZ, finalStirrupProfile, RebarHookOrientation.Right, RebarHookOrientation.Right, true, true);
                            if (stirrupRebar != null)
                            {
                                double stirrupSpcFoot = UnitUtils.ConvertToInternalUnits(RebarData.StirrupSpacing, UnitTypeId.Millimeters);
                                double heightInsideFooting = Math.Abs(zStarterBottom - bbox.Max.Z);
                                stirrupRebar.GetShapeDrivenAccessor().SetLayoutAsMaximumSpacing(stirrupSpcFoot, heightInsideFooting, true, true, true);
                                SetRebarSolid3D(stirrupRebar, activeView3D);
                            }
                        }
                    }

                    trans.Commit();
                }

                if (parameter is Window window)
                {
                    window.DialogResult = true;
                    window.Close();
                }
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Lỗi hệ thống", $"Có lỗi xảy ra: {ex.Message}");
            }
        }

        protected void SetRebarSolid3D(Rebar rebar, View3D view3D)
        {
            if (rebar == null || view3D == null) return;
            try
            {
                rebar.SetUnobscuredInView(view3D, true);
                Parameter solidParam = rebar.LookupParameter("Solid In View");
                if (solidParam != null && !solidParam.IsReadOnly)
                {
                    solidParam.Set(1);
                }
            }
            catch { }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}