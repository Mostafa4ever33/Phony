using Microsoft.Win32;
using Phony.Extensions;
using Phony.Kernel;
using Phony.Model;
using Phony.Utility;
using Phony.View;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Input;
using System.Windows.Media;

namespace Phony.ViewModel
{
    public class BarcodeVM : CommonBase
    {
        int _height;
        int _width;
        int _barWidth;
        double _aspectRatio;
        string _encodeValue;
        string _selectedRotate;
        string _alignment;
        string _foreground;
        string _background;
        string _alternateLabelText;
        string _labelLocation;
        string _encodedValue;
        string _selectedEncoder;
        byte[] _image;
        bool _generateLabel;

        List<string> _rotateTypes;
        List<Enumeration<byte>> _encoders;

        public int Height
        {
            get => _height;
            set
            {
                if (value != _height)
                {
                    _height = value;
                    RaisePropertyChanged();
                }
            }
        }

        public int Width
        {
            get => _width;
            set
            {
                if (value != _width)
                {
                    _width = value;
                    RaisePropertyChanged();
                }
            }
        }

        public int BarWidth
        {
            get => _barWidth;
            set
            {
                if (value != _barWidth)
                {
                    _barWidth = value;
                    RaisePropertyChanged();
                }
            }
        }

        public double AspectRatio
        {
            get => _aspectRatio;
            set
            {
                if (value != _aspectRatio)
                {
                    _aspectRatio = value;
                    RaisePropertyChanged();
                }
            }
        }

        public string EncodeValue
        {
            get => _encodeValue;
            set
            {
                if (value != _encodeValue)
                {
                    _encodeValue = value;
                    RaisePropertyChanged();
                }
            }
        }

        public string SelectedRotate
        {
            get => _selectedRotate;
            set
            {
                if (value != _selectedRotate)
                {
                    _selectedRotate = value;
                    RaisePropertyChanged();
                }
            }
        }

        public string Alignment
        {
            get => _alignment;
            set
            {
                if (value != _alignment)
                {
                    _alignment = value;
                    RaisePropertyChanged();
                }
            }
        }

        public string Foreground
        {
            get => _foreground;
            set
            {
                if (value != _foreground)
                {
                    _foreground = value;
                    RaisePropertyChanged();
                }
            }
        }

        public string Background
        {
            get => _background;
            set
            {
                if (value != _background)
                {
                    _background = value;
                    RaisePropertyChanged();
                }
            }
        }

        public string AlternateLabelText
        {
            get => _alternateLabelText;
            set
            {
                if (value != _alternateLabelText)
                {
                    _alternateLabelText = value;
                    RaisePropertyChanged();
                }
            }
        }

        public string LabelLocation
        {
            get => _labelLocation;
            set
            {
                if (value != _labelLocation)
                {
                    _labelLocation = value;
                    RaisePropertyChanged();
                }
            }
        }

        public string EncodedValue
        {
            get => _encodedValue;
            set
            {
                if (value != _encodedValue)
                {
                    _encodedValue = value;
                    RaisePropertyChanged();
                }
            }
        }

        public string SelectedEncoder
        {
            get => _selectedEncoder;
            set
            {
                if (value != _selectedEncoder)
                {
                    _selectedEncoder = value;
                    RaisePropertyChanged();
                }
            }
        }

        public bool GenerateLabel
        {
            get => _generateLabel;
            set
            {
                if (value != _generateLabel)
                {
                    _generateLabel = value;
                    RaisePropertyChanged();
                }
            }
        }

        public byte[] Image
        {
            get => _image;
            set
            {
                if (value != _image)
                {
                    _image = value;
                    RaisePropertyChanged();
                }
            }
        }

        public List<string> RotateTypes
        {
            get => _rotateTypes;
            set
            {
                if (value != _rotateTypes)
                {
                    _rotateTypes = value;
                    RaisePropertyChanged();
                }
            }
        }

        public List<Enumeration<byte>> Encoders
        {
            get => _encoders;
            set
            {
                if (value != _encoders)
                {
                    _encoders = value;
                    RaisePropertyChanged();
                }
            }
        }

        BarcodeLib.Barcode barCode = new BarcodeLib.Barcode();

        public ICommand SelectForeColor { get; set; }
        public ICommand SelectBackColor { get; set; }
        public ICommand Encode { get; set; }
        public ICommand Save { get; set; }

