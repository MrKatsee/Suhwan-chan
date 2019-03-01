using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyServer
{
    class MyUser
    {
        public static string DIRECTORY = Directory.GetCurrentDirectory() + "/Data/User";

        public static MyUser ParseData(string data)
        {
            return JsonConvert.DeserializeObject<MyUser>(data);
        }

        // 서버에서만 사용하는 부분

        public static void SaveUserData(MyUser userData)
        {
            Directory.CreateDirectory(DIRECTORY);
            string filePath = string.Format("{0}/{1}.dat", DIRECTORY, userData.ID);
            try
            {
                StreamWriter writer = new StreamWriter(filePath, false);
                writer.WriteLine(JsonConvert.SerializeObject(userData, Formatting.Indented));
                writer.Close();
            }
            catch (IOException)
            {
                return;
            }
            return;
        }

        public static MyUser LoadUserData(string id)
        {
            string filePath = string.Format("{0}/{1}.dat", DIRECTORY, id);
            string fileData = string.Empty;
            try
            {
                StreamReader streamReader = new StreamReader(filePath);
                fileData = streamReader.ReadToEnd();
                streamReader.Close();
            }
            catch (IOException) { }
            return JsonConvert.DeserializeObject<MyUser>(fileData);
        }

        public static bool CheckUserData(string id)
        {
            return File.Exists(string.Format("{0}/{1}.dat", DIRECTORY, id));
        }

        // 서버에서만 사용하는 부분

        public string ID { get; set; }
        public string PW { get; set; }
        public int Win { get; set; }
        public int Lose { get; set; }
        public float Rate
        {
            get
            {
                if (Win + Lose <= 0) return 100f;
                return (float)Win / (Win + Lose) * 100f;
            }
        }

        public bool IsAvaliable
        {
            get
            {
                if (!string.IsNullOrEmpty(ID) && !string.IsNullOrEmpty(PW)) return true;
                else return false;
            }
        }

        public MyUser()
        {
            ID = string.Empty;
            PW = string.Empty;
        }

        public MyUser(string _id, string _pw)
        {
            ID = _id;
            PW = _pw;
        }

        public bool CheckID(string _id)
        {
            return string.Equals(ID, _id);
        }

        public bool CheckPassword(string _pw)
        {
            return string.Equals(PW, _pw);
        }

        public string ToData()
        {
            return JsonConvert.SerializeObject(this, Formatting.None);
        }
    }
}
