using System;
using Microsoft.UI;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using ERManagementSystem.Models;

namespace ERManagementSystem.Helpers
{
    /// <summary>
    /// Task 5.14 — Maps ER_Room.Room_Type strings to color brushes for DataGrid display.
    /// Operating Room = Red, Trauma Bay = Orange, Respiratory = Blue,
    /// Neurology = Purple, Orthopedic = Green, General = Teal.
    /// </summary>
    public class RoomTypeColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is not string roomType)
                return new SolidColorBrush(Colors.Gray);

            return roomType switch
            {
                ER_Room.RoomType.OperatingRoom => new SolidColorBrush(Windows.UI.Color.FromArgb(255, 220, 38, 38)), // Red
                ER_Room.RoomType.TraumaBay => new SolidColorBrush(Windows.UI.Color.FromArgb(255, 234, 108, 10)), // Orange
                ER_Room.RoomType.RespiratoryRoom => new SolidColorBrush(Windows.UI.Color.FromArgb(255, 37, 100, 235)), // Blue
                ER_Room.RoomType.NeurologyRoom => new SolidColorBrush(Windows.UI.Color.FromArgb(255, 124, 58, 237)), // Purple
                ER_Room.RoomType.OrthopedicRoom => new SolidColorBrush(Windows.UI.Color.FromArgb(255, 22, 163, 74)), // Green
                ER_Room.RoomType.GeneralRoom => new SolidColorBrush(Windows.UI.Color.FromArgb(255, 13, 148, 136)), // Teal
                _ => new SolidColorBrush(Colors.Gray)
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
            => throw new NotImplementedException();
    }

    /// <summary>
    /// Task 5.14 — Maps Availability_Status to a status badge color.
    /// available = Green, occupied = Red, cleaning = Amber.
    /// </summary>
    public class RoomStatusColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is not string status)
                return new SolidColorBrush(Colors.Gray);

            return status switch
            {
                ER_Room.RoomStatus.Available => new SolidColorBrush(Windows.UI.Color.FromArgb(255, 22, 163, 74)), // Green
                ER_Room.RoomStatus.Occupied => new SolidColorBrush(Windows.UI.Color.FromArgb(255, 220, 38, 38)), // Red
                ER_Room.RoomStatus.Cleaning => new SolidColorBrush(Windows.UI.Color.FromArgb(255, 217, 119, 6)), // Amber
                _ => new SolidColorBrush(Colors.Gray)
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
            => throw new NotImplementedException();
    }

    /// <summary>
    /// Formats an integer Room ID into a display string "Room #[ID]".
    /// </summary>
    public class RoomIdConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is int id)
            {
                return $"Room #{id}";
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
            => throw new NotImplementedException();
    }

    /// <summary>
    /// Inverts a boolean value. True becomes False, False becomes True.
    /// </summary>
    public class BoolNegationConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return !(value is bool b && b);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return !(value is bool b && b);
        }
    }
}