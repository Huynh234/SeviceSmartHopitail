using SeviceSmartHopitail.Models.Health;

namespace SeviceSmartHopitail.Services.Profiles
{
    public class HealthAlertService
    {
        public string GetHeartRateAlert(int heartRate, PriWarning? pri)
        {
            int min = pri?.MinHeartRate ?? 60;
            int max = pri?.MaxHeartRate ?? 100;

            if (heartRate < min) return "Nhịp tim thấp (cảnh báo)";
            if (heartRate > max) return "Nhịp tim cao (cảnh báo)";
            return "Nhịp tim bình thường";
        }

        public string GetBloodSugarAlert(decimal? bloodSugar, PriWarning? pri)
        {
            if (!bloodSugar.HasValue) return "Không có dữ liệu";
            decimal min = pri?.MinBloodSugar ?? 70;
            decimal max = pri?.MaxBloodSugar ?? 140;

            if (bloodSugar < min) return "Đường huyết thấp (cảnh báo)";
            if (bloodSugar > max) return "Đường huyết cao (cảnh báo)";
            return "Đường huyết bình thường";
        }

        public string GetBloodPressureAlert(int? systolic, int? diastolic, PriWarning? pri)
        {
            if (!systolic.HasValue || !diastolic.HasValue)
                return "Không có dữ liệu";

            int minSys = pri?.MinSystolic ?? 90;
            int maxSys = pri?.MaxSystolic ?? 140;
            int minDia = pri?.MinDiastolic ?? 60;
            int maxDia = pri?.MaxDiastolic ?? 90;

            if (systolic < minSys || diastolic < minDia)
                return "Huyết áp thấp (cảnh báo)";
            if (systolic > maxSys || diastolic > maxDia)
                return "Huyết áp cao (cảnh báo)";
            return "Huyết áp bình thường";
        }

        public string GetSleepAlert(decimal? timeSleep, PriWarning? pri)
        {
            if (!timeSleep.HasValue) return "Không có dữ liệu";
            decimal min = pri?.MinSleep ?? 6;
            decimal max = pri?.MaxSleep ?? 10;

            if (timeSleep < min) return "Ngủ ít (cảnh báo)";
            if (timeSleep > max) return "Ngủ quá nhiều (cảnh báo)";
            return "Giấc ngủ bình thường";
        }
    }
}
