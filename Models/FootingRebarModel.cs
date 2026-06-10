using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AddinVeMong.Models
{
    public class FootingRebarModel : INotifyPropertyChanged
    {
        // 1. Thông số chung
        private int _cover = 50;
        public int Cover { get => _cover; set { _cover = value; OnPropertyChanged(); } }

        // 2. Thép cạnh dài
        private int _longDiameter = 12;
        public int LongDiameter { get => _longDiameter; set { _longDiameter = value; OnPropertyChanged(); } }
        private int _longSpacing = 150;
        public int LongSpacing { get => _longSpacing; set { _longSpacing = value; OnPropertyChanged(); } }
        private int _longQuantity = 10;
        public int LongQuantity { get => _longQuantity; set { _longQuantity = value; OnPropertyChanged(); } }
        private int _longHookLength = 150;
        public int LongHookLength { get => _longHookLength; set { _longHookLength = value; OnPropertyChanged(); } }

        // 3. Thép cạnh ngắn
        private int _shortDiameter = 12;
        public int ShortDiameter { get => _shortDiameter; set { _shortDiameter = value; OnPropertyChanged(); } }
        private int _shortSpacing = 150;
        public int ShortSpacing { get => _shortSpacing; set { _shortSpacing = value; OnPropertyChanged(); } }
        private int _shortQuantity = 8;
        public int ShortQuantity { get => _shortQuantity; set { _shortQuantity = value; OnPropertyChanged(); } }
        private int _shortHookLength = 150;
        public int ShortHookLength { get => _shortHookLength; set { _shortHookLength = value; OnPropertyChanged(); } }

        // 4. Thép cổ cột & Đai
        private int _starterDiameter = 18;
        public int StarterDiameter { get => _starterDiameter; set { _starterDiameter = value; OnPropertyChanged(); } }
        private int _starterHookLength = 250;
        public int StarterHookLength { get => _starterHookLength; set { _starterHookLength = value; OnPropertyChanged(); } }
        private int _starterLength = 600;
        public int StarterLength { get => _starterLength; set { _starterLength = value; OnPropertyChanged(); } }
        private int _stirrupDiameter = 6;
        public int StirrupDiameter { get => _stirrupDiameter; set { _stirrupDiameter = value; OnPropertyChanged(); } }
        private int _stirrupSpacing = 150;
        public int StirrupSpacing { get => _stirrupSpacing; set { _stirrupSpacing = value; OnPropertyChanged(); } }
        private int _columnWidthX = 300;
        public int ColumnWidthX { get => _columnWidthX; set { _columnWidthX = value; OnPropertyChanged(); } }
        private int _columnWidthY = 300;
        public int ColumnWidthY { get => _columnWidthY; set { _columnWidthY = value; OnPropertyChanged(); } }

        // 5. Thuộc tính lệch tâm (Dành riêng cho file Eccentric dùng chung sau này)
        private int _eccentricityX = 0;
        public int EccentricityX { get => _eccentricityX; set { _eccentricityX = value; OnPropertyChanged(); } }
        private int _eccentricityY = 0;
        public int EccentricityY { get => _eccentricityY; set { _eccentricityY = value; OnPropertyChanged(); } }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}