using System;
using System.Threading.Tasks;

namespace speech
{
    public static class Constants
    {
        public static string Key = "7bd50b2629864e46ae5bd18e3a52dfac";
        public static string Region = "japanwest";
    }

    public interface IMicrophoneService
    {
        Task<bool> GetPermissionAsync();
        void OnRequestPermissionResult(bool isGranted);
    }
}
