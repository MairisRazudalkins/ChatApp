using Newtonsoft.Json;
using System;
using System.IO;
using System.Security.Cryptography;
using User;

namespace Server
{
    [Serializable]
    public class User
    {
        private static readonly string usersPath = Directory.GetParent(Environment.CurrentDirectory).Parent.FullName + "\\Data\\Users\\";

        public LoginDetails loginDetails;
        public UserInfo info;

        private User() { }

        public User(LoginDetails loginDetails)
        {
            this.loginDetails = loginDetails;
            info = new UserInfo(loginDetails.userName, GenerateUniqueId());
        }

        public User(LoginDetails loginDetails, UserInfo info)
        {
            this.loginDetails = loginDetails;
            this.info = info;
        }

        public void SaveData()
        {
            File.WriteAllBytes(usersPath + this.loginDetails.userName + ".json", Encrypt(JsonConvert.SerializeObject(this)));

            //File.WriteAllText(usersPath + this.loginDetails.userName + ".json", JsonConvert.SerializeObject(this));
        }

        private byte[] Encrypt(string data) // Encryption src - https://www.c-sharpcorner.com/article/aes-encryption-in-c-sharp/
        {
            AesManaged aes = new AesManaged();
            ICryptoTransform cryptoTransform = aes.CreateEncryptor(aes.Key, aes.IV);

            byte[] bytes = null;

            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream cs = new CryptoStream(ms, cryptoTransform, CryptoStreamMode.Write))
                {
                    using (StreamWriter sw = new StreamWriter(cs))
                        sw.Write(data);
                    bytes = ms.ToArray();
                }
            }

            return bytes;
        }

        private string Decrypt(byte[] bytes) 
        {
            return "";
        }

        // TODO: Broadcast changes to other users. Maybe don't save data until the user disconnects or server shuts down.
        public void ChangeName(string newName) { this.info.name = newName; SaveData(); }
        public void ChangeImage(byte[] newImgData) { this.info.image = newImgData; SaveData(); }


        public static bool CreateAccount(LoginDetails loginDetails, UserInfo info, out User user)
        {
            info.uniqueId = GenerateUniqueId();
            return (user = !DoesUserExist(loginDetails.userName) ? new User(loginDetails, info) : null) != null;
        }

        public static User TryLoadUserInfo(string userName)
        {
            if (DoesUserExist(userName))
                return JsonConvert.DeserializeObject<User>(File.ReadAllText(usersPath + userName + ".json"));

            return null;
        }

        public static bool DoesUserExist(string userName)
        {
            return File.Exists(usersPath + userName + ".json");
        }

        private static int GenerateUniqueId() // Not safe but it will do.
        {
            Random rand = new Random();
            return rand.Next();
        }
    }
}
