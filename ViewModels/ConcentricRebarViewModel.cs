using AddinVeMong.Commands;
using AddinVeMong.Models;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using System.Windows;
using System.Windows.Input;

namespace AddinVeMong.ViewModels
{
    public class ConcentricRebarViewModel
    {
        private readonly Document _doc;
        private readonly List<Element> _selectedFootings;

        public FootingRebarModel RebarData { get; set; } = new();
        public ICommand DrawRebarCommand { get; }

        public ConcentricRebarViewModel(ExternalCommandData commandData, List<Element> footings)
        {
            _doc = commandData.Application.ActiveUIDocument.Document;
            _selectedFootings = footings;
            DrawRebarCommand = new RelayCommand(ExecuteDrawRebar);
        }

        private void ExecuteDrawRebar(object parameter)
        {
            if (_selectedFootings == null || _selectedFootings.Count == 0)
            {
                TaskDialog.Show("Thông báo", "Không tìm thấy dữ liệu móng được chọn.");
                return;
            }

            using (Transaction trans = new(_doc, "Tạo thép móng hàng loạt"))
            {
                trans.Start();

                var barTypes = new FilteredElementCollector(_doc)
                    .OfClass(typeof(RebarBarType))
                    .Cast<RebarBarType>()
                    .ToList();

                if (barTypes.Count == 0)
                {
                    TaskDialog.Show("Thiếu dữ liệu", "Dự án chưa được tải Family thép.");
                    trans.RollBack();
                    return;
                }

                RebarBarType defType = barTypes.FirstOrDefault();
                RebarBarType longBarType = barTypes.FirstOrDefault(t => t.Name.Contains(RebarData.LongDiameter.ToString()) || Math.Abs(UnitUtils.ConvertFromInternalUnits(t.get_Parameter(BuiltInParameter.REBAR_BAR_DIAMETER).AsDouble(), UnitTypeId.Millimeters) - RebarData.LongDiameter) < 0.1) ?? defType;
                RebarBarType shortBarType = barTypes.FirstOrDefault(t => t.Name.Contains(RebarData.ShortDiameter.ToString()) || Math.Abs(UnitUtils.ConvertFromInternalUnits(t.get_Parameter(BuiltInParameter.REBAR_BAR_DIAMETER).AsDouble(), UnitTypeId.Millimeters) - RebarData.ShortDiameter) < 0.1) ?? defType;
                RebarBarType starterBarType = barTypes.FirstOrDefault(t => t.Name.Contains(RebarData.StarterDiameter.ToString()) || Math.Abs(UnitUtils.ConvertFromInternalUnits(t.get_Parameter(BuiltInParameter.REBAR_BAR_DIAMETER).AsDouble(), UnitTypeId.Millimeters) - RebarData.StarterDiameter) < 0.1) ?? defType;
                RebarBarType stirrupBarType = barTypes.FirstOrDefault(t => t.Name.Contains(RebarData.StirrupDiameter.ToString()) || Math.Abs(UnitUtils.ConvertFromInternalUnits(t.get_Parameter(BuiltInParameter.REBAR_BAR_DIAMETER).AsDouble(), UnitTypeId.Millimeters) - RebarData.StirrupDiameter) < 0.1) ?? defType;

                foreach (Element footingElement in _selectedFootings)
                {
                    if (footingElement is not FamilyInstance footingInstance) continue;

                    Element columnElement = null;
                    var boundingBox = footingElement.get_BoundingBox(null);
                    if (boundingBox != null)
                    {
                        Outline outline = new(boundingBox.Min, boundingBox.Max + new XYZ(0.1, 0.1, 2.0));
                        var connectedColumns = new FilteredElementCollector(_doc)
                            .OfCategory(BuiltInCategory.OST_StructuralColumns)
                            .OfClass(typeof(FamilyInstance))
                            .WherePasses(new BoundingBoxIntersectsFilter(outline))
                            .ToList();

                        if (connectedColumns.Count > 0) columnElement = connectedColumns.First();
                    }

                    Element columnHostElement = columnElement ?? footingElement;

                    if (_doc.GetElement(footingElement.GetTypeId()) is not ElementType footingType) continue;

                    Parameter pLength = footingType.LookupParameter("Length");
                    Parameter pWidth = footingType.LookupParameter("Width");
                    Parameter pHeight = footingType.LookupParameter("Foundation Thickness") ?? footingType.LookupParameter("Thickness");

                    if (pLength == null || pWidth == null || pHeight == null) continue;

                    double coverFoot = UnitUtils.ConvertToInternalUnits(RebarData.Cover, UnitTypeId.Millimeters);
                    double lengthFoot = pLength.AsDouble();
                    double widthFoot = pWidth.AsDouble();
                    double heightFoot = pHeight.AsDouble();

                    Transform tf = footingInstance.GetTransform();
                    XYZ uX = tf.BasisX, uY = tf.BasisY, uZ = tf.BasisZ, footingCenter = tf.Origin;

                    double zBottom = -heightFoot + coverFoot;
                    XYZ baseCenter = footingCenter + uZ * zBottom;

                    double hookLong = UnitUtils.ConvertToInternalUnits(RebarData.LongHookLength, UnitTypeId.Millimeters);
                    double hookShort = UnitUtils.ConvertToInternalUnits(RebarData.ShortHookLength, UnitTypeId.Millimeters);
                    double longDiaFoot = UnitUtils.ConvertToInternalUnits(RebarData.LongDiameter, UnitTypeId.Millimeters);

                    double longSpacingFoot = UnitUtils.ConvertToInternalUnits(RebarData.LongSpacing, UnitTypeId.Millimeters);
                    double shortSpacingFoot = UnitUtils.ConvertToInternalUnits(RebarData.ShortSpacing, UnitTypeId.Millimeters);

                    double totalDistLong = (RebarData.LongQuantity > 1) ? (RebarData.LongQuantity - 1) * longSpacingFoot : 0;
                    double totalDistShort = (RebarData.ShortQuantity > 1) ? (RebarData.ShortQuantity - 1) * shortSpacingFoot : 0;

                    // thép phương dài
                    double startX_Long = -totalDistLong / 2;
                    XYZ p2 = baseCenter + uX * startX_Long + uY * (-lengthFoot / 2 + coverFoot);
                    XYZ p3 = baseCenter + uX * startX_Long + uY * (lengthFoot / 2 - coverFoot);
                    List<Curve> curvesLong = new() { Line.CreateBound(p2 + uZ * hookLong, p2), Line.CreateBound(p2, p3), Line.CreateBound(p3, p3 + uZ * hookLong) };

                    if (longBarType != null && curvesLong.Count > 0)
                    {
                        Rebar rebarLong = Rebar.CreateFromCurves(_doc, RebarStyle.Standard, longBarType, null, null, footingElement, uX, curvesLong, RebarHookOrientation.Right, RebarHookOrientation.Right, true, true);
                        rebarLong?.GetShapeDrivenAccessor().SetLayoutAsFixedNumber(RebarData.LongQuantity, totalDistLong, true, true, true);
                    }

                    // thép thương ngắn
                    double startY_Short = -totalDistShort / 2;
                    XYZ baseCenterShort = footingCenter + uZ * (zBottom + longDiaFoot);
                    XYZ q2 = baseCenterShort + uX * (-widthFoot / 2 + coverFoot) + uY * startY_Short;
                    XYZ q3 = baseCenterShort + uX * (widthFoot / 2 - coverFoot) + uY * startY_Short;
                    List<Curve> curvesShort = new() { Line.CreateBound(q2 + uZ * hookShort, q2), Line.CreateBound(q2, q3), Line.CreateBound(q3, q3 + uZ * hookShort) };

                    if (shortBarType != null && curvesShort.Count > 0)
                    {
                        Rebar rebarShort = Rebar.CreateFromCurves(_doc, RebarStyle.Standard, shortBarType, null, null, footingElement, uY, curvesShort, RebarHookOrientation.Right, RebarHookOrientation.Right, true, true);
                        rebarShort?.GetShapeDrivenAccessor().SetLayoutAsFixedNumber(RebarData.ShortQuantity, totalDistShort, true, true, true);
                    }

                    // Thép chờ Cột & Thép đai
                    double colXFoot = UnitUtils.ConvertToInternalUnits(RebarData.ColumnWidthX, UnitTypeId.Millimeters);
                    double colYFoot = UnitUtils.ConvertToInternalUnits(RebarData.ColumnWidthY, UnitTypeId.Millimeters);
                    double starterHookFoot = UnitUtils.ConvertToInternalUnits(RebarData.StarterHookLength, UnitTypeId.Millimeters);

                    double zStarterBottom = zBottom + longDiaFoot + UnitUtils.ConvertToInternalUnits(RebarData.ShortDiameter, UnitTypeId.Millimeters);
                    double starterLenFoot = UnitUtils.ConvertToInternalUnits(RebarData.StarterLength, UnitTypeId.Millimeters) + Math.Abs(zStarterBottom);

                    XYZ starterBaseCenter = footingCenter + uZ * zStarterBottom;
                    XYZ corner1 = starterBaseCenter + uX * (colXFoot / 2) + uY * (colYFoot / 2);
                    XYZ corner2 = starterBaseCenter + uX * (-colXFoot / 2) + uY * (colYFoot / 2);
                    XYZ corner3 = starterBaseCenter + uX * (-colXFoot / 2) + uY * (-colYFoot / 2);
                    XYZ corner4 = starterBaseCenter + uX * (colXFoot / 2) + uY * (-colYFoot / 2);

                    XYZ[] columnCorners = { corner1, corner2, corner3, corner4 };
                    XYZ[] hookDirections = { uX, -uX, -uX, uX };
                    double deltaShortLenFoot = UnitUtils.ConvertToInternalUnits(180, UnitTypeId.Millimeters);

                    for (int i = 0; i < 4; i++)
                    {
                        XYZ cPt = columnCorners[i];
                        double currentStarterLen = starterLenFoot - ((i == 1 || i == 3) ? deltaShortLenFoot : 0);

                        List<Curve> sCurves = new() {
                            Line.CreateBound(cPt + hookDirections[i] * starterHookFoot, cPt),
                            Line.CreateBound(cPt, cPt + uZ * currentStarterLen)
                        };

                        if (starterBarType != null)
                        {
                            Rebar.CreateFromCurves(_doc, RebarStyle.Standard, starterBarType, null, null, columnHostElement, uY, sCurves, RebarHookOrientation.Right, RebarHookOrientation.Right, true, true);
                        }
                    }

                    CurveLoop stirrupLoop = new();
                    stirrupLoop.Append(Line.CreateBound(corner1, corner2));
                    stirrupLoop.Append(Line.CreateBound(corner2, corner3));
                    stirrupLoop.Append(Line.CreateBound(corner3, corner4));
                    stirrupLoop.Append(Line.CreateBound(corner4, corner1));

                    if (stirrupBarType != null && stirrupLoop.Count() > 0)
                    {
                        Rebar stirrupRebar = Rebar.CreateFromCurves(_doc, RebarStyle.StirrupTie, stirrupBarType, null, null, columnHostElement, uZ, stirrupLoop.ToList(), RebarHookOrientation.Right, RebarHookOrientation.Right, true, true);
                        if (stirrupRebar != null)
                        {
                            double stirrupSpcFoot = UnitUtils.ConvertToInternalUnits(RebarData.StirrupSpacing, UnitTypeId.Millimeters);
                            stirrupRebar.GetShapeDrivenAccessor().SetLayoutAsMaximumSpacing(stirrupSpcFoot, Math.Abs(zStarterBottom) + (stirrupSpcFoot * 2), true, true, true);
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
    }
}