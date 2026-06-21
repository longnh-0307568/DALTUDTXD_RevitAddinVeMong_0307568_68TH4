$content = @'
using AddinVeMong.Commands;
using AddinVeMong.Models;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace AddinVeMong.ViewModels
{
    public class BarPreviewX
    {
        public double XPosition { get; set; }
    }

    public class BarPreviewY
    {
        public double YPosition { get; set; }
    }

    public class EccentricRebarViewModel : ConcentricRebarViewModel
    {
        private readonly Document _doc;
        private readonly List<Element> _selectedFootings;

        public FootingRebarModel Model { get; set; }

        public new ICommand DrawRebarCommand { get; }

        #region Properties Binding
        public int Cover
        {
            get => Model.Cover;
            set { Model.Cover = value; OnPropertyChanged(); }
        }

        public int LongDiameter
        {
            get => Model.LongDiameter;
            set { Model.LongDiameter = value; OnPropertyChanged(); }
        }

        public int LongSpacing
        {
            get => Model.LongSpacing;
            set { Model.LongSpacing = value; OnPropertyChanged(); }
        }

        public int LongQuantity
        {
            get => Model.LongQuantity;
            set { Model.LongQuantity = value; OnPropertyChanged(); UpdateLongBarsPreview(); }
        }

        public int LongHookLength
        {
            get => Model.LongHookLength;
            set { Model.LongHookLength = value; OnPropertyChanged(); }
        }

        public int ShortDiameter
        {
            get => Model.ShortDiameter;
            set { Model.ShortDiameter = value; OnPropertyChanged(); }
        }

        public int ShortSpacing
        {
            get => Model.ShortSpacing;
            set { Model.ShortSpacing = value; OnPropertyChanged(); }
        }

        public int ShortQuantity
        {
            get => Model.ShortQuantity;
            set { Model.ShortQuantity = value; OnPropertyChanged(); UpdateShortBarsPreview(); }
        }

        public int ShortHookLength
        {
            get => Model.ShortHookLength;
            set { Model.ShortHookLength = value; OnPropertyChanged(); }
        }

        public int ColumnWidthX
        {
            get => Model.ColumnWidthX;
            set { Model.ColumnWidthX = value; OnPropertyChanged(); }
        }

        public int ColumnWidthY
        {
            get => Model.ColumnWidthY;
            set { Model.ColumnWidthY = value; OnPropertyChanged(); }
        }

        public int StarterDiameter
        {
            get => Model.StarterDiameter;
            set { Model.StarterDiameter = value; OnPropertyChanged(); }
        }

        public int StarterHookLength
        {
            get => Model.StarterHookLength;
            set { Model.StarterHookLength = value; OnPropertyChanged(); }
        }

        public int StarterLength
        {
            get => Model.StarterLength;
            set { Model.StarterLength = value; OnPropertyChanged(); }
        }

        public int StirrupDiameter
        {
            get => Model.StirrupDiameter;
            set { Model.StirrupDiameter = value; OnPropertyChanged(); }
        }

        public int StirrupSpacing
        {
            get => Model.StirrupSpacing;
            set { Model.StirrupSpacing = value; OnPropertyChanged(); }
        }

        public int EccentricityX
        {
            get => Model.EccentricityX;
            set { Model.EccentricityX = value; OnPropertyChanged(); }
        }

        public int EccentricityY
        {
            get => Model.EccentricityY;
            set { Model.EccentricityY = value; OnPropertyChanged(); }
        }
        #endregion

        #region Preview Properties
        private System.Collections.ObjectModel.ObservableCollection<BarPreviewX> _previewShortBars;
        public System.Collections.ObjectModel.ObservableCollection<BarPreviewX> PreviewShortBars
        {
            get => _previewShortBars;
            set { _previewShortBars = value; OnPropertyChanged(); }
        }

        private System.Collections.ObjectModel.ObservableCollection<BarPreviewY> _previewLongBars;
        public System.Collections.ObjectModel.ObservableCollection<BarPreviewY> PreviewLongBars
        {
            get => _previewLongBars;
            set { _previewLongBars = value; OnPropertyChanged(); }
        }
        #endregion

        public EccentricRebarViewModel(ExternalCommandData commandData, List<Element> selectedFootings)
            : base(commandData, selectedFootings)
        {
            _doc = commandData.Application.ActiveUIDocument.Document;
            _selectedFootings = selectedFootings ?? new List<Element>();

            Model = new FootingRebarModel();

            PreviewShortBars = new System.Collections.ObjectModel.ObservableCollection<BarPreviewX>();
            PreviewLongBars = new System.Collections.ObjectModel.ObservableCollection<BarPreviewY>();

            DrawRebarCommand = new RelayCommand(ExecuteDrawRebar);

            UpdateShortBarsPreview();
            UpdateLongBarsPreview();
        }

        private void UpdateShortBarsPreview()
        {
            if (PreviewShortBars == null) return;
            PreviewShortBars.Clear();

            int quantity = ShortQuantity;
            if (quantity <= 1) return;

            double startX = 5;
            double endX = 265;
            double step = (endX - startX) / (quantity - 1);

            for (int i = 0; i < quantity; i++)
            {
                PreviewShortBars.Add(new BarPreviewX { XPosition = startX + (i * step) });
            }
        }

        private void UpdateLongBarsPreview()
        {
            if (PreviewLongBars == null) return;
            PreviewLongBars.Clear();

            int quantity = LongQuantity;
            if (quantity <= 1) return;

            double startY = 5;
            double endY = 175;
            double step = (endY - startY) / (quantity - 1);

            for (int i = 0; i < quantity; i++)
            {
                PreviewLongBars.Add(new BarPreviewY { YPosition = startY + (i * step) });
            }
        }

        private void ExecuteDrawRebar(object parameter)
        {
            try
            {
                if (_selectedFootings == null || _selectedFootings.Count == 0)
                {
                    TaskDialog.Show("Thông báo", "Không t?m th?y d? li?u móng ðý?c ch?n.");
                    return;
                }

                View3D activeView3D = _doc.ActiveView as View3D;

                using (Transaction trans = new Transaction(_doc, "V? thép móng l?ch tâm"))
                {
                    trans.Start();

                    var barTypes = new FilteredElementCollector(_doc)
                        .OfClass(typeof(RebarBarType))
                        .Cast<RebarBarType>()
                        .ToList();

                    if (barTypes.Count == 0)
                    {
                        TaskDialog.Show("Thi?u d? li?u", "D? án chýa ðý?c t?i Family thép.");
                        trans.RollBack();
                        return;
                    }

                    RebarBarType starterBarType = barTypes.FirstOrDefault(t => t.Name.Contains(this.StarterDiameter.ToString()) || Math.Abs(UnitUtils.ConvertFromInternalUnits(t.get_Parameter(BuiltInParameter.REBAR_BAR_DIAMETER).AsDouble(), UnitTypeId.Millimeters) - this.StarterDiameter) < 0.1);
                    RebarBarType stirrupBarType = barTypes.FirstOrDefault(t => t.Name.Contains(this.StirrupDiameter.ToString()) || Math.Abs(UnitUtils.ConvertFromInternalUnits(t.get_Parameter(BuiltInParameter.REBAR_BAR_DIAMETER).AsDouble(), UnitTypeId.Millimeters) - this.StirrupDiameter) < 0.1);

                    if (starterBarType == null) starterBarType = barTypes.FirstOrDefault();
                    if (stirrupBarType == null) stirrupBarType = barTypes.FirstOrDefault();

                    RebarStyle styleStandard = RebarStyle.Standard;
                    RebarStyle styleStirrup = RebarStyle.StirrupTie;

                    foreach (Element footingElement in _selectedFootings)
                    {
                        FamilyInstance footingInstance = footingElement as FamilyInstance;
                        if (footingInstance == null) continue;

                        Element columnElement = null;
                        var boundingBox = footingElement.get_BoundingBox(null);
                        if (boundingBox != null)
                        {
                            XYZ tolerance = new XYZ(0.1, 0.1, 2.0);
                            Outline outline = new Outline(boundingBox.Min, boundingBox.Max + tolerance);
                            BoundingBoxIntersectsFilter boxFilter = new BoundingBoxIntersectsFilter(outline);

                            var connectedColumns = new FilteredElementCollector(_doc)
                                .OfCategory(BuiltInCategory.OST_StructuralColumns)
                                .OfClass(typeof(FamilyInstance))
                                .WherePasses(boxFilter)
                                .ToList();

                            if (connectedColumns.Count > 0) columnElement = connectedColumns.First();
                        }

                        Element columnHostElement = columnElement ?? footingElement;

                        ElementType footingType = _doc.GetElement(footingElement.GetTypeId()) as ElementType;
                        if (footingType == null) continue;

                        Parameter pLength = footingType.LookupParameter("Length");
                        Parameter pWidth = footingType.LookupParameter("Width");
                        Parameter pHeight = footingType.LookupParameter("Foundation Thickness") ?? footingType.LookupParameter("Thickness");

                        if (pLength == null || pWidth == null || pHeight == null) continue;

                        double footingLengthMm = UnitUtils.ConvertFromInternalUnits(pLength.AsDouble(), UnitTypeId.Millimeters);
                        double footingWidthMm = UnitUtils.ConvertFromInternalUnits(pWidth.AsDouble(), UnitTypeId.Millimeters);
                        double footingHeightMm = UnitUtils.ConvertFromInternalUnits(pHeight.AsDouble(), UnitTypeId.Millimeters);

                        double coverFoot = UnitUtils.ConvertToInternalUnits(this.Cover, UnitTypeId.Millimeters);
                        double lengthFoot = UnitUtils.ConvertToInternalUnits(footingLengthMm, UnitTypeId.Millimeters);
                        double widthFoot = UnitUtils.ConvertToInternalUnits(footingWidthMm, UnitTypeId.Millimeters);
                        double heightFoot = UnitUtils.ConvertToInternalUnits(footingHeightMm, UnitTypeId.Millimeters);

                        Transform tf = footingInstance.GetTransform();
                        XYZ uX = tf.BasisX;
                        XYZ uY = tf.BasisY;
                        XYZ uZ = tf.BasisZ;
                        XYZ footingCenter = tf.Origin;

                        double zBottom = -heightFoot + coverFoot;
                        double longDiaFoot = UnitUtils.ConvertToInternalUnits(this.LongDiameter, UnitTypeId.Millimeters);

                        double colXFoot = UnitUtils.ConvertToInternalUnits(this.ColumnWidthX, UnitTypeId.Millimeters);
                        double colYFoot = UnitUtils.ConvertToInternalUnits(this.ColumnWidthY, UnitTypeId.Millimeters);
                        double eccXFoot = UnitUtils.ConvertToInternalUnits(this.EccentricityX, UnitTypeId.Millimeters);
                        double eccYFoot = UnitUtils.ConvertToInternalUnits(this.EccentricityY, UnitTypeId.Millimeters);

                        double starterHookFoot = UnitUtils.ConvertToInternalUnits(this.StarterHookLength, UnitTypeId.Millimeters);
                        double starterLenFoot = UnitUtils.ConvertToInternalUnits(this.StarterLength, UnitTypeId.Millimeters);

                        double zStarterBottom = zBottom + longDiaFoot + UnitUtils.ConvertToInternalUnits(this.ShortDiameter, UnitTypeId.Millimeters);

                        XYZ columnCenter = footingCenter + (uX * eccXFoot) + (uY * eccYFoot);
                        XYZ starterBaseCenter = columnCenter + uZ * zStarterBottom;

                        XYZ corner1 = starterBaseCenter + uX * (colXFoot / 2) + uY * (colYFoot / 2);
                        XYZ corner2 = starterBaseCenter + uX * (-colXFoot / 2) + uY * (colYFoot / 2);
                        XYZ corner3 = starterBaseCenter + uX * (-colXFoot / 2) + uY * (-colYFoot / 2);
                        XYZ corner4 = starterBaseCenter + uX * (colXFoot / 2) + uY * (-colYFoot / 2);

                        List<XYZ> columnCorners = new List<XYZ> { corner1, corner2, corner3, corner4 };
                        List<XYZ> hookDirections = new List<XYZ> { uX, -uX, -uX, uX };
                        List<XYZ> starterNormals = new List<XYZ> { uY, uY, uY, uY };

                        for (int i = 0; i < 4; i++)
                        {
                            XYZ cPt = columnCorners[i];
                            XYZ hDir = hookDirections[i];
                            XYZ sNormal = starterNormals[i];

                            List<Curve> sCurves = new List<Curve>();
                            XYZ pB?Chân = cPt + hDir * starterHookFoot;
                            XYZ pÐ?nhCh? = cPt + uZ * (starterLenFoot + Math.Abs(zStarterBottom));

                            sCurves.Add(Line.CreateBound(pB?Chân, cPt));
                            sCurves.Add(Line.CreateBound(cPt, pÐ?nhCh?));

                            if (starterBarType != null)
                            {
                                Rebar starterRebar = Rebar.CreateFromCurves(_doc, styleStandard, starterBarType, null, null, columnHostElement, sNormal, sCurves, RebarHookOrientation.Right, RebarHookOrientation.Right, true, true);
                                if (starterRebar != null) SetRebarSolid3D(starterRebar, activeView3D);
                            }
                        }

                        CurveLoop stirrupLoop = new CurveLoop();
                        stirrupLoop.Append(Line.CreateBound(corner1, corner2));
                        stirrupLoop.Append(Line.CreateBound(corner2, corner3));
                        stirrupLoop.Append(Line.CreateBound(corner3, corner4));
                        stirrupLoop.Append(Line.CreateBound(corner4, corner1));

                        if (stirrupBarType != null && stirrupLoop.Count() > 0)
                        {
                            List<Curve> finalStirrupProfile = stirrupLoop.ToList();
                            Rebar stirrupRebar = Rebar.CreateFromCurves(_doc, styleStirrup, stirrupBarType, null, null, columnHostElement, uZ, finalStirrupProfile, RebarHookOrientation.Right, RebarHookOrientation.Right, true, true);

                            if (stirrupRebar != null)
                            {
                                double stirrupSpcFoot = UnitUtils.ConvertToInternalUnits(this.StirrupSpacing, UnitTypeId.Millimeters);
                                double heightInsideFooting = Math.Abs(zStarterBottom);
                                double heightOutsideFooting = stirrupSpcFoot * 2;
                                double totalStirrupDistributionLength = heightInsideFooting + heightOutsideFooting;

                                stirrupRebar.GetShapeDrivenAccessor().SetLayoutAsMaximumSpacing(stirrupSpcFoot, totalStirrupDistributionLength, true, true, true);
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
                TaskDialog.Show("L?i th?c thi", "Có l?i x?y ra: " + ex.Message);
            }
        }
    }
}
'@
Set-Content -Path ViewModels\EccentricRebarViewModel.cs -Value $content