        public BarcodeVM()
        {
            Foreground = barCode.ForeColor.ToHexString();
            Background = barCode.BackColor.ToHexString();
            RotateTypes = new List<string>();
            foreach (var item in Enum.GetNames(typeof(RotateFlipType)))
            {
                if (item.ToString().Trim().ToLower() == "rotatenoneflipnone")
                {
                    continue;
                }
                RotateTypes.Add(item.ToString());
            }
            SelectedRotate = "Rotate180FlipXY";
            Encoders = new List<Enumeration<byte>>();
            foreach (var type in Enum.GetValues(typeof(BarCodeEncoders)))
            {
                Encoders.Add(new Enumeration<byte>
                {
                    Id = (byte)type,
                    Name = Enumerations.GetEnumDescription((BarCodeEncoders)type).ToString()
                });
            }
            SelectedEncoder = "Code 128";
            LoadCommands();
        }

        public void LoadCommands()
        {
            SelectForeColor = new CustomCommand(DoSelectForeColor, CanSelectForeColor);
            SelectBackColor = new CustomCommand(DoBackColor, CanBackColor);
            Encode = new CustomCommand(DoEncode, CanEncode);
            Save = new CustomCommand(DoSave, CanSave);
        }

        private bool CanSave(object obj)
        {
            if (string.IsNullOrWhiteSpace(EncodedValue))
            {
                return false;
            }
            return true;
        }

        private void DoSave(object obj)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "BMP (*.bmp)|*.bmp|GIF (*.gif)|*.gif|JPG (*.jpg)|*.jpg|PNG (*.png)|*.png|TIFF (*.tif)|*.tif";
            sfd.FilterIndex = 2;
            sfd.AddExtension = true;
            if (sfd.ShowDialog() == true)
            {
                BarcodeLib.SaveTypes savetype = BarcodeLib.SaveTypes.UNSPECIFIED;
                switch (sfd.FilterIndex)
                {
                    case 1: /* BMP */  savetype = BarcodeLib.SaveTypes.BMP; break;
                    case 2: /* GIF */  savetype = BarcodeLib.SaveTypes.GIF; break;
                    case 3: /* JPG */  savetype = BarcodeLib.SaveTypes.JPG; break;
                    case 4: /* PNG */  savetype = BarcodeLib.SaveTypes.PNG; break;
                    case 5: /* TIFF */ savetype = BarcodeLib.SaveTypes.TIFF; break;
                    default: break;
                }
                barCode.SaveImage(sfd.FileName, savetype);
            }
        }

        private bool CanEncode(object obj)
        {
            if (string.IsNullOrWhiteSpace(EncodeValue) || string.IsNullOrWhiteSpace(Alignment) || string.IsNullOrWhiteSpace(SelectedEncoder))
            {
                return false;
            }
            return true;
        }

        private void DoEncode(object obj)
        {
            int W = 202;
            int H = 101;
            if (Width > 0)
            {
                W = Width;
            }
            if (Height > 0)
            {
                H = Height;
            }
            switch (Alignment)
            {
                case "يمين": barCode.Alignment = BarcodeLib.AlignmentPositions.RIGHT; break;
                case "يسار": barCode.Alignment = BarcodeLib.AlignmentPositions.LEFT; break;
                default: barCode.Alignment = BarcodeLib.AlignmentPositions.CENTER; break;
            }
            BarcodeLib.TYPE type = BarcodeLib.TYPE.UNSPECIFIED;
            switch (SelectedEncoder)
            {
                case "UPC-A": type = BarcodeLib.TYPE.UPCA; break;
                case "UPC-E": type = BarcodeLib.TYPE.UPCE; break;
                case "UPC 2 Digit Ext.": type = BarcodeLib.TYPE.UPC_SUPPLEMENTAL_2DIGIT; break;
                case "UPC 5 Digit Ext.": type = BarcodeLib.TYPE.UPC_SUPPLEMENTAL_5DIGIT; break;
                case "EAN-13": type = BarcodeLib.TYPE.EAN13; break;
                case "JAN-13": type = BarcodeLib.TYPE.JAN13; break;
                case "EAN-8": type = BarcodeLib.TYPE.EAN8; break;
                case "ITF-14": type = BarcodeLib.TYPE.ITF14; break;
                case "Codabar": type = BarcodeLib.TYPE.Codabar; break;
                case "PostNet": type = BarcodeLib.TYPE.PostNet; break;
                case "Bookland/ISBN": type = BarcodeLib.TYPE.BOOKLAND; break;
                case "Code 11": type = BarcodeLib.TYPE.CODE11; break;
                case "Code 39": type = BarcodeLib.TYPE.CODE39; break;
                case "Code 39 Extended": type = BarcodeLib.TYPE.CODE39Extended; break;
                case "Code 39 Mod 43": type = BarcodeLib.TYPE.CODE39_Mod43; break;
                case "Code 93": type = BarcodeLib.TYPE.CODE93; break;
                case "LOGMARS": type = BarcodeLib.TYPE.LOGMARS; break;
                case "MSI": type = BarcodeLib.TYPE.MSI_Mod10; break;
                case "Interleaved 2 of 5": type = BarcodeLib.TYPE.Interleaved2of5; break;
                case "Standard 2 of 5": type = BarcodeLib.TYPE.Standard2of5; break;
                case "Code 128": type = BarcodeLib.TYPE.CODE128; break;
                case "Code 128-A": type = BarcodeLib.TYPE.CODE128A; break;
                case "Code 128-B": type = BarcodeLib.TYPE.CODE128B; break;
                case "Code 128-C": type = BarcodeLib.TYPE.CODE128C; break;
                case "Telepen": type = BarcodeLib.TYPE.TELEPEN; break;
                case "FIM": type = BarcodeLib.TYPE.FIM; break;
                case "Pharmacode": type = BarcodeLib.TYPE.PHARMACODE; break;
            }
            try
            {
                if (type != BarcodeLib.TYPE.UNSPECIFIED)
                {
                    try
                    {
                        barCode.BarWidth = BarWidth < 1 ? 2 : (int?)BarWidth;
                    }
                    catch (Exception ex)
                    {
                        Core.SaveException(ex);
                        BespokeFusion.MaterialMessageBox.Show("هناك مشكله تخص عرض الكود");
                    }
                    try
                    {
                        barCode.AspectRatio = AspectRatio < 1 ? null : (double?)AspectRatio;
                    }
                    catch (Exception ex)
                    {
                        Core.SaveException(ex);
                        BespokeFusion.MaterialMessageBox.Show("هناك مشكله تخص النسبة ");
                    }
                    barCode.IncludeLabel = GenerateLabel;
                    barCode.RotateFlipType = (RotateFlipType)Enum.Parse(typeof(RotateFlipType), SelectedRotate, true);

                    if (!String.IsNullOrEmpty(AlternateLabelText))
                    {
                        barCode.AlternateLabel = AlternateLabelText;
                    }
                    else
                    {
                        barCode.AlternateLabel = EncodeValue;
                    }
                    switch (LabelLocation)
                    {
                        case "اسفل - يمين": barCode.LabelPosition = BarcodeLib.LabelPositions.BOTTOMRIGHT; break;
                        case "اسفل - يسار": barCode.LabelPosition = BarcodeLib.LabelPositions.BOTTOMLEFT; break;
                        case "اعلى - وسط": barCode.LabelPosition = BarcodeLib.LabelPositions.TOPCENTER; break;
                        case "اعلى - يمين": barCode.LabelPosition = BarcodeLib.LabelPositions.TOPLEFT; break;
                        case "اعلى - يسار": barCode.LabelPosition = BarcodeLib.LabelPositions.TOPRIGHT; break;
                        default: barCode.LabelPosition = BarcodeLib.LabelPositions.BOTTOMCENTER; break;
                    }
                    //===== Encoding performed here =====
                    Image = barCode.Encode(type, EncodeValue, ColorTranslator.FromHtml(Foreground), ColorTranslator.FromHtml(Background), W, H).ImageToByteArray();
                    //===================================
                    EncodedValue = barCode.EncodedValue;
                    // Read dynamically calculated Width/Height because the user is interested.
                    if (barCode.BarWidth.HasValue)
                    {
                        Height = barCode.Height;
                        Width = barCode.Width;
                        BarWidth = (int)barCode.BarWidth;
                    }
                    if (barCode.AspectRatio.HasValue)
                    {
                        AspectRatio = (double)barCode.AspectRatio;
                    }
                }
            }
            catch (Exception ex)
            {
                Core.SaveException(ex);
            }
        }

        private bool CanSelectForeColor(object obj)
        {
            return true;
        }

        private void DoSelectForeColor(object obj)
        {
            ColorDialog colorDialog = new ColorDialog();
            colorDialog.SelectedColor = ((SolidColorBrush)(new BrushConverter().ConvertFrom(Foreground))).Color;
            if ((bool)colorDialog.ShowDialog())
            {
                Foreground = colorDialog.SelectedColor.ToHexString();
            }
        }

        private bool CanBackColor(object obj)
        {
            return true;
        }

        private void DoBackColor(object obj)
        {
            ColorDialog colorDialog = new ColorDialog();
            colorDialog.SelectedColor = ((SolidColorBrush)(new BrushConverter().ConvertFrom(Background))).Color;
            if ((bool)colorDialog.ShowDialog())
            {
                Background = colorDialog.SelectedColor.ToHexString();
            }
        }
    }
}